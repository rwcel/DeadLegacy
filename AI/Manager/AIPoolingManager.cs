using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Global;
//Netcode short로 변경 Netcode 부분 Short로 변경할것
// 매니저에서 풀링, 스폰줄 때 필요한 정보 구조체
[System.Serializable]
public struct AIPool
{
    // [Header("전체 AI 설정")]
    public GameObject prefab;           // ai 객체 종류
    public short id;                    // ai 번호
    public short max;                   // 만들 ai수
    public AIInfo aiInfo;               // Ai정보
}

[System.Serializable]
public struct DeathPool
{
    public GameObject deathPrefab;  // 죽었을 때 남길 시체 오브젝트
    public int max;                 // 만들 ai수

    [HideInInspector] public List<GameObject> deathPool;
}

[System.Serializable]
public class SerializableDictionary<TKey, TValue> : Dictionary<TKey, TValue>, ISerializationCallbackReceiver
{
    [SerializeField] private List<TKey> keys = new List<TKey>();
    [SerializeField] private List<TValue> values = new List<TValue>();

    public void OnAfterDeserialize()
    {
        this.Clear();

        if (keys.Count != values.Count)
            throw new System.Exception(string.Format("there are {0} keys and {1} values after deserialization. Make sure that both key and value types are serializable."));

        for (int i = 0; i < keys.Count; i++)
            this.Add(keys[i], values[i]);
    }

    public void OnBeforeSerialize()
    {
        keys.Clear();
        values.Clear();
        foreach (KeyValuePair<TKey, TValue> pair in this)
        {
            keys.Add(pair.Key);
            values.Add(pair.Value);
        }
    }
}

[System.Serializable] public class SerializeDictionary : SerializableDictionary<short, GameObject> { }

// 몬스터 스폰 스크립트
public class AIPoolingManager : MonoBehaviour
{
    public SerializeDictionary activelDictionary;

    [Header("[데칼]")]
    [SerializeField] GameObject bloodDecal;         // 피 데칼
    [SerializeField] GameObject overkillParticle;   // 오버킬 파티클 
    [SerializeField] int m_bloodMax = 30;           // 피 파티클 개수
    float m_bloodTime = 5f;                         // 피 흘리는 시간
    Queue<GameObject> bloodQueue;
    Queue<GameObject> deathParticleQueue;

    [Header("[적 AI]")] public AIPool[] aiPools;
    [Header("[시체]")] public DeathPool[] deathPools;

    int m_minZombieNum = 2;                         // 최소 좀비 백의자리 번호

    InGameUserInfo inGameUserInfo;                  // 게임 정보


    //                id      오브젝트 풀
    public Dictionary<short, Queue<GameObject>> poolDictionary;
    //                id  배열번호
    public Dictionary<short, int> poolAIID;
    //                netcode, 오브젝트
    // public Dictionary<short, GameObject> activelDictionary;

    private static AIPoolingManager _instance = null;   // 싱글톤
    // AIPoolingManager가 없다면 경고메시지 출력
    public static AIPoolingManager instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(AIPoolingManager)) as AIPoolingManager;
                if (_instance == null)
                    Debug.LogError("풀링 매니저가 없습니다");
            }
            return _instance;
        }
    }

    /// <summary>
    /// 호스트/게스트 확인 + 풀링 실행하기
    /// </summary>
    private void Awake()
    {
        SetIngameUserInfo();
        AIPooling();
        DeathPooling();
        BloodPooling();
    }

    // 호스트인지 게스트인지 확인하기
    public void SetIngameUserInfo()
    {
        inGameUserInfo = RoomManager.Instance.m_MyInfo.inGameUserInfo;
    }

    /// <summary>
    /// 1. 풀링할것들 순서대로 id 집어넣기 (dequeue할 때 for문으로 배열번호 조사할 필요 없게)
    /// 2. 스폰대상 객체 수만큼 큐 생성하기
    /// 3. 식별번호 생성
    /// </summary>
    void AIPooling()
    {
        // activelDictionary = new Dictionary<short, GameObject>();

        poolDictionary = new Dictionary<short, Queue<GameObject>>();
        poolAIID = new Dictionary<short, int>();

        // 1.
        for (int i = 0; i < aiPools.Length; i++)
        {
            poolAIID.Add(aiPools[i].id, i);
        }

        // 2.
        for (int i = 0; i < aiPools.Length; i++)
        {
            Queue<GameObject> _objectPool = new Queue<GameObject>();
            aiPools[i].aiInfo = CSV_AI.instance.aiDictionary[aiPools[i].id];
            // Debug.Log(pools[i].id + "," + pools[i].max);

            for (int j = 0; j < aiPools[i].max; j++)
            {
                GameObject _obj = InstantiatePrefab(aiPools[i].prefab, i);
                _obj.name = aiPools[i].prefab.name + "_" + j.ToString();
                _obj.SetActive(false);
                _objectPool.Enqueue(_obj);

            }
            poolDictionary.Add(aiPools[i].id, _objectPool);
        }
    }
    /// <summary>
    /// 시체 풀링
    /// 1. 100번대마다 종류가 바뀌므로 같은 시체 사용하기
    /// 2. 100번대는(트랩종류) 해당 안되게 하기
    /// </summary>
    void DeathPooling()
    {
        for (int i = 0; i < deathPools.Length; i++)
        {
            deathPools[i].deathPool = new List<GameObject>();
            deathPools[i].max = 0;
        }

        for (int i = 0; i < aiPools.Length; i++)
        {
            // 1.
            int deathNum = (int)(aiPools[i].id * 0.01) - m_minZombieNum;
            // 2.
            if (deathNum >= 0 && deathNum < deathPools.Length)
            {
                deathPools[deathNum].max += aiPools[i].max;
            }
        }

        GameObject _deathParent = new GameObject();
        _deathParent.name = "Death_Pool";

        for (int i = 0; i < deathPools.Length; i++)
        {
            for (int j = 0; j < deathPools[i].max; j++)
            {
                GameObject _deathObj = Instantiate(deathPools[i].deathPrefab, transform.position, Quaternion.identity);
                _deathObj.name = deathPools[i].deathPrefab.name + "_" + j.ToString();
                _deathObj.transform.SetParent(_deathParent.transform);

                _deathObj.SetActive(false);
                deathPools[i].deathPool.Add(_deathObj);
            }
        }
    }

    // 데칼 풀링
    void BloodPooling()
    {
        // bloodQueue = new Queue<GameObject>();
        deathParticleQueue = new Queue<GameObject>();

        GameObject _bloodParent = new GameObject();
        _bloodParent.name = "Decal_Pool";

        for (int i = 0; i < m_bloodMax; i++)
        {
            //GameObject _decalObj = Instantiate(bloodDecal, transform.position, Quaternion.identity);
            //_decalObj.name = bloodDecal.name + "_" + i.ToString();
            //_decalObj.transform.SetParent(_bloodParent.transform);
            //_decalObj.SetActive(false);
            //bloodQueue.Enqueue(_decalObj);

            GameObject _overkillParticle = Instantiate(overkillParticle, transform.position, Quaternion.identity);
            _overkillParticle.name = overkillParticle.name + "_" + i.ToString();
            _overkillParticle.transform.SetParent(_bloodParent.transform);
            _overkillParticle.SetActive(false);
            deathParticleQueue.Enqueue(_overkillParticle);
        }
    }

    /// <summary>
    /// 프리팹 생성
    /// 1. AI인지 소환수인지에 다르게 함
    /// </summary>
    /// <param name="prefab">소환할 프리팹</param>
    /// <param name="num">풀 번호</param>
    /// <returns></returns>
    GameObject InstantiatePrefab(GameObject prefab, int num)
    {
        GameObject _obj = Instantiate(prefab, transform.position, Quaternion.identity);
        // 1. 
        if (_obj.GetComponent<AIController>() != null)
            _obj.GetComponent<AIController>().aiInfo = aiPools[num].aiInfo;
        else
            _obj.GetComponent<SummonController>().aiInfo = aiPools[num].aiInfo;

        _obj.transform.parent = this.transform;
        return _obj;
    }

    /// <summary>
    /// 시체 똑같이 소환해서 날아가게 하기
    /// 1. 배열 번호 찾기 : deathPools에 있어서 첫 배열의 번호 이용
    /// 2. 옷 색상, 트랜스폼 적용
    /// 3. 날아가는 힘 설정하기
    /// </summary>
    /// <param name="origin"></param>
    /// <param name="id"></param>
    /// <param name="force"></param>
    public void ChangeDeathAI(GameObject origin, int id, Vector3 force)
    {
        // 1.
        int _num = (id / 100) - m_minZombieNum;
        // Debug.Log("_num = " + _num);
        for (int i = 0; i < deathPools[_num].deathPool.Count; i++)
        {
            GameObject _obj = deathPools[_num].deathPool[i];
            DeathAIController _objController = _obj.GetComponent<DeathAIController>();
            if (!_obj.activeSelf)
            {
                // 2.
                _objController.SettingForce(force);
                _objController.CopyAppearance(origin);
                _obj.SetActive(true);
                break;
            }
        }
    }

    // 시체 전부 지우기
    public void DeathAISetFalse()
    {
        for (int i = 0; i < deathPools.Length; i++)
        {
            for (int j = 0; j < deathPools[i].deathPool.Count; j++)
            {
                if (deathPools[i].deathPool[j].activeSelf)
                {
                    deathPools[i].deathPool[j].GetComponent<DeathAIController>().DeadAIReset();
                    deathPools[i].deathPool[j].SetActive(false);
                }
            }
        }
    }

    // 바닥에 피흘리기
    public void FloorBlood(Vector3 pos, Quaternion rot)
    {
        GameObject _bloodDecal = null;
        // 뽑을 개수가 남아있다면
        Debug.Log(bloodQueue.Count);
        if (bloodQueue.Count > 0)
        {
            _bloodDecal = bloodQueue.Dequeue();
            _bloodDecal.transform.position = pos;
            _bloodDecal.transform.rotation = rot;
            _bloodDecal.transform.localScale = new Vector3(Random.Range(0.4f, 1.6f), 1f, Random.Range(0.4f, 1.6f));
            _bloodDecal.SetActive(true);

            StartCoroutine(EnqueueDecal(_bloodDecal));
        }
    }

    /// <summary>
    /// 오버킬 파티클 실행
    /// </summary>
    /// <param name="pos">위치</param>
    /// <param name="rot">각도</param>
    public void PlayerOverkillParticle(Vector3 pos, Quaternion rot)
    {
        GameObject _bloodParticle = null;

        // Debug.Log(deathParticleQueue.Count);
        if (deathParticleQueue.Count > 0)
        {
            _bloodParticle = deathParticleQueue.Dequeue();
            _bloodParticle.transform.position = pos + new Vector3(0, 2f, 0);
            _bloodParticle.transform.rotation = rot;
            _bloodParticle.SetActive(true);
            _bloodParticle.GetComponent<ParticleSystem>().Play();

            StartCoroutine(EnqueueBloodParticle(_bloodParticle));
        }
    }

    // 데칼 일정시간동안 보여주고 풀링에 넣기
    IEnumerator EnqueueDecal(GameObject _bloodDecal)
    {
        yield return new WaitForSeconds(m_bloodTime);
        _bloodDecal.transform.localScale = Vector3.one;
        _bloodDecal.SetActive(false);
        bloodQueue.Enqueue(_bloodDecal);
    }

    IEnumerator EnqueueBloodParticle(GameObject _bloodParticle)
    {
        yield return new WaitForSeconds(m_bloodTime);
        _bloodParticle.SetActive(false);
        deathParticleQueue.Enqueue(_bloodParticle);
    }

    // netcode를 받아서 해당 좀비를 return : 서버에서 부르는 용도
    public GameObject GetActiveZombie(short netcode)
    {
        return activelDictionary[netcode];
    }

    /// <summary>
    /// 오브젝트 스폰하기
    /// 1. 해당 번호 좀비를 큐에서 꺼내고
    /// 2. 좀비에게 넷코드 부여  **서버 코드 없어서 오류남
    /// 3. pos, rot 설정 후 필드에 활성화
    /// 4. activeDictionary에 Add하기
    /// </summary>
    /// <param name="idcode"></param>
    /// <param name="netcode"></param>
    /// <param name="pos"></param>
    /// <param name="rot"></param>
    /// <param name="anim"></param>
    public void SpawnObject(short idcode, short netcode, Vector3 pos, float rot)
    {
        // 1.
        GameObject _obj = poolDictionary[idcode].Dequeue();
        // 2.
        CNetworkIdentity _networkIdentity;
        _networkIdentity = _obj.GetComponent<CNetworkIdentity>();
        _networkIdentity.Netcode = netcode;

        _obj.GetComponent<AIController>().m_NetCode = netcode;
        // 3.
        _obj.transform.position = pos;
        _obj.transform.eulerAngles = new Vector3(0,rot,0);

        // 4.
        activelDictionary.Add(netcode, _obj);
        _obj.SetActive(true);
    }

    /// <summary>
    /// 풀에 다시 등록하기
    /// 1. 오브젝트 끄기
    /// 2. 큐에 다시 넣어주기(poolDictionary)
    /// 3. activeDictionary에서는 빼기
    /// </summary>
    /// <param name="netcode"></param>
    public void UnspawnObject(short netcode)
    {
        GameObject _obj = activelDictionary[netcode];
        short _idcode = _obj.GetComponent<AIController>().aiInfo.id;

        // 1.
        _obj.SetActive(false);
        // 2.
        poolDictionary[_idcode].Enqueue(_obj);

        Debug.Log(_idcode + "지워짐");
        // 3.
        activelDictionary.Remove(netcode);
    }

    /// <summary>
    /// SummonSkill 썼을 때 스폰되는 것 : 넷코드 부여할 필요 없음
    /// </summary>
    /// <param name="idcode"></param>
    /// <param name="pos"></param>
    public void SpawnObject(short idcode, Vector3 pos)
    {
        GameObject _obj = poolDictionary[idcode].Dequeue();
        _obj.transform.position = pos;
        _obj.SetActive(true);
    }

    public void UnSpawnObject(short idcode, GameObject obj)
    {
        obj.SetActive(false);
        poolDictionary[idcode].Enqueue(obj);
    }

    public void AllUnSpawnObject()
    {
        foreach (KeyValuePair<short, GameObject> _activeZombie in activelDictionary)
        {
            _activeZombie.Value.SetActive(false);
            poolDictionary[_activeZombie.Value.GetComponent<AIController>().aiInfo.id].Enqueue(_activeZombie.Value);
        }
        activelDictionary.Clear();
    }
}
