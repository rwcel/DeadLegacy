using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// DB로 받을 것들
public struct Skill
{
    // 스킬 당 시전가능 거리, 쿨타임 필요           -- 기본공격도 스킬로 판단
    [Header("[기본 수치]")]            // +받아올 변수들
    public byte id;                         // +스킬 넘버 : 다섯자리의 숫자로 이루어짐
    public SkillType skillType;             // 스킬종류 구분
// public bool bCancel;                 // 캔슬 가능한 스킬인지 (애니메이션에서 옮겨갈 수 있는가)  **AnyState에서 이동하는거 변경해야함[보스
    public float minDist;                   // 스킬불가 사정거리 (스킬 사정거리 최소)
    public float maxDist;                   // 스킬가능 사정거리 (스킬 사정거리 최대)
    public float coolTime;                  // 쿨타임 시간
    public float castingTime;               // 시전 시간 (시전 후 판정내리기)
    public float reboundTime;               // 반동 시간 (행동불가 시간)
    [Header("[판정 수치]")]
    public SkillJudgeShape judgeShape;      // 스킬모양
    public SkillJudgeTarget judgeTarget;    // 스킬타겟
    public float judgeTime;                 // 판정 시간
    public float judgeRange;                // 범위 거리
    public float judgeAxis;                 // 범위 폭 (직선의 폭, 부채꼴 각도)
    public float judgeOffset;               // 판정 위치(transform.forward + 값)
    [Header("[적용 수치]")]
  //public bool btargetObject;              // 오브젝트를 타겟으로 지정가능한지
    public float damage;                    // +데미지
    public int infection;                   // 감염치
    [Header("[효과 수치]")]
    public SkillEffectKind effectKind;      // 효과 종류
    public float effectDuration;            // 효과 지속시간
    public float effectTime;                // 효과 빈도
    public float effectPower;               // 효과 위력(슬로우 퍼센트, 독 데미지 등)
    public int addEffectID;                 // 추가 효과 아이디
    [Header("[막는 수치]")]
    public float guardAxis;                 // 막는 각도
    public float guardDuration;             // 막기 지속시간
}

public class SkillManager : MonoBehaviour
{
    #region 스킬아이디
    public const int THROW_GRENADE = 10;
    public const int JUDGE_GRENADE = 11;
    public const int SHIELD_KNOCKBACK = 20;
    public const int SHIELD_RUSH = 21;

    //public const int NORMAL_ATTACK = 20001;
    //public const int NORMAL_ATTACK2 = 20002;
    //public const int BITE_ATTACK = 20003;
    public const int SLOW_SPIT = 110;
    public const int JUDGE_SLOW_SPIT = 111;
    public const int SLOW_SPIT2 = 112;
    public const int POSION_SPIT = 120;
    public const int JUDGE_POSION_SPIT = 121;
    public const int LEAP_ATTACK = 130;
    public const int JUDGE_LEAP = 131;
    public const int RUSH_ATTACK = 140;

    //public const int HEALING = 20050;

    //public const int SUMMON_AI = 20100;
    public const int SELF_DESTRUCT = 201;
    #endregion

    Skill activeSkill;                      // 사용하는 스킬
    float sumDamage;                        // 기본데미지 + 스킬데미지

    public static SkillManager instance = null;   // 싱글톤

    //                id  스킬구조체
    public Dictionary<int, Skill> dictSkill;

    private void Awake()
    {
        instance = this;
        dictSkill = new Dictionary<int, Skill>();
    }

    /// <summary>
    /// 스킬 AI, 캐릭터에서 받아 발동하기
    /// **여기서 판정시간을 기다리면 안됨(싱글톤이기 때문에 다른 객체가 접근을 제대로 못함)
    /// </summary>
    /// <param name="_id"> 스킬아이디 </param>
    /// <param name="_obj">사용 오브젝트</param>
    /// <param name="_startTr">사용트랜스폼(Vector, Quaternion)</param>
    /// <param name="_endPos">타겟위치</param>
    /// <param name="_damage">오브젝트 데미지</param>
    public void Skilling(int _id, GameObject _obj, Transform _startTr, Vector3 _endPos, float _damage)
    {
        Debug.Log(_id);
        activeSkill = dictSkill[_id];

        sumDamage = _damage * activeSkill.damage;

        Debug.Log(" 번호 : " + _id + ", 데미지 : " + sumDamage + ", 스킬타입 : "+activeSkill.skillType);

        for (int i = 0; i < SkillPoolingManager.instance.skillObj[(int)activeSkill.skillType].skillPool.Count; i++)
        {
            GameObject skillObj = SkillPoolingManager.instance.skillObj[(int)activeSkill.skillType].skillPool[i];
            if (!skillObj.activeSelf)
            {
                skillObj.transform.position = _startTr.position;
                skillObj.transform.rotation = _startTr.rotation;
                skillObj.transform.localScale = new Vector3(activeSkill.judgeRange, activeSkill.judgeRange, activeSkill.judgeRange);
                skillObj.SetActive(true);

                switch (activeSkill.skillType)
                {
                    // 플레이어는 호스트만 판정할 수 있게
                    case SkillType.JUDGE:
                        
                        skillObj.GetComponent<JudgingSkill>().SkillJudgement(activeSkill, _obj, _endPos, sumDamage);
                        break;
                    case SkillType.PROJECTION:
                        skillObj.GetComponent<ProjectionSkill>().ThrowThing(activeSkill, _obj, _endPos, sumDamage);
                        break;
                    case SkillType.SUMMON:
                        //**Player랑 다르게 해야함
                        _endPos = _startTr.position + _startTr.forward * activeSkill.judgeOffset;
                        skillObj.GetComponent<SummonSkill>().SummonObj(activeSkill, _endPos);
                        break;
                    case SkillType.DOT:
                        skillObj.GetComponent<DotSkill>().SkillSetting(activeSkill, _obj);
                        break;
                }
                break;
            }
        }
    }
}
