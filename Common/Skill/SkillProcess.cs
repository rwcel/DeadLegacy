using System.Collections;
using System.Collections.Generic;
using UnityEngine;

/// <summary>
/// DB(CSV_Skill)에서 가져온 데이터들
/// </summary>
public struct SkillInfo
{
    public int skillCount;                  // 스킬 개수
    public int[] skillIds;                  // 스킬 아이디들
    public int usingSkillNum;               // 사용하는 스킬번호
}

public class SkillProcess : MonoBehaviour
{
    public SkillInfo skillInfo;

    AIController aiCon;
    AILiving living;

    // ** AISkillInfo에 넣어버리면 모든 AI가 쿨타임을 공유함
    bool[] skillCooltime;                  // 쿨타임 인지 (NO DB)

    /// <summary>
    /// 스킬 쿨타임초기화
    /// </summary>
    private void Start()
    {
        aiCon = GetComponent<AIController>();
        living = GetComponent<AILiving>();

        // **AI에서 밖에 못씀
        skillInfo = aiCon.aiInfo.skill;

        skillCooltime = new bool[skillInfo.skillCount];

        //Debug.Log(skillCooltime.Length);

        GetComponent<AIController>().DieAction += () =>
        {
            for (int i = 0; i < skillInfo.skillCount; i++)
            {
                skillCooltime[i] = false;
            }
        };
    }

    /// <summary>
    /// AI 스킬 사용 가능한지 확인
    /// 1. 스킬 거리, 쿨타임 확인
    /// 2. 사용 가능하면 스킬번호 지정
    /// </summary>
    /// <param name="dist"></param>
    /// <returns></returns>
    public bool SkillCheck(float dist)
    {
        for (int i = 0; i < skillInfo.skillCount; i++)
        {
            // Debug.Log(i+":"+dist + " <" + SkillManager.instance.dictSkill[skillInfo.skillIds[i]].maxDist * SkillManager.instance.dictSkill[skillInfo.skillIds[i]].maxDist);
            // 1.
            if (!skillCooltime[i] && dist < (SkillManager.instance.dictSkill[skillInfo.skillIds[i]].maxDist * SkillManager.instance.dictSkill[skillInfo.skillIds[i]].maxDist) &&
                                            dist > (SkillManager.instance.dictSkill[skillInfo.skillIds[i]].minDist * SkillManager.instance.dictSkill[skillInfo.skillIds[i]].minDist))
            {
                // 2.
                skillInfo.usingSkillNum = i;
                return true;
            }
        }
        return false;
    }

    // 플레이어 스킬확인
    public bool SkillCheck()
    {
        return false;
    }

    /// <summary>
    /// 스킬 쿨타임 돌리기
    /// 시전시간 대기
    /// 스킬 발동
    /// </summary>
    public IEnumerator SkillProcessing(GameObject obj, Transform startTr, Vector3 endPos)
    {
        // 2.
        int _skillID = skillInfo.skillIds[skillInfo.usingSkillNum];
        StartCoroutine(StartSkillCooltime(skillInfo.usingSkillNum, _skillID));
        yield return new WaitForSeconds(SkillManager.instance.dictSkill[_skillID].castingTime);
        if (!living.isDieP)          // 중간에 죽으면 스킬 못씀
            CastingSkill(_skillID, obj, startTr, endPos);

        if(_skillID == SkillManager.RUSH_ATTACK)
            GetComponent<AIRushState>().isRush = true;
    }

    public IEnumerator SkillProcessing(int arrayNum, int skillId, GameObject obj, Transform startTr, Vector3 endPos)
    {
        StartCoroutine(StartSkillCooltime(arrayNum, skillId));
        yield return new WaitForSeconds(SkillManager.instance.dictSkill[skillId].castingTime);
        if (!living.isDieP)          // 중간에 죽으면 스킬 못씀
            CastingSkill(skillId, obj, startTr, endPos);

        if (skillId == SkillManager.RUSH_ATTACK)
            GetComponent<AIRushState>().isRush = true;
    }

    /// <summary>
    /// 스킬 쿨타임 돌리기 true -> false
    /// </summary>
    /// <param name="_num"></param>
    /// <returns></returns>
    IEnumerator StartSkillCooltime(int skillArray, int skillID)
    {
        // Debug.Log("스킬쿨중 : " + skillID + ", " + SkillManager.instance.dictSkill[skillID].coolTime + "초");
        skillCooltime[skillArray] = true;
        yield return new WaitForSeconds(SkillManager.instance.dictSkill[skillID].coolTime);
        // Debug.Log("스킬쿨끝 "+ skillID);
        skillCooltime[skillArray] = false;
    }

    /// <summary>
    /// 스킬 사용 함수
    /// 1. 스킬매니저에게 사용 요청하기(스킬번호, 오브젝트, 공격위치, 타겟위치, 기본데미지)
    /// </summary>
    protected virtual void CastingSkill(int skillId, GameObject obj, Transform startTr, Vector3 endPos)
    {
        // Debug.Log(skillId);
        SkillManager.instance.Skilling(skillId, this.gameObject, startTr, endPos, aiCon.aiInfo.damage);
    }

}
