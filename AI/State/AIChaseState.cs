using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkBase;
using Global;
using UnityEngine.AI;

public class AIChaseState : AIBaseState
{
    int targetUpdateTickCount = 4;          // 4*(7~9) - 어그로 동기화랑 같은 시간주기

    private void Start()
    {
        targetUpdateTickCount *= aiCon.stateTickCount;
    }

    /// <summary>
    /// ** 상태값 바뀌는 것들 aiController에 넣어서 관리할 수 있게 하기 (서버와 상담하기)
    /// 1. 타겟이 없으면 idle상태로 변경
    /// 2. 타겟을 바라봐 이동하기
    /// </summary>
    public override void StartAction()
    {
        base.StartAction();
        this.enabled = true;

        // Debug.Log("추적상태");
        // anim.AIAnimationCheck(aiCon.aiState);
        networkAITransmitor.SetCurAIAction(AiAction.Chase);

        targetTr = aiCon.targetTransform;
        // 1.
        if (targetTr == null)
        {
            aiCon.AIChangeState(AIController.AIState.PATROL);
            return;
        }

        LookatTarget(targetTr.position);
        // 2.
        nav.isStopped = false;
        nav.speed = aiCon.aiInfo.chaseSpeed;
        nav.SetDestination(targetTr.position);
    }

    private void FixedUpdate()
    {
        tick++;
        if (tick >= aiTickCount * targetUpdateTickCount)
        {
            TargetUpdate();
        }
    }

    // 타겟으로 이동 : [AIAggro]  **3틱 배수마다 해야함
    public void TargetUpdate()
    {
        targetTr = aiCon.targetTransform;
        LookatTarget(targetTr.position);
        nav.SetDestination(targetTr.position);
    }

    public override void EndAction()
    {
        base.EndAction();
        this.enabled = false;
    }
}