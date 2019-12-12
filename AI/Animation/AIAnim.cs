using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// AI 애니메이션 스크립트
[RequireComponent(typeof(Animator))]
public class AIAnim : MonoBehaviour
{
	protected const int c_biteAnimation = 10;           // 무는 애니메이션(idle)

    // 해시코드 생성
    public int hashAnimNumInteger;                      // 실행할 애니메이션 번호 (같은 동작이 2가지 이상일 때 고르게 해주는 것 : start에서 랜덤)
    public int hashAttackNumInteger;                    // 공격 애니메이션 번호
    public int hashChaseBool;                           // 추적중인지
	public int hashAttackBool;                          // 공격중인지
    public int hahsStunBool;                            // 스턴상태인지
    public int hashClimbBool;                           // 벽을 올라가는지
    public int hashHitTrigger;                          // 데미지를 입음
	public int hashReboundTrigger;                      // 스킬사용 후 반동
    public int hashJumpTrigger;                         // 점프
    public int hashLandingTrigger;                      // 떨어지기
    public int hashChaseSpeedFloat;                     // 추적 속도 **Multiplier
    public int hashLeapSpeedFloat;                      // 도약 애니메이션 속도 **Multiplier
    public int hashRushReboundTrigger;                  // 러시 후 반동

    // 외부참조
    protected AIController aiCon;
    protected AIBaseState aiBaseState;

    // 내부참조
    protected Animator anim;
    protected NavMeshAgent agent;

    // 기본 상태
    protected IdleState idleState;
    protected PatrolState patrolState;

    protected float m_mulChaseSpeed;
    protected float m_normalSpeed;
    protected float m_chaseSpeed;

    // 캐싱하기
	private void Awake()
	{
		hashChaseBool = Animator.StringToHash("IsChase");
		hashAttackBool = Animator.StringToHash("IsAttack");
		hashHitTrigger = Animator.StringToHash("Hit");

		hashAnimNumInteger = Animator.StringToHash("AnimNum");
		hahsStunBool = Animator.StringToHash("IsStun");
		hashReboundTrigger = Animator.StringToHash("Rebound");
		hashClimbBool = Animator.StringToHash("IsClimb");

		hashJumpTrigger = Animator.StringToHash("Jump");
        hashLandingTrigger = Animator.StringToHash("Landing");
        hashAttackNumInteger = Animator.StringToHash("AttackNum");

        hashChaseSpeedFloat = Animator.StringToHash("ChaseSpeed");
        hashLeapSpeedFloat = Animator.StringToHash("LeapSpeed");
        hashRushReboundTrigger = Animator.StringToHash("RushRebound");
    }

    // 참조하기
	void Start()
    {
        anim = GetComponent<Animator>();
        aiCon = GetComponentInParent<AIController>();
        agent = GetComponentInParent<NavMeshAgent>();
        aiBaseState = GetComponentInParent<AIBaseState>();

        InitSet();
    }

    // 애니메이션 속도 조절 : 상속
    public virtual void InitSet()   {    }

    // state에 따른 애니메이션 행동하기 : 상속
    public virtual void AIAnimationCheck(AIController.AIState state) {  }

    /// <summary>
    /// AI들이 하는 기본행동들 가져오기  
    /// **start에 넣으면 컨트롤러 DB적용보다 먼저 가져와져서 data가 없음
    /// </summary>
    /// <param name="_idleState">기본상태</param>
    /// <param name="_patrolState">순찰상태</param>
    public void GetBasicState(IdleState _idleState, PatrolState _patrolState, float _normalSpeed, float _chaseSpeed)
	{
		idleState = _idleState;
		patrolState = _patrolState;

        m_normalSpeed = _normalSpeed;
        m_chaseSpeed = _chaseSpeed;
    }

    // 스턴
    public void Stun(float _time)
    {
        // Debug.Log("스턴하세요");
        StartCoroutine(Stunning(_time));
    }
    IEnumerator Stunning(float _time)
    {
        anim.SetBool(hahsStunBool, true);
        yield return new WaitForSeconds(_time);
        anim.SetBool(hahsStunBool, false);
    }

    // 가중치 변경해서 상체만 애니메이션 실행
    public void HitAnimWeight()
    {
        anim.SetLayerWeight(1, 0.5f);
        anim.SetTrigger(hashHitTrigger);
        anim.SetLayerWeight(1, 1f);
    }

    public void LandingAnimWeight()
    {
        anim.SetLayerWeight(2, 0.5f);
        anim.SetTrigger(hashLandingTrigger);
        anim.SetLayerWeight(2, 1f);
    }

    // 루트모션 켜서 벽오르기
    public void ClimbRail()
    {
        anim.applyRootMotion = true;
        anim.SetBool(hashClimbBool, false);
    }

    /// <summary>
    /// 벽 오르기 끝 [AnimationEvent]
    /// 모델 위치 돌리고 루트모션끄기
    /// </summary>
    public void ClimbEnd()
    {
        aiBaseState.dictStateAction[AIController.AIState.CLIMB].EndState(transform.position);
        transform.localPosition = new Vector3(0, 0, 0);
        anim.applyRootMotion = false;
    }

    // 점프 끝 [AnimationEvent]
    public void JumpEnd()
    {
        aiBaseState.dictStateAction[AIController.AIState.JUMP].EndState(transform.position);
        transform.localPosition = new Vector3(0, 0, 0);
        anim.applyRootMotion = false;
    }

    // 돌진 반동 애니메이션 실행
    public void RushRebound()
    {
        anim.SetTrigger(hashRushReboundTrigger);
    }

    // 돌진 끝 [AnimationEvent]
    public void RushEnd()
    {
        aiBaseState.dictStateAction[AIController.AIState.RUSH].EndState(transform.position);
        transform.localPosition = new Vector3(0, 0, 0);
        anim.applyRootMotion = false;
    }

}