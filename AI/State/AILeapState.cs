using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkBase;

public class AILeapState : AIFightState
{
    bool isLanding;             // 착륙가능

    /// <summary>
    /// ** 1. 도약상태로 만들기 : ProjectionSkill
    /// 2. 최정상에 위치했을 때 착륙가능 표시
    /// 3. 바닥근처인지 확인하고 상태 바꿔주기
    /// 4. 스킬사용하기(넉백)
    /// </summary>
    public override void StartAction()
    {
        base.StartAction();
        this.enabled = true;

        nav.avoidancePriority = highProperty;
        LookatTarget(targetTr.position);

        StartCoroutine(Leaping());
    }

    public override void GuestSkillAction(int skillId, Vector3 targetPos)
    {
        base.GuestSkillAction(skillId, targetPos);
        //this.enabled = true;
        //isAction = true;
        //activeSkillNum = skillId;
        //skillProcess.StartCoroutine(skillProcess.SkillProcessing((int)AttackSkillArray.SPECIAL, skillId, this.gameObject, attackTr, targetPos));
        StartCoroutine(GuestLeaping(targetPos));
    }

    IEnumerator Leaping()
    {
        yield return new WaitUntil(() => !Physics.Raycast(transform.position, -transform.up, groundHeight, groundLayer));
        while (true)
        {
            //2. * *스위치 하나 더 만들어주기
            if (isAction && Mathf.Abs(rd.velocity.y) < 0.2f)
            {
                isLanding = true;
            }
            // 3.
            else if (isLanding && LandingAndStatusCheck(AIController.AIState.LEAP))
            {
                Debug.Log("끝");
                EndState(transform.position);
                break;
            }
            yield return new WaitForFixedUpdate();
        }
    }

    IEnumerator GuestLeaping(Vector3 targetPos)
    {
        yield return new WaitUntil(() => !Physics.Raycast(transform.position, -transform.up, groundHeight, groundLayer));
        while (true)
        {
            //2. * *스위치 하나 더 만들어주기
            if (isAction && Mathf.Abs(rd.velocity.y) < 0.2f)
            {
                isLanding = true;
            }
            // 3.
            else if (isLanding && LandingAndStatusCheck(targetPos, AIController.AIState.LEAP))
            {
                Debug.Log("끝");
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
            SkillManager.instance.Skilling(SkillManager.JUDGE_LEAP, this.gameObject, transform, targetTr.position, aiCon.aiInfo.damage);
            isAction = false;
            isLanding = false;
            LookatTarget(targetTr.position);
            networkAITransmitor.EndFullAction();
            aiCon.EndActionCheckState(AIController.AIState.LEAP);
        }
        else
        {
            SkillManager.instance.Skilling(SkillManager.JUDGE_LEAP, this.gameObject, transform, _pos, aiCon.aiInfo.damage);
            this.enabled = false;
            isAction = false;
            Debug.Log("완전공격끝 처리");
            CAIPacket _aiPacket = networkAISyncor.GetSyncInfo();
            _aiPacket.DeFlagAction();
            networkAISyncor.CheckSyncEnd(_aiPacket);
            networkAISyncor.EndFullAction();
        }
    }

    public override void EndAction()
    {
        base.EndAction();
        this.enabled = false;
    }
}
