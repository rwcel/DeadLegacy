using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// **하위Component들이 Update로 돌아가는 것에 대해서 고민해야함
public class BuffDebuff : MonoBehaviour
{
    protected LivingEntity living;
    protected Skill skill;
    protected GameObject attackObj;

    protected float time;

    protected void Start()
    {
        living = GetComponent<LivingEntity>();
        time = 0;

        // Debug.Log("디버프효과받음");
        StartCoroutine(BuffDebuffEffect());
    }

    /// <summary>
    /// 정보 셋팅하기
    /// </summary>
    /// <param name="_skill">스킬정보</param>
    /// <param name="_attackObj">공격한 오브젝트</param>
    public void SettingSkill(Skill _skill, GameObject _attackObj)
    {
        skill = _skill;
        attackObj = _attackObj;
    }

    // 버프/디버프 효과들
    protected virtual IEnumerator BuffDebuffEffect()
    {
        yield return null;
    }
}
