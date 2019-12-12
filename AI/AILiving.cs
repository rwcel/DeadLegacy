using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using HighlightingSystem;

public class AILiving : LivingEntity
{
    // 외부 참조항목
    AIController aiCon;
    AIAggro aiAggro;
    AIInfo aiInfo;
    AIAnim aiAnim;
    CNetworkIdentity cNetworkIdentity;

    // 내부 참조항목
    Rigidbody rd;
    Collider col;
    NavMeshAgent agent;
    Highlighter highlighter;

    protected LayerMask enviroLayer;                // 환경레이어

    bool isDie;
    public bool isDieP
    {
        get { return isDie; }
        set { isDie = value; }
    }

    // 머리티얼 넣어줄 오브젝트 
    public Renderer[] renderObject;

    Vector3 m_takeForce;                              // 플레이어로부터 벡터 받아오기
    float m_takeForcePower = 300f;                    // 밀려나는 힘  **무기데미지 곱 연산
    float m_decalDist = 3f;                           // 데칼 맞는 거리

    // 데미지 받을 때 변수
    // [SerializeField] ParticleSystem bloodParticle;  // 피 파티클 : 오버데미지를 맞았을 때 발동
    float m_redTime = 0.1f;                           // 몸이 빨갛게 변하는 시간
    float m_shockTime = 0.6f;                         // 정지 시간

    /// <summary>
    /// AIController로부터 데이터 가져오기 + AIAnim
    /// 값 세팅하기
    /// </summary>
    void Start()
    {
        aiCon = GetComponent<AIController>();
        aiInfo = aiCon.aiInfo;
        aiAggro = GetComponent<AIAggro>();
        aiAnim = transform.GetChild(0).GetComponent<AIAnim>();
        cNetworkIdentity = GetComponent<CNetworkIdentity>();

        skillProcess = GetComponent<SkillProcess>();
        rd = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();
        agent = GetComponent<NavMeshAgent>();
        highlighter = GetComponent<Highlighter>();

        m_stun = false;
        isDie = false;

        enviroLayer = 1 << (int)SkillJudgeTarget.ENVIRO;    // 환경레이어 -> 데칼 확인

        ChangeClothes();                                    // 옷 조정

        GetComponent<AIController>().DieAction += () => { isDie = false; };
    }

    /// <summary>
    /// 옷 여부, 색상 변경해주기
    /// render[0]은 몸이기때문에 무조건 보여야함
    /// 보이는 것들은 palette_DB의 랜덤 값 넣기
    /// </summary>
    void ChangeClothes()
    {
        // Debug.Log(renderObject.Length + "," + gameObject.name);
        for (int i = 0; i < aiInfo.looks.Length; i++)
        {
            if (aiInfo.looks[i] >= Random.Range(0f, 1f))
            {
                renderObject[i].gameObject.SetActive(true);

                int _tmp = Random.Range(0, aiInfo.paletteNum);
                Color _color = new Color(aiInfo.palette[_tmp, 0], aiInfo.palette[_tmp, 1], aiInfo.palette[_tmp, 2]);
                renderObject[i].material.color = _color;
            }
            else
            {
                renderObject[i].gameObject.SetActive(false);
            }
        }
    }

    // 데미지 받았을 때 서버 보내기
    public override void TakeDamage(Skill skill, float damage, Vector3 force, GameObject attackObject)
    {
        base.TakeDamage(skill, damage, force, attackObject);

        if (cNetworkIdentity.isHost)
            InGameManager.Instance.CTransmitZombieTakeDamage(gameObject.GetComponent<CNetworkIdentity>().Netcode, skill.id);
    }

    /// <summary>
    /// 데미지 받았을 때 실행할 것들
    /// 1. 실드 확인
    /// 2. 어그로 증가시키기
    /// 3. 맞았을 때 애니메이션
    /// 4. 다른 AI들 추적으로 바꾸기
    /// 5. 충격으로 이동 일시정지하기
    /// </summary>
    /// <param name="playerObj">공격한 플레이어 오브젝트</param>
    /// <param name="hp">AI체력</param>
    /// <param name="damage">받은 데미지</param>
    /// <param name=""></param>
    public void TakeHitFromServer(GameObject playerObj)
    {
        // 1.
        if (m_shield)
        {
            Debug.Log("아니 이걸막네");
            if (ShieldCheck(playerObj.transform.position))
            {
                Debug.Log("방패나가신다!");
                return;
            }
        }
        // 2.
        if (cNetworkIdentity.isHost)
            aiAggro.IncreaseRagePoint(playerObj);

        // 3.
        aiAnim.HitAnimWeight();
        // 4.
        AnotherAIStateCheck();
        // 5.
        StartCoroutine(ShockStop());
    }

    /// <summary>
    /// 죽었을 때 행동할 것들
    /// 1. 죽은 소리내기
    /// 2. 공격한 위치 찾기 (그 방향으로 날려야하기 때문)
    /// 3. 죽은 상태로 만들기
    /// </summary>
    /// <param name="playerObj"></param>
    public void TakeDieFromServer(GameObject playerObj)
    {
        // Debug.Log(playerObj.transform.GetChild(0).name" 한테 죽음!");
        isDie = true;
        SoundManager.instance.ZombieHitSound(aiInfo.gender);

        Vector3 _force = playerObj.transform.GetChild(0).position;

        GetComponent<AIDeathState>().SetTakeForce(_force);
        aiCon.AIChangeState(AIController.AIState.DEATH);
    }

    /// <summary>
    /// 오버킬 당함
    /// 오버킬 파티클 + 언스폰
    /// </summary>
    public void TakeOverKillFromServer()
    {
        // Debug.Log("오버킬!");
        isDie = true;
        SoundManager.instance.ZombieHitSound(aiInfo.gender);
        AIPoolingManager.instance.PlayerOverkillParticle(transform.position, transform.rotation);

        GetComponent<AIDeathState>().CheckSpawnType();
    }

    /// <summary>
    /// 맞은 AI가 순찰중이면 주변 AI들을 추적으로 만듦
    /// </summary>
    void AnotherAIStateCheck()
    {
        if (aiCon.aiState == AIController.AIState.PATROL)
        {
            aiCon.AIChangeState(AIController.AIState.CHASE);

            Collider[] hit = Physics.OverlapSphere(transform.position, 3f, gameObject.layer);
            for (int i = 0; i < hit.Length; i++)
            {
                if (hit[i].gameObject.GetComponent<AIController>().aiState <= AIController.AIState.PATROL)
                    hit[i].gameObject.GetComponent<AIController>().AIChangeState(AIController.AIState.CHASE);
            }
        }
    }

    /// <summary>
    /// 일정시간 아웃라인 보이게 하고 속도 0으로 만들기
    /// </summary>
    /// <returns></returns>
    IEnumerator ShockStop()
    {
        if (agent.speed != 0)
        {
            highlighter.constant = true;
            agent.speed = 0f;
            yield return new WaitForSeconds(m_shockTime);
            agent.speed = aiCon.aiInfo.chaseSpeed;
            highlighter.constant = false;
        }
    }

    /// <summary>
    /// 데미지 입었을 때 일정시간동안 몸 빨개지기
    /// </summary>
    /// <returns></returns>
    IEnumerator ChangeHitColor()
    {
        for (int i = 0; i < renderObject.Length; i++)
        {
            renderObject[i].material.SetColor("_EmissionColor", Color.red * 0.2f);
        }
        yield return new WaitForSeconds(m_redTime);
        for (int i = 0; i < renderObject.Length; i++)
        {
            renderObject[i].material.SetVector("_EmissionColor", Color.red * 0f);
        }
    }

    /// <summary>
    /// 0. AIController 넉백 세팅하기
    /// 1. 넉백 자체를 하는 함수
    /// 2. 타겟으로 몸 돌리기
    /// </summary>
    /// <param name="pos"></param>
    public override void KnockBack(Vector3 pos)
    {
        base.KnockBack(pos);

        // 0
        aiCon.KnockbackSetting(pos);

        if (cNetworkIdentity.isGuest)
            return;

        // 1 
        rd.AddForce((transform.position - new Vector3(pos.x, transform.position.y, pos.z)).normalized * m_knockForce + Vector3.up * m_knockYForce, ForceMode.Impulse);
        // 넉백               힘      위치      반경(멀리날아나는 정도)      높이
        // rd.AddExplosionForce(activateSkill.effectPower, pos, activateSkill.judgeRange, activateSkill.effectTime, ForceMode.Impulse);

        // 2
        Vector3 rotatePos = pos - transform.position;
        rotatePos.y = 0;
        var rotation = Quaternion.LookRotation(rotatePos);
        transform.rotation = rotation;
    }


    /// <summary>
    /// 실드체크  **플레이어 총에 대해서만 성립해야함
    /// 범위 각도내에 있으면 실드된 것
    /// </summary>
    /// <param name="targetPos"></param>
    /// <returns></returns>
    public override bool ShieldCheck(Vector3 targetPos)
    {
        Vector3 _direction = (targetPos - transform.position).normalized;
        _direction.y = 0;
        float _angle = Vector3.Angle(_direction, transform.forward);

        // Debug.Log(_angle + ", " + aiInfo.skill[aiInfo.skillNum].guardAxis);
        if (_angle < SkillManager.instance.dictSkill[aiInfo.skill.skillIds[aiInfo.skill.usingSkillNum]].guardAxis * 0.5f)
        {
            return true;
        }
        return false;
    }
}