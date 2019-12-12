using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkBase;

public class AIKnockBackState : AIBaseState
{
    bool isLanding;             // 착륙가능

    public override void StartAction()
    {
        base.StartAction();
        targetTr = aiCon.targetTransform;
        isAction = true;

        ConditionRegulate(AIController.AIState.KNOCKBACK, true);
        // 밀려날 자리 만들기
        nav.avoidancePriority = highProperty;
        rd.velocity = Vector3.zero;

        StartCoroutine(KnockBacking());
    }

    /// <summary>
    /// 스킬사용은 따로 안하고 밀려나는 것만 하기
    /// </summary>
    /// <param name="skillId"></param>
    /// <param name="targetPos"></param>
    public override void GuestSkillAction(int skillId, Vector3 targetPos)
    {
        isAction = true;
        ConditionRegulate(AIController.AIState.KNOCKBACK, true);
        StartCoroutine(GuestKnockbacking(targetPos));
    }

    /// <summary>
    /// 1. 바닥에서 떨어지는 최소시간 대기
    /// 2. 바닥근처인지 확인하고 상태 바꾸기
    /// </summary>
    /// <returns></returns>
    IEnumerator KnockBacking()
    {
        yield return new WaitUntil(() => !Physics.Raycast(transform.position, -transform.up, groundHeight, groundLayer));
        Debug.Log("넉백시작");
        while(true)
        {
            if (isAction && Mathf.Abs(rd.velocity.y) < 0.2f)
            {
                isLanding = true;
                col.isTrigger = false;
            }
            else if (isLanding && LandingAndStatusCheck(AIController.AIState.KNOCKBACK))
            {
                Debug.Log("넉백끝");
                EndState(transform.position);
                break;
            }
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator GuestKnockbacking(Vector3 targetPos)
    {
        rd.AddForce((transform.position - new Vector3(targetPos.x, transform.position.y, targetPos.z)).normalized * 4f + Vector3.up * 6f, ForceMode.Impulse);
        yield return new WaitUntil(() => !Physics.Raycast(transform.position, -transform.up, groundHeight, groundLayer));
        Debug.Log("넉백시작");
        while (true)
        {
            if (isAction && Mathf.Abs(rd.velocity.y) < 0.2f)
            {
                isLanding = true;
                col.isTrigger = false;
            }
            else if (isLanding && LandingAndStatusCheck(targetPos, AIController.AIState.KNOCKBACK))
            {
                Debug.Log("넉백끝");
                EndState(transform.position);
                break;
            }
            yield return new WaitForFixedUpdate();
        }
    }

    public override void EndState(Vector3 _pos)
    {
        base.EndState(_pos);

        if (cNetworkIdentity.isHost)
        {
            living.bKnockback = false;
            isAction = false;
            isLanding = false;
            networkAITransmitor.EndFullAction();
            aiCon.EndActionCheckState(AIController.AIState.KNOCKBACK);
        }
        else
        {
            this.enabled = false;
            isAction = false;
            living.bKnockback = false;
            isLanding = false;
            Debug.Log("완전공격끝 처리");           
            networkAISyncor.EndFullAction();
        }
    }

}
