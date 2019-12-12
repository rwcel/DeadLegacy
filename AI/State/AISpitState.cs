using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkBase;

public class AISpitState : AIFightState
{

    /// <summary>
    /// 1. 스킬 사용도중 다른 스킬 사용 못하게 막아두기
    /// 2. 목적지까지 연결하는 패스 지우기  (공격 시 플레이어에게 다가오지 않음)
    /// 3. 스킬 사용
    /// </summary>
    public override void StartAction()
    {
        base.StartAction();
        this.enabled = true;

        StartCoroutine(SkillRebound(SkillManager.instance.dictSkill[aiCon.aiInfo.skill.skillIds[activeSkillNum]].reboundTime));
    }

    /// <summary>
    /// 게스트 스킬사용
    /// </summary>
    /// <param name="skillId">스킬번호</param>
    /// <param name="targetPos">타겟위치</param>
    public override void GuestSkillAction(int skillId, Vector3 targetPos)
    {
        base.GuestSkillAction(skillId, targetPos);
        //activeSkillNum = skillId;
        //skillProcess.StartCoroutine(skillProcess.SkillProcessing((int)AttackSkillArray.SPECIAL, skillId, this.gameObject, attackTr, targetPos));
        //StartCoroutine(SkillRebound(SkillManager.instance.dictSkill[skillId].reboundTime));
    }

    //public override void EndState(Vector3 _pos)
    //{
    //    base.EndState(_pos);

    //    Debug.Log("끝");

    //    if (cNetworkIdentity.isHost)
    //    {
    //        isAction = false;
    //        aiCon.EndActionCheckState(AIController.AIState.FIGHT);
    //    }
    //    else
    //    {
    //        Debug.Log("완전공격끝 처리");
    //        CAIPacket _aiPacket = networkAISyncor.GetSyncInfo();
    //        _aiPacket.DeFlagAction();
    //        networkAISyncor.CheckSyncEnd(_aiPacket);
    //        networkAISyncor.EndFullAction();
    //    }
    //}

    public override void EndAction()
    {
        base.EndAction();

        this.enabled = false;
    }
}
