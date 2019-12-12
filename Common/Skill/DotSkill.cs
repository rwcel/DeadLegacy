using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 지속스킬 함수
public class DotSkill : MonoBehaviour
{
    Skill skill;
    GameObject usingObj;

    LayerMask livingLayer;
    [SerializeField] GameObject swarmParticle;

    protected LayerMask targetLayer;	// 타겟 레이어마스크(비트)

    bool[] isObjEnter;

    private void OnEnable()
    {
        livingLayer = 3 << 9;
    }


    public void SkillSetting(Skill _skill, GameObject _usingObj)
    {
        skill = _skill;
        usingObj = _usingObj;

        switch (skill.id)
        {
            // 스웜 파티클
            case SkillManager.JUDGE_SLOW_SPIT:
            case SkillManager.JUDGE_POSION_SPIT:
                swarmParticle.SetActive(true);
                swarmParticle.GetComponent<ParticleSystem>().Play();
                // 일정시간 뒤, 스킬 사라지게 하기
                StartCoroutine(GameObjectActiveFalse(skill.effectDuration));
                break;
        }

        // StartCoroutine(DotOverlapCheck());
    }

    /// <summary>
    /// 1. 레이어 포함되어 있으면 판정
    /// 2. 효과종류에 따라 부여
    /// 3. 컴포넌트가 없으면 효과 컴포넌트를 부여
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        // Debug.Log(other.name);

        // 1.
        targetLayer = (int)skill.judgeTarget << (int)SkillJudgeTarget.PLAYER;
        if ((targetLayer.value & (1 << other.gameObject.layer)) != 0)
        {
            // 2.
            switch (skill.effectKind)
            {
                // 3.
                case SkillEffectKind.SLOW:
                    if (other.gameObject.GetComponent<SlowDebuff>() == null)
                    {
                        other.gameObject.AddComponent<SlowDebuff>();
                        other.gameObject.GetComponent<LivingEntity>().TakeSkillEffect(skill, skill.damage, transform.position, usingObj);
                    }
                    break;
                case SkillEffectKind.POISON:
                    if(other.gameObject.GetComponent<PosionDebuff>() == null)
                    {
                        other.gameObject.AddComponent<PosionDebuff>();
                        other.gameObject.GetComponent<PosionDebuff>().SettingSkill(skill, usingObj);
                    }
                    break;
            }

        }
    }

    /// <summary>
    /// 오브젝트 일정 시간후에 지우기
    /// </summary>
    /// <param name="_time"></param>
    /// <returns></returns>
    IEnumerator GameObjectActiveFalse(float _time)
    {
        yield return new WaitForSeconds(_time);
        gameObject.SetActive(false);
    }
}