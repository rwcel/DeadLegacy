using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;
using NetworkBase;

public class AIRushState : AIFightState
{
    [SerializeField] float rushSpeed;
    public bool isRush;

    /// <summary>
    /// Slerp말고 바로 rotation적용하기
    /// 1. 실드 활성화 함수 실행
    /// 2. 네비게이션을 끄고 translate로 이동하기
    /// </summary>
    public override void StartAction()
    {
        base.StartAction();
        this.enabled = true;

        ConditionRegulate(AIController.AIState.RUSH,true);
        // isRush = true;
        StartCoroutine(ShieldOnOff(SkillManager.instance.dictSkill[aiCon.aiInfo.skill.skillIds[activeSkillNum]].guardDuration));
    }

    public override void GuestSkillAction(int skillId, Vector3 targetPos)
    {
        base.GuestSkillAction(skillId, targetPos);
        //this.enabled = true;
        //// isRush = true;
        //activeSkillNum = skillId;
        //skillProcess.StartCoroutine(skillProcess.SkillProcessing((int)AttackSkillArray.SPECIAL, skillId, this.gameObject, attackTr, targetPos));
    }

    private void FixedUpdate()
    {
        if(isRush)
            transform.Translate(transform.forward * rushSpeed * Time.deltaTime, Space.World);
    }

    /// <summary>
    /// 1. 실드 일정시간 활성화
    /// 2. 시간 끝나면 러시 반동 (이미 종료된 경우에는 호출 안함(navmesh로) : 아무것도 맞지 않은 경우 반동이 없음)
    /// </summary>
    /// <param name="time"></param>
    /// <returns></returns>
    IEnumerator ShieldOnOff(float time)
    {
        Debug.Log("들어갑니다 : " + time);
        // 1.
        AILiving entity = GetComponent<AILiving>();
        entity.bShield = true;
        yield return new WaitUntil(() => isRush == false);
        entity.bShield = false;
        Debug.Log("종료됩니다");
        // 2.
        //if (!nav.enabled)
        //    RushRebound();
    }

    // 더이상 앞으로 가지않게 하고 반동 애니메이션 재생
    public void RushRebound()
    {
        isRush = false;
        aiAnim.RushRebound();
    }

        /// <summary>
    /// 애니메이션 이벤트
    /// 1. 위치 이동시키기
    /// 2. 상태 돌려주기
    /// 3. 바로 움직일 수 있게 해주기
    /// </summary>
    /// <param name="_pos"></param>
    public override void EndState(Vector3 _pos)
    {
        base.EndState(_pos);

        Debug.Log("돌진 끝");

        // 2.
        NavMeshHit hit;
        NavMesh.SamplePosition(transform.position, out hit, 2f, NavMesh.AllAreas);
        nav.nextPosition = hit.position;
        // 3.
        ConditionRegulate(AIController.AIState.RUSH,false);

        // 판정오브젝트 제자리로 돌리기 (JudgingSkill 자체처리)
        aiCon.judgeObj = null;

        if (cNetworkIdentity.isHost)
        {
            nav.SetDestination(targetTr.position);

            isSkill = false;
            isAction = false;
            networkAITransmitor.EndFullAction();
            aiCon.EndActionCheckState(AIController.AIState.RUSH);
        }
        else
        {
            this.enabled = false;
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
