using UnityEngine;
using System.Collections;
using System.Collections.Generic;
using System.Text.RegularExpressions;

public class CSV_Skill : MonoBehaviour
{
    byte skillID;
    Skill skill;

    int tmp;

    void Awake()
    {
        List<Dictionary<string, object>> data = CSVReader.Read("SkillDB");
        //Debug.Log("딕셔너리 수 :" + data.Count);

        for (var i = 0; i < data.Count; i++)
        {
            byte.TryParse(data[i]["ID"].ToString(), out skillID);
            byte.TryParse(data[i]["ID"].ToString(), out skill.id);
            //Debug.Log(skill.id);

            float.TryParse(data[i]["MinDist"].ToString(), out skill.minDist);
            float.TryParse(data[i]["MaxDist"].ToString(), out skill.maxDist);
            float.TryParse(data[i]["CoolTime"].ToString(), out skill.coolTime);
            float.TryParse(data[i]["CastingTime"].ToString(), out skill.castingTime);
            float.TryParse(data[i]["ReboundTime"].ToString(), out skill.reboundTime);
            float.TryParse(data[i]["GuardAxis"].ToString(), out skill.guardAxis);
            float.TryParse(data[i]["GuardDuration"].ToString(), out skill.guardDuration);

            int.TryParse(data[i]["SkillType"].ToString(), out tmp);
            skill.skillType = (SkillType)tmp;
            int.TryParse(data[i]["JudgeTarget"].ToString(), out tmp);
			skill.judgeTarget = (SkillJudgeTarget)tmp;
			int.TryParse(data[i]["JudgeShape"].ToString(), out tmp);
			skill.judgeShape = (SkillJudgeShape)tmp;
			float.TryParse(data[i]["JudgeTime"].ToString(), out skill.judgeTime);
            float.TryParse(data[i]["JudgeRange"].ToString(), out skill.judgeRange);
            float.TryParse(data[i]["JudgeAxis"].ToString(), out skill.judgeAxis);
            float.TryParse(data[i]["JudgeOffset"].ToString(), out skill.judgeOffset);

            float.TryParse(data[i]["Damage"].ToString(), out skill.damage);
            int.TryParse(data[i]["Infection"].ToString(), out skill.infection);

            float.TryParse(data[i]["EffectDuration"].ToString(), out skill.effectDuration);
            float.TryParse(data[i]["EffectTime"].ToString(), out skill.effectTime);
            float.TryParse(data[i]["EffectPower"].ToString(), out skill.effectPower);
            int.TryParse(data[i]["EffectKind"].ToString(), out tmp);
            skill.effectKind = (SkillEffectKind)tmp;
            int.TryParse(data[i]["AddEffectID"].ToString(), out skill.addEffectID);


            SkillManager.instance.dictSkill.Add(skillID, skill);
        }
    }
}
