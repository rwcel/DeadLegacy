using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 좀비들 애니메이션 : 기본상태 1개
[RequireComponent(typeof(Animator))]
public class CommonAIAnim : AIAnim
{
    float m_leapVelocity = 4.8f;                // 도약 속력

    public override void InitSet()
    {
        base.InitSet();

        m_mulChaseSpeed = 1.2f;
    }

    // 행동에 따른 애니메이션
    public override void AIAnimationCheck(AIController.AIState state)
    {
        base.AIAnimationCheck(state);

        switch (state)
        {
            case AIController.AIState.IDLE:             // 대기
                anim.SetInteger(hashAnimNumInteger, 0);
				anim.SetBool(hashChaseBool, false);
                break;
			case AIController.AIState.PATROL:           // 탐색
				switch (patrolState)
				{
					case PatrolState.IDLE:
						switch (idleState)
						{
							case IdleState.NON:
								break;
							case IdleState.EAT:
								anim.SetInteger(hashAnimNumInteger, c_biteAnimation);
								break;
						}
						break;
					case PatrolState.CLOSE:
						anim.SetBool(hashChaseBool, true);
						break;
				}
				break;
			case AIController.AIState.CHASE:            // 추적
                //if (nav.enabled)
                //    nav.speed = aiCon.aiInfo.chaseSpeed;
                if (agent.speed != 0)
                    anim.SetFloat(hashChaseSpeedFloat, m_mulChaseSpeed);
                anim.SetBool(hashAttackBool, false);
                anim.SetBool(hashChaseBool, true);
                anim.applyRootMotion = false;
                break;
            // 스킬마다 애니메이션 다름
            case AIController.AIState.ATTACK:            // 싸움
                anim.SetInteger(hashAttackNumInteger, 0);
                anim.SetBool(hashAttackBool, true);
                break;
            case AIController.AIState.SPIT:
                anim.SetInteger(hashAttackNumInteger, 1);
                anim.SetBool(hashAttackBool, true);
                break;
            case AIController.AIState.CLIMB:            // 벽타기
                anim.SetBool(hashClimbBool, true);
                break;
            case AIController.AIState.LAND:             // 벽 뛰어내리기
                Debug.Log("뛰어내리기");
                LandingAnimWeight();
                break;
            case AIController.AIState.KNOCKBACK:
				//Debug.Log("제대로 실행?");
				anim.SetTrigger(hahsStunBool);
				break;
            // 거리가 멀수록 애니메이션 속도가 느려짐
            case AIController.AIState.LEAP:
                anim.SetInteger(hashAttackNumInteger, 1);
                anim.SetBool(hashAttackBool, true);
                float _leapSpeed = m_leapVelocity / SkillManager.instance.dictSkill[aiCon.aiInfo.skill.skillIds[1]].maxDist;
                anim.SetFloat(hashLeapSpeedFloat, _leapSpeed);
                break;
            case AIController.AIState.RUSH:
                anim.SetInteger(hashAttackNumInteger, 1);
                anim.SetBool(hashAttackBool, true);
                break;
        }

    }
}