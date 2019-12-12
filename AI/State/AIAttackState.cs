using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using NetworkBase;

public class AIAttackState : AIFightState
{
    public override void StartAction()
    {
        base.StartAction();
        this.enabled = true;
    }

    private void FixedUpdate()
    {
        if (cNetworkIdentity.isHost)
        {
            tick++;

            if (tick >= aiTickCount * SkillManager.instance.dictSkill[aiCon.aiInfo.skill.skillIds[activeSkillNum]].reboundTime * 16)
            {
                tick = 0;

                isSkill = false;
                isAction = false;

                CAIPacket _aiPacket = networkAITransmitor.GetAIPacket();
                _aiPacket.SetAIAction(Global.AiAction.AttackEnd);
                networkAITransmitor.EndFullAction();
                aiCon.EndActionCheckState(AIController.AIState.ATTACK);
            }
        }
        else
        {
            CAIPacket _aiPacket = networkAISyncor.GetSyncInfo();

            if (_aiPacket == null)
                return;
            if (_aiPacket.m_Action == Global.AiAction.AttackEnd)
            {
                //attack 끝처리
                isAction = false;
                _aiPacket.DeFlagAction();
                networkAISyncor.CheckSyncEnd(_aiPacket);
                networkAISyncor.EndFullAction();
                this.enabled = false;
            }
            else if (_aiPacket.m_Action == Global.AiAction.KnockBack)
            {
                //공격 캔슬 후 넉백처리 
                Vector3 _knockBackPos = _aiPacket.m_TargetPos;
                AIKnockBackState _knockbackState = GetComponent<AIKnockBackState>();
                _knockbackState.GuestSkillAction(0, _knockBackPos);
                _aiPacket.DeFlagAngleV();
                _aiPacket.DeFlagAiPos();
                _aiPacket.DeFlagAction();
                networkAISyncor.CheckSyncEnd(_aiPacket);
                this.enabled = false;
            }
        }
    }

    /// <summary>
    /// 스킬 반동시간 먹이기 : 행동 불가능
    /// **반동때 어떤 걸 할지 정해주기 : ex. isStun + aiAnim.Stun
    /// **nav.isstopped는 leaping, rush등 문제 생길 수 있음 (스킬 끝나고 하는걸로?)
    /// + 이후에 일은 isAction이 해제되었으니 Aicontroller자체적으로 실행가능(= tickCount맞출 필요 없음)
    /// </summary>
    //public override IEnumerator SkillRebound(float _time)
    //{
    //    yield return new WaitForSeconds(_time);

    //    if (aiCon.aiState == AIController.AIState.FIGHT)
    //    {
    //        if (cNetworkIdentity.isHost)
    //        {

    //        }
    //        else
    //        {
    //            Debug.Log("완전공격끝 처리");
    //            CAIPacket _aiPacket = networkAISyncor.GetSyncInfo();
    //        }
    //    }
    //}

    public override void EndAction()
    {
        base.EndAction();
        this.enabled = false;
    }
}
