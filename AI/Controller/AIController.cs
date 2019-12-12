using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using System;
using NetworkBase;
using Global;
using DG.Tweening;

/// <summary>
/// AI 정보 데이터 : DB(CSV_AI)에서 가져온 데이터
/// </summary>
[System.Serializable]
public struct AIInfo
{
    [Header("[스텟]")]
    public short id;                  // ai번호
    public AIType type;             // 소환종류
    public Gender gender;           // 성별
    public float hp;                // 체력
    public float damage;            // 기본데미지
    public float armor;             // 방어력
    public float minHitDamage;      // 최소 맞는 데미지
    public float normalSpeed;       // 기본속도(idle)
    public float chaseSpeed;        // 추적속도

    public IdleState idleState;     // 대기 상태
    public PatrolState patrolState; // 기본 상태

    [Header("[어그로 수치]")]
    public float chaseDist;         // 추적거리 [여기서도 사용]
    public float viewAngle;         // 시야각
    public float viewDist;          // 시야거리

    [Header("[외형]")]
    public float scale;             // 캐릭터 크기
    public int paletteID;           // 팔레트 ID (색 넣어줄 항목)
    public int paletteNum;          // 팔레트 수
    public float[,] palette;        // 팔레트

    public int clothesNum;          // 옷 개수
    public float[] looks;           // 옷 항목

    public SkillInfo skill;           // 가지고 있는 스킬
}
[RequireComponent(typeof(NavMeshAgent))]

public class AIController : MonoBehaviour
{
    // Inspector 상에 띄울 ID
    [SerializeField] public short ID;                 // 필드몹에게 넣어줄 ID

    [HideInInspector] public AIInfo aiInfo;

    // 외부 참조
    AIAggro aiAggro;
    protected AIAnim aiAnim;
    SkillProcess skillProcess;
    CNetworkIdentity cNetworkIdentity;

    // 내부참조
    protected NavMeshAgent nav;
    protected NavMeshPath path;
    Animator anim;
    Rigidbody rd;
    protected CapsuleCollider col;

    float targetDist;

    // 탐색 변수
    protected Transform targetTr;                   // 타겟(플레이어) 트랜스폼
    public Transform targetTransform
    {
        get { return targetTr; }
        set { targetTr = value; }
    }

    [SerializeField] GameObject rootM;

    // 레이어 마스크들
    LayerMask aiLayer;
    LayerMask dieLayer;
    protected LayerMask playerLayer;
    protected LayerMask enviroLayer;
    LayerMask groundLayer;

    // Action관련
    protected AIBaseState baseState;
    public Action DieAction;

    public short m_NetCode;

    AILiving living;

    // 상태              대기   순찰(x)  추적   공격   죽음   스턴  벽타기  점프	넉백	
    public enum AIState
    {
        IDLE, PATROL, CHASE, ATTACK, DEATH, STUN, CLIMB, LAND, JUMP, KNOCKBACK, 
        // 특수좀비공격       침   돌진  도약
        SPIT, RUSH, LEAP
    }
    protected AIState state;
    public AIState aiState
    {
        get { return state; }
        set { state = value; }
    }
    public AIState beforeState;

    // 오프메시 링크관련 변수  [AnimationLerp]
    public OffMeshLink offLink = null;         // 링크 가지고 있는지 확인
    bool isOffLink;                            // 오프 메시 링크를 가져도 되는지

    // **통합하기
    bool isAggro;       // 어그로 받기 가능?

    [HideInInspector] public GameObject judgeObj;             // 들어가는 판정 오브젝트

    int tick = 0;
    int aiTickCount = 5;
    public int stateTickCount = 8;                             // 3*8 -> 0.06 * 8 = 0.48초
    int aggroTick = 0;
    int aggroTickCount;
    int avoidPriority;                                          // 기본 80

    NetworkAITransmitor networkAITransmitor;                    // 호스트
    NetworkAISyncor networkAISyncor;                            // 게스트

    bool isFirst;

    // 컴포넌트 불러오기
    private void Awake()
    {
        aiAggro = GetComponent<AIAggro>();
        aiAnim = transform.GetChild(0).GetComponent<AIAnim>();
        anim = aiAnim.GetComponent<Animator>();
        baseState = GetComponent<AIBaseState>();
        living = GetComponent<AILiving>();
        skillProcess = GetComponent<SkillProcess>();
        cNetworkIdentity = GetComponent<CNetworkIdentity>();
        networkAITransmitor = GetComponent<NetworkAITransmitor>();
        networkAISyncor = GetComponent<NetworkAISyncor>();

        nav = GetComponent<NavMeshAgent>();
        path = nav.path;
        rd = GetComponent<Rigidbody>();
        col = GetComponent<CapsuleCollider>();

        SettingLayerMasks();

        // 애니메이션 똑같이 안보이게
        stateTickCount = UnityEngine.Random.Range(stateTickCount - 1, stateTickCount + 2);
        // stateCheckCooltime = UnityEngine.Random.Range(stateCheckCooltime - 0.1f, stateCheckCooltime + 0.1f);
    }

    void SettingLayerMasks()
    {
        playerLayer = 1 << (int)SkillJudgeTarget.PLAYER;
        aiLayer = 1 << (int)SkillJudgeTarget.MONSTER;
        enviroLayer = 1 << (int)SkillJudgeTarget.ENVIRO;
        groundLayer = 1 << (int)SkillJudgeTarget.GROUND;
        dieLayer = 1 << (int)SkillJudgeTarget.DIE;
    }

    /// <summary>
    /// 1. DB관련 초기설정하기(스폰, 필드) : anim, aggro
    /// 2. 랜덤된 크기 조정(모델, 콜라이더)
    /// 3. 호스트만 판단하게 하기
    /// </summary>
    protected virtual void Start()
    {
        // 1. **미리 깔린 애들은 실행 전 id 넣어주고, db가 비어있는지를 확인해서 넣어주기
        if (aiInfo.hp == 0)
        {
            aiInfo = CSV_AI.instance.aiDictionary[ID];
            aiInfo.type = AIType.FIELD;
            // **AIPoolingManager가 게스트에 없기 때문
        }
        else
        {
            ID = aiInfo.id;
            aiInfo.type = AIType.SPAWN;
        }

        aiAnim.GetBasicState(aiInfo.idleState, aiInfo.patrolState, aiInfo.normalSpeed, aiInfo.chaseSpeed);

        if (cNetworkIdentity.isHost)
        {
            aiAggro.AggroDBSetting(aiInfo.id);
            aggroTickCount = (int)(aiAggro.aggroData.aggroTime * 2);
        }

        // 2.
        aiAnim.gameObject.transform.localScale *= aiInfo.scale;
        col.height *= aiInfo.scale;
        col.radius *= aiInfo.scale;

        LiveReset();
        DieAction += LiveReset;

        //AINetworkManager.instance.AIAddList(this.gameObject);

        // 3.
        //baseState.dictStateAction[state].enabled = true;
        //baseState.dictStateAction[state].StartAction();
    }

    private void OnEnable()
    {
        try
        {
            // Debug.Log(state);
            baseState.dictStateAction[state].enabled = true;
            baseState.dictStateAction[state].StartAction();
        }
        catch
        {
            // Debug.Log("시작시");
        }
    }

    /// <summary>
    /// 바뀐 상태(값)들 초기화 해주는 함수
    /// 상태 OFF  
    /// **상태중인것을 해당하는 변수를 새로 만들지(그 하나로만 관리) 
    /// 어차피 다른 상태로는 못가기에
    /// </summary>
    void LiveReset()
    {
        isAggro = false;

        state = AIState.IDLE;
    }

    private void FixedUpdate()
    {
        tick++;
        if (tick >= aiTickCount * stateTickCount)
        {
            if (living.isDieP)
                return;

            tick = 0;

            //beforeState = state;
            if (baseState.dictStateAction[state].CanAct())
                return;

            if (cNetworkIdentity.isHost)
            {
                aggroTick++;
                if (!isAggro)
                {
                    StartAggroCheck();
                }
                else if (aggroTick >= aggroTickCount)
                {
                    aggroTick = 0;
                    EndAggroCheck();
                }
            }
            else
            {
                this.enabled = false;
                return;
            }

            // 2. 
            DistStateCheck();

            // 3. 
            // OffLinkStateCheck();

            // 4.
            aiAnim.AIAnimationCheck(state);

            // 5.
            AIActionCheck();
        }
    }

    /// <summary>
    /// 거리관련 상태 확인 함수
    /// 1. 타겟 갱신(aggroTime 주기마다)
    /// 2. 타겟과의 거리 확인 : chaseDist내에 있어야 추적
    /// 3. 스킬 사용가능한지 확인 (공격상태)
    /// 4. 그게 아니면 추적상태
    /// </summary>
    public void DistStateCheck()
    {
        if (cNetworkIdentity.isHost)
        {
            if (targetTr == null)
                return;
        }

        // 2.
        targetDist = (transform.position - targetTr.position).sqrMagnitude;

        //Debug.Log("스킬 수 : " + skill.Length);
        if (targetDist < (aiInfo.chaseDist * aiInfo.chaseDist))
        {
            // 3.
            if (state == AIState.CHASE && skillProcess.SkillCheck(targetDist))
            {
                Vector3 rotatePos = targetTr.position - transform.position;
                rotatePos.y = 0;
                transform.rotation = Quaternion.LookRotation(rotatePos);

                if (cNetworkIdentity.isHost)
                    AttackTransmitor();
            }
            // 4.
            else
            {
                if (!nav.enabled)
                    nav.enabled = true;

                state = AIState.CHASE;
            }
        }

        // 가장 가까울때 1.8, 추적거리 20 -> 3.xx ~ 400 -> 0.4f
        avoidPriority = (int)(targetDist * 0.4f);
        nav.avoidancePriority = Mathf.Clamp(avoidPriority, 5, 80);

        // else 거리 멀어지더라도 계속 추적하게 변경됨 : 맨 처음만 IDLE
    }

    void AttackTransmitor()
    {
        CAIPacket _aiPacket = networkAITransmitor.GetAIPacket();

        switch (aiInfo.skill.skillIds[skillProcess.skillInfo.usingSkillNum])
        {
            case SkillManager.LEAP_ATTACK:
                state = AIState.LEAP;
                targetTr = aiAggro.getSecondTarget();
                networkAITransmitor.SetCurAIAction(AiAction.Skill_Jump);
                _aiPacket.SetAIAngleV(targetTr.position);
                break;
            case SkillManager.RUSH_ATTACK:
                state = AIState.RUSH;
                networkAITransmitor.SetCurAIAction(AiAction.Skill_Rush);
                break;
            case SkillManager.SLOW_SPIT:
            case SkillManager.SLOW_SPIT2:
            case SkillManager.POSION_SPIT:
                state = AIState.SPIT;
                networkAITransmitor.SetCurAIAction(AiAction.Skill_Spit);
                _aiPacket.SetAIAngleV(targetTr.position);
                break;
            default:
                state = AIState.ATTACK;
                networkAITransmitor.SetCurAIAction(AiAction.AttackStart);
                break;
        }
        _aiPacket.SetAIAction(networkAITransmitor.GetCurAIAction());
        _aiPacket.SetAIPosition(transform.position);
        _aiPacket.SetAIAngleR(transform.rotation.eulerAngles.y);

        // networkAITransmitor.StartFullAction();
    }

    public void FullActionSyncor(CAIPacket _aiPacket)
    {
        transform.position = _aiPacket.m_AIPos;
        transform.rotation = Quaternion.Euler(new Vector3(0, _aiPacket.m_AIRotY, 0));

        // Debug.Log(_aiPacket.m_Action);
        switch (_aiPacket.m_Action)
        {
            case AiAction.AttackStart:
                aiAnim.AIAnimationCheck(AIState.ATTACK);
                baseState.dictStateAction[AIState.ATTACK].GuestSkillAction(aiInfo.skill.skillIds[0], transform.position);
                _aiPacket.DeFlagAngleR();
                _aiPacket.DeFlagAiPos();
                _aiPacket.DeFlagAction();
                networkAISyncor.CheckSyncEnd(_aiPacket);
                break;
            case AiAction.Skill_Spit:
                Vector3 _spitPos = _aiPacket.m_TargetPos;
                aiAnim.AIAnimationCheck(AIState.SPIT);
                baseState.dictStateAction[AIState.SPIT].GuestSkillAction(aiInfo.skill.skillIds[1], _spitPos);
                _aiPacket.DeFlagAngleV();
                _aiPacket.DeFlagAiPos();
                _aiPacket.DeFlagAngleR();
                break;
            case AiAction.Skill_Jump:
                Vector3 _LeapPos = _aiPacket.m_TargetPos;
                aiAnim.AIAnimationCheck(AIState.LEAP);
                baseState.dictStateAction[AIState.LEAP].GuestSkillAction(aiInfo.skill.skillIds[1], _LeapPos);
                _aiPacket.DeFlagAngleV();
                _aiPacket.DeFlagAiPos();
                _aiPacket.DeFlagAngleR();
                break;
            case AiAction.Skill_Rush:
                aiAnim.AIAnimationCheck(AIState.RUSH);
                Debug.Log(aiInfo.skill.skillIds[1]);
                baseState.dictStateAction[AIState.RUSH].GuestSkillAction(aiInfo.skill.skillIds[1], transform.position);
                _aiPacket.DeFlagAngleR();
                _aiPacket.DeFlagAiPos();
                break;
            case AiAction.KnockBack:
                Vector3 _knockBackPos = _aiPacket.m_TargetPos;
                baseState.dictStateAction[AIState.KNOCKBACK].GuestSkillAction(0, _knockBackPos);
                _aiPacket.DeFlagAngleV();
                _aiPacket.DeFlagAiPos();
                _aiPacket.DeFlagAction();
                networkAISyncor.CheckSyncEnd(_aiPacket);
                break;
            case AiAction.Climb:
                aiAnim.AIAnimationCheck(AIState.CLIMB);
                baseState.dictStateAction[AIState.CLIMB].GuestAction();
                _aiPacket.DeFlagAiPos();
                break;
            case AiAction.Land:
                aiAnim.AIAnimationCheck(AIState.LAND);
                baseState.dictStateAction[AIState.LAND].GuestAction();
                _aiPacket.DeFlagAiPos();
                _aiPacket.DeFlagAngleR();
                break;
        }
        // Debug.Log("공격 명령시작");
    }

    /// <summary>
    /// 오프메시링크 관련 상태 : 벽 오르기(Wall), 뛰어내리기(Cliff)
    /// **서버에 어떻게 주지?
    /// </summary>
    public void OffLinkStateCheck()
    {
        if (offLink==null && nav.isOnOffMeshLink)
        {
            CAIPacket _aiPacket = networkAITransmitor.GetAIPacket();

            offLink = nav.currentOffMeshLinkData.offMeshLink;

            if (offLink.CompareTag("Wall"))
            {
                state = AIState.CLIMB;
                networkAITransmitor.SetCurAIAction(AiAction.Climb);
            }
            else if(offLink.CompareTag("Cliff"))
            {
                state = AIState.LAND;
                networkAITransmitor.SetCurAIAction(AiAction.Land);
                _aiPacket.SetAIAngleR(transform.rotation.eulerAngles.y);
            }

            _aiPacket.SetAIAction(networkAITransmitor.GetCurAIAction());
            _aiPacket.SetAIPosition(transform.position);

            AIChangeState(state);
        }

        // **여기서 전송
    }

    // 어그로 확인하기
    void StartAggroCheck()
    {
        targetTr = aiAggro.getTargetTransform();
        isAggro = true;
    }
    void EndAggroCheck()
    {
        isAggro = false;
    }

    /// <summary>
    /// AI 상태에 따른 행동
    /// baseState 자식 객체 중 상태에 해당하는 함수 실행
    /// </summary>
    /// <returns></returns>
    void AIActionCheck()
    {
        // Debug.Log(beforeState + "==" + state);

        if (beforeState != state && baseState.dictStateAction.ContainsKey(state))
        {
            baseState.dictStateAction[beforeState].EndAction();
            baseState.dictStateAction[state].StartAction();

            // InGameManager.Instance.TransmitZombieState(cNetworkIdentity.netCode, (int)state);
        }
        beforeState = state;
    }

    public void KnockbackSetting(Vector3 pos)
    {
        if (cNetworkIdentity.isHost)
        {
            if (state == AIState.KNOCKBACK)
                return;

            CAIPacket _aiPacket = networkAITransmitor.GetAIPacket();

            networkAITransmitor.SetCurAIAction(AiAction.KnockBack);
            _aiPacket.SetAIAction(networkAITransmitor.GetCurAIAction());
            _aiPacket.SetAIPosition(transform.position);
            // _aiPacket.SetAIAngleR(transform.rotation.eulerAngles.y);
            _aiPacket.SetAIAngleV(pos);

            state = AIState.KNOCKBACK;
            AIChangeState(state);
        }
        else
        {
            while (true)
            {
                try
                {
                    CAIPacket _aiPacket = networkAISyncor.GetSyncInfo();
                    if (_aiPacket.m_Action == Global.AiAction.AttackEnd)
                    {
                        _aiPacket.DeFlagAction();
                    }
                    else if (_aiPacket.m_Action == AiAction.KnockBack)
                    { 
                        _aiPacket.ForceDeFlagAll();
                        networkAISyncor.CheckSyncEnd(_aiPacket);
                    }
                    else
                    {
                        break;
                    }
                }
                catch
                {
                    break;
                }
            }
        }
    }

    /// <summary>
    /// 상태 + 애니메이션 바꾸기
    /// **action = true하는 상태일 경우 이전상태가 저장되지 않음으로 beforeState에 넣어주고
    /// 새로 상태 입력한 것과 비교하게 한다(무조건 다음상태 가짐)
    /// </summary>
    /// <param name="_state"></param>
    public void EndActionCheckState(AIState _state)
    {
        // Debug.Log("상태변경");
        beforeState = _state;

        DistStateCheck();
        aiAnim.AIAnimationCheck(state);
        AIActionCheck();
    }

    /// <summary>
    /// 바뀐 상태에 대해 행동 + 애니메이션
    /// </summary>
    /// <param name="_state"></param>
    public void AIChangeState(AIState _state)
    {
        state = _state;
        // Debug.Log("바뀌는 상태 : " + state);
        aiAnim.AIAnimationCheck(state);
        AIActionCheck();
    }

    // **벽 통과되는거 방지
    private void OnTriggerEnter(Collider other)
    {
        OffLinkStateCheck();

        if (1 << other.gameObject.layer == enviroLayer.value)
            col.isTrigger = false;
    }
}