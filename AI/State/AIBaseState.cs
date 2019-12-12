using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public enum AttackSkillArray
{
    NORMAL, SPECIAL
}

public class AIBaseState : MonoBehaviour
{
    // 하위 상태 딕셔너리  **AIIdleState에서 add
    public Dictionary<AIController.AIState, AIBaseState> dictStateAction = new Dictionary<AIController.AIState, AIBaseState>();

    // 외부 참조항목
    protected AIController aiCon;
    protected AIAnim aiAnim;
    protected Animator anim;
    protected AILiving living;
    protected SkillProcess skillProcess;
    protected CNetworkIdentity cNetworkIdentity;

    // 내부컴포넌트 참조항목
    protected NavMeshAgent nav;
    protected CapsuleCollider col;
    protected Rigidbody rd;

    // **값 실시간으로 바꿔줘야함 (함수 접근할 때 받아오는 상태)
    protected Transform targetTr;                   // 타겟(플레이어) 트랜스폼
    protected bool isPatrolMove;                    // 추적 끝?
    protected bool isSkill;
    protected bool isAction;

    public bool isAct
    {
        get { return isAction; }
        set { isAction = value; }
    }

    protected float rotSpeed = 5f;                  // 회전 속도
    protected int activeSkillNum;                   // 사용중인 스킬

    // 레이어
    protected LayerMask enviroLayer;
    protected LayerMask groundLayer;

    // 순찰 변수  **serialize가능성 있음
    protected Vector3 createPos;                              // 생성 위치
    protected Vector3 patrolDest = new Vector3(0, 0, 0);      // 추적 위치

    protected float waitTime = 0.5f;                          // 기다리는 시간
    protected float groundHeight = 0.2f;                      // 땅 인식 최소 거리  **아래로 ray를 쏴서 이때부터 땅으로 취급

    protected int tick = 0;
    protected int aiTickCount = 5;

    // **무조건 바꿔야함 : 현재 좀비가 1이기 때문에
    protected int lowProperty = 50;                         // 기본우선순위
    protected int highProperty = 20;                        // 높은 우선순위

    protected NetworkAITransmitor networkAITransmitor;                    // 호스트
    protected NetworkAISyncor networkAISyncor;                            // 게스트

    void Awake()
    {
        aiCon = GetComponent<AIController>();
        aiAnim = transform.GetChild(0).GetComponent<AIAnim>();
        anim = aiAnim.GetComponent<Animator>();
        living = GetComponent<AILiving>();
        skillProcess = GetComponent<SkillProcess>();
        cNetworkIdentity = GetComponent<CNetworkIdentity>();
        networkAITransmitor = GetComponent<NetworkAITransmitor>();
        networkAISyncor = GetComponent<NetworkAISyncor>();

        nav = GetComponent<NavMeshAgent>();
        col = GetComponent<CapsuleCollider>();
        rd = GetComponent<Rigidbody>();

        SettingLayerMasks();
        DictionarySetting();

        // 죽으면 실행
        GetComponent<AIController>().DieAction += () => { isAction = false; isPatrolMove = false; };
    }

    void SettingLayerMasks()
    {
        enviroLayer = 1 << (int)SkillJudgeTarget.ENVIRO;
        groundLayer = 1 << (int)SkillJudgeTarget.GROUND;
    }

    // [AIIdleState] 딕셔너리 세팅
    protected virtual void DictionarySetting() { }

    // 액션 시작부분 : 한번만 실행
    public virtual void StartAction() { }

    // 게스트 스킬액션 부분
    public virtual void GuestSkillAction(int skillId, Vector3 targetPos) { }

    // 게스트 액션
    public virtual void GuestAction() { }

    // 스킬 반동
    public virtual IEnumerator SkillRebound(float _time) { yield return null; }

    // 액션 후처리 : 상태값 돌리기
    public virtual void EndAction() { }

    /// <summary>
    /// 타겟을 바라보게 하기
    /// y값 차이는 없애기
    /// </summary>
    /// <param name="targetPos"></param>
    protected void LookatTarget(Vector3 targetPos)
    {
        Vector3 rotatePos = targetPos - transform.position;
        rotatePos.y = 0;
        var rotation = Quaternion.LookRotation(rotatePos);
        transform.rotation = Quaternion.Slerp(transform.rotation, rotation, Time.deltaTime * rotSpeed);
    }

    /// <summary>
    /// 바닥으로 레이를 쏴서 착륙상태인지 확인
    /// 불가능하면 return false
    /// 상태 및 네비게이션 재설정
    /// </summary>
    /// <returns></returns>
    protected bool LandingAndStatusCheck(AIController.AIState state)
    {
        // Debug.DrawRay(transform.position, -transform.up * 0.2f, Color.green, 2f);
        if (Physics.Raycast(transform.position, -transform.up, groundHeight*2, groundLayer))
        {
            ConditionRegulate(state, false);
            NavMeshHit hit;
            nav.SamplePathPosition(groundLayer, 2f, out hit);
            nav.nextPosition = hit.position;
            //NavMesh.CalculatePath(transform.position, targetTr.transform.position, NavMesh.AllAreas, nav.path);
            // Debug.Log("되돌아오기:"+nav.nextPosition);
            // col.radius = 0.5f;

            nav.avoidancePriority = lowProperty;
            nav.acceleration = 12;

            return true;
        }

        return false;
    }

    /// <summary>
    ///  게스트용 착륙확인
    /// </summary>
    /// <param name="pos"></param>
    /// <param name="state"></param>
    /// <returns></returns>
    protected bool LandingAndStatusCheck(Vector3 targetPos, AIController.AIState state)
    {
        if (Physics.Raycast(transform.position, -transform.up, groundHeight, groundLayer))
        {
            ConditionRegulate(state, false);
            NavMeshHit hit;
            nav.SamplePathPosition(groundLayer, 2f, out hit);
            nav.nextPosition = hit.position;
            // NavMesh.CalculatePath(transform.position, targetPos, NavMesh.AllAreas, nav.path);
            // Debug.Log("되돌아오기:" + nav.nextPosition);
            // col.radius = 0.5f;

            nav.avoidancePriority = lowProperty;
            nav.acceleration = 12;

            return true;
        }

        return false;
    }

    // 벽타기 종료, 점프 종료 -> ClimbState, JumpState
    public virtual void EndState(Vector3 _pos) { }

    // 다른 행동이 가능한 상태인지 확인 -> AIController
    public bool CanAct()
    {
        if (!isSkill && !isAct)
            return false;
        else
            return true;
    }

    /// <summary>
    /// rigidbody, navmesh, animation, collider 조절
    /// _bool은 무조건 true->false
    /// </summary>
    /// <param name="_bool"></param>
    public void ConditionRegulate(AIController.AIState _state, bool _bool)
    {
        // 상태에 따라 끄는 것들 조절
        switch (_state)
        {
            // 죽는상태 : rigidbody, navmesh, animation, collider
            case AIController.AIState.DEATH:
                nav.enabled = !_bool;

                rd.isKinematic = _bool;
                rd.velocity = Vector3.zero;

                col.isTrigger = _bool;
                col.enabled = !_bool;

                anim.enabled = !_bool;
                break;
            // 도약, 넉백 : navmesh, rigidbody
            case AIController.AIState.LEAP:
                nav.updatePosition = !_bool;
                nav.enabled = !_bool;

                rd.isKinematic = !_bool;

                 col.isTrigger = _bool;
                // col.enabled = !_bool;         ** 점프 중 벽 못뚫게
                break;
            case AIController.AIState.KNOCKBACK:
                nav.updatePosition = !_bool;
                nav.enabled = !_bool;

                rd.isKinematic = !_bool;
                col.isTrigger = _bool;
                break;
            case AIController.AIState.RUSH:
                rd.isKinematic = !_bool;

                nav.updatePosition = !_bool;
                nav.updateRotation = !_bool;
                break;
        }
    }
}
