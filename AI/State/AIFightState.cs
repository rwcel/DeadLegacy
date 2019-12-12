using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkBase;

public class AIFightState : AIBaseState
{
    // 공격 위치 트랜스폼  **스킬마다 위치가 다를텐데 어떻게 할지
    [SerializeField] protected Transform attackTr;

    protected int reboundTick = 0;
    protected int reboundTickCount;
    Coroutine reboundCor;

    /// <summary>
    /// 1. 스킬 사용도중 다른 스킬 사용 못하게 막아두기
    /// 2. 목적지까지 연결하는 패스 지우기  (공격 시 플레이어에게 다가오지 않음)
    /// 3. 스킬 사용
    /// </summary>
    public override void StartAction()
    {
        base.StartAction();
        isAction = true;

        targetTr = aiCon.targetTransform;

        // 1.
        activeSkillNum = skillProcess.skillInfo.usingSkillNum;

        //reboundTickCount = (int)(SkillManager.instance.dictSkill[aiCon.aiInfo.skill.skillIds[activeSkillNum]].reboundTime * 25);

        // 2.
        if (nav.enabled)
        {
            nav.velocity = Vector3.zero;
            nav.ResetPath();
        }

        // 3.
        skillProcess.StartCoroutine(skillProcess.SkillProcessing(this.gameObject, attackTr, targetTr.position));
    }

    /// <summary>
    /// 게스트 스킬사용
    /// </summary>
    /// <param name="skillId">스킬번호</param>
    /// <param name="targetPos">타겟위치</param>
    public override void GuestSkillAction(int skillId, Vector3 targetPos)
    {
        this.enabled = true;
        isAction = true;
        activeSkillNum = skillId;
        skillProcess.StartCoroutine(skillProcess.SkillProcessing((int)AttackSkillArray.NORMAL, skillId, this.gameObject, attackTr, targetPos));
        //StartCoroutine(SkillRebound(SkillManager.instance.dictSkill[skillId].reboundTime));
    }

    public override IEnumerator SkillRebound(float _time)
    {
        yield return new WaitForSeconds(_time);

        if (cNetworkIdentity.isHost)
        {
            isSkill = false;
            isAction = false;
            networkAITransmitor.EndFullAction();
            aiCon.EndActionCheckState(AIController.AIState.SPIT);
        }
        else
        {
            Debug.Log("완전공격끝 처리");
            CAIPacket _aiPacket = networkAISyncor.GetSyncInfo();
            _aiPacket.DeFlagAction();
            networkAISyncor.CheckSyncEnd(_aiPacket);
            networkAISyncor.EndFullAction();
        }
    }
}
