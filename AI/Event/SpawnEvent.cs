using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 스폰에 필요한 항목 구조체
[System.Serializable]
public struct SpawnInfo
{
    public SpawnActionTime spawnActionTime;     // 현재 스폰 즉시하는지
    [HideInInspector] public Transform tr;      // 포인트 장소
    public short prefabID;                        // 프리팹 넘버(*DB)
    //[HideInInspector] public NetworkHash128 assetid;

    public float delayTime;                     // 어느 시간 이후에 소환할건지

    [Header("[스폰 수 조정]")]
    public short spawnNum;                        // 한번에 소환할 마리 수
    public short spawnCount;                      // 횟수
    public float spawnTerm;                     // 다음 스폰 대기시간
}

public class SpawnEvent : AIEvent
{
    public static int spawnEventsID = 0;                    // 이벤트 ID 부여해줄 변수(0번부터 증가)
    //                  spawnEventID, this                스폰이벤트의 번호가 담겨져있는 딕셔너리
    public static Dictionary<short, SpawnEvent> spawnEventDictionary = new Dictionary<short, SpawnEvent>();

    public short spawnEventID;                                // 이 이벤트의 아이디
    //              aiID, totalNum = spawnNum * spawnCount
    public Dictionary<short, short> totalCountDictionary;       // 이벤트마다 개수 (ai아이디, 총 개수) - 서버에게 보내기
    //              aiID, netCode
    public Dictionary<short, Queue<short>> netcodeDictionary;   // 아이디를 찾아서 넷코드를 부여하는 딕셔너리 (ai아이디, 넷코드) - 서버에서 보내줌

    public SpawnInfo[] spawnInfo;                           // 본인의 스폰정보
    SpawnPooling spawnPooling;

    /// <summary>
    /// 0. 현재 스폰이 가진 ai의 총 개수 넣기
    /// 1. 트리거 스포너 고유 번호 지정 후 딕셔너리에 넣기
    /// 2. 하위 스포너들 가져와서 ai마다 총개수 확인해서 spawnDictionary에 넣기
    /// (spawnNum * spawnCount)
    /// </summary>
    private void Awake()
    {
        // 0.
        totalCountDictionary = new Dictionary<short, short>();
        // Debug.Log(spawnInfo.Length);
        for (int i = 0; i < spawnInfo.Length; i++)
        {
            if (totalCountDictionary.ContainsKey(spawnInfo[i].prefabID))
            {
                totalCountDictionary[spawnInfo[i].prefabID] += (short)(spawnInfo[i].spawnNum * spawnInfo[i].spawnCount);
            }
            else
            {
                totalCountDictionary.Add(spawnInfo[i].prefabID, (short)(spawnInfo[i].spawnNum * spawnInfo[i].spawnCount));
            }
        }

        netcodeDictionary = new Dictionary<short, Queue<short>>();

        // 1.
        // spawnEventID = spawnEventsID++;
        spawnEventDictionary.Add(spawnEventID, this);

    }

    /// <summary>
    /// 소환 위치 지정
    /// </summary>
    private void Start()
    {
        spawnPooling = GetComponent<SpawnPooling>();

        for (int i = 0; i < spawnInfo.Length; i++)
        {
            spawnInfo[i].tr = gameObject.transform;
            // Debug.Log("스폰 포인트 : " + aiSpawn[i].tr.position);
        }
    }

    /// <summary>
    /// 서버에 스폰 요청하기
    /// 0번 실행 : 이후에는 풀링 내부에서 실행하게 함
    /// </summary>
    public override void Play()
    {
        InGameManager.Instance.TransmitSpawnerEvent(this);
    }

    public void SpawnStart()
    {
        spawnPlay(0);
    }

    /// <summary>
    /// 1. num번은 바로 스폰하기
    /// 2. 다음번호의 스폰 즉시여부에 따라 바로 할지 나눔
    /// </summary>
    /// <param name="num">num번 스폰 실행</param>
    public void spawnPlay(int num)
    {
        // 1.
        spawnPooling.Pooling(spawnInfo[num], num++);

        // 2.
        for (int i = num; i < spawnInfo.Length; i++)
        {  
            if (spawnInfo[i].spawnActionTime == SpawnActionTime.SEQUENCE)
                break;
            else
                spawnPooling.Pooling(spawnInfo[i], i);
        }
    }

    /// <summary>
    /// 딕셔너리 value 개수만큼 큐에 넣고 딕셔너리에 큐값 저장하기
    /// ???
    /// </summary>
    /// <param name="netcode"></param>
    public void NetcodeSetting(short[] netcode)
    {
        int count = 0;

        foreach (KeyValuePair<short, short> pair in totalCountDictionary)
        {
            Queue<short> queue = new Queue<short>();
            for (short i = 0; i < pair.Value; i++)
            {
                queue.Enqueue(netcode[count++]);
            }
            netcodeDictionary.Add(pair.Key, queue);
        }
    }

    /// <summary>
    /// 이벤트 트리거 찾기
    /// </summary>
    /// <param name="eventNum">트리거 번호</param>
    /// <returns></returns>
    public static SpawnEvent FindSpawnEvent(short eventNum)
    {
        return spawnEventDictionary[eventNum];
    }
}

