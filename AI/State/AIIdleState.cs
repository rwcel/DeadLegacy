using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

public class AIIdleState : AIBaseState
{
    #region 다른 상태들
    AIPatrolState patrolState;
    AIChaseState chaseState;
    AIAttackState attackState;
    AIKnockBackState knockbackState;
    AIDeathState deathState;
    // 일부좀비(?)
    AIClimbState climbState;
    AILandState landState;
    AIJumpState jumpState;
    // 특수좀비
    AISpitState spitState;
    AILeapState leapState;
    AIRushState rushState;
    #endregion

    /// <summary>
    /// 상태 딕셔너리 추가
    /// **다른 곳에서는 수정이 안됨
    /// **try~catch로는 null값으로 들어가서 catch 잡기가 불가능함
    /// </summary>
    protected override void DictionarySetting()
    {
        base.DictionarySetting();

        patrolState = GetComponent<AIPatrolState>();
        chaseState = GetComponent<AIChaseState>();
        attackState = GetComponent<AIAttackState>();
        knockbackState = GetComponent<AIKnockBackState>();
        deathState = GetComponent<AIDeathState>();

        dictStateAction.Add(AIController.AIState.IDLE, this);
        dictStateAction.Add(AIController.AIState.PATROL, patrolState);
        dictStateAction.Add(AIController.AIState.CHASE, chaseState);
        dictStateAction.Add(AIController.AIState.ATTACK, attackState);
        dictStateAction.Add(AIController.AIState.KNOCKBACK, knockbackState);
        dictStateAction.Add(AIController.AIState.DEATH, deathState);

        // Debug.Log("접근끝");
        climbState = GetComponent<AIClimbState>();
        if(climbState != null)
            dictStateAction.Add(AIController.AIState.CLIMB, climbState);

        landState = GetComponent<AILandState>();
        if (landState != null)
            dictStateAction.Add(AIController.AIState.LAND, landState);

        jumpState = GetComponent<AIJumpState>();
        if (jumpState != null)
            dictStateAction.Add(AIController.AIState.JUMP, jumpState);

        spitState = GetComponent<AISpitState>();
        if(spitState != null)
            dictStateAction.Add(AIController.AIState.SPIT, spitState);

        leapState = GetComponent<AILeapState>();
        if (leapState != null)
            dictStateAction.Add(AIController.AIState.LEAP, leapState);

        rushState = GetComponent<AIRushState>();
        if (rushState != null)
            dictStateAction.Add(AIController.AIState.RUSH, rushState);
    }

    /// <summary>
    /// 죽으면 기존 상태 다 지우기
    /// 탐색 상태로 이동하기
    /// </summary>
    public override void StartAction()
    {
        base.StartAction();

        GetComponent<AIController>().DieAction += () =>
        {
            foreach (KeyValuePair<AIController.AIState, AIBaseState> _state in dictStateAction)
            {
                _state.Value.enabled = false;
            }
        };

        if (cNetworkIdentity.isHost)
        {
            nav.isStopped = true;
            aiCon.AIChangeState(AIController.AIState.PATROL);
        }
    }

}
