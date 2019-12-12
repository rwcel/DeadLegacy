using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// ZombieAA 애니메이션 : 기본상태 여러개
[RequireComponent(typeof(Animator))]
public class AAAIAnim : AIAnim
{
    int m_idleAnimNum;              // 대기 애니메이션 번호

    /// <summary>
    /// 속도조절 + 처음 애니메이션 랜덤 선택
    /// </summary>
    public override void InitSet()
    {
        base.InitSet();

        m_mulChaseSpeed = 1.5f;
        m_idleAnimNum = Random.Range(0, 5);
        // Debug.Log("기본AI 애님번호 : " + animNum);
    }

    /// <summary>
    /// AI상태에 따른 애니메이션 행동하기 
    /// anim.setXX
    /// </summary>
    /// <param name="state">행동상태</param>
    public override void AIAnimationCheck(AIController.AIState state)
    {
        base.AIAnimationCheck(state);

        switch (state)
        {
            case AIController.AIState.IDLE:             // 대기
                anim.SetInteger(hashAnimNumInteger, m_idleAnimNum);
                anim.SetBool(hashChaseBool, false);
                break;
			case AIController.AIState.PATROL:           // 탐색
                switch (patrolState)
				{
					case PatrolState.IDLE:
						switch (idleState)
						{
							case IdleState.NON:         // Idle행동 그대로 실행
								break;
							case IdleState.EAT:         // 시체 먹기
								anim.SetInteger(hashAnimNumInteger, c_biteAnimation);
								break;
						}
						break;
					case PatrolState.CLOSE:              // 랜덤 순찰
						anim.SetBool(hashChaseBool, true);
						break;
				}
				break;
            case AIController.AIState.CHASE:            // 추적
                //if (nav.enabled)
                //    nav.speed = aiCon.aiInfo.chaseSpeed;
                if(agent.speed != 0)
                    anim.SetFloat(hashChaseSpeedFloat, m_mulChaseSpeed);
                anim.SetBool(hashAttackBool, false);
                anim.SetBool(hashChaseBool, true);
                anim.applyRootMotion = false;
                break;
            case AIController.AIState.ATTACK:            // 싸움
                anim.SetBool(hashAttackBool, true);
                break;
            case AIController.AIState.CLIMB:            // 벽타기
                anim.SetBool(hashClimbBool, true);
                break;
            case AIController.AIState.LAND:             // 벽 뛰어내리기
                Debug.Log("뛰어내리기");
                LandingAnimWeight();
                break;
            case AIController.AIState.JUMP:             // 벽점프
                anim.SetTrigger(hashJumpTrigger);
                anim.applyRootMotion = true;
                break;
            case AIController.AIState.KNOCKBACK:        // 넉백
				anim.SetTrigger(hahsStunBool);
				break;
		}

    }
}