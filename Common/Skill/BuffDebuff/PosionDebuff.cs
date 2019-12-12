using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PosionDebuff : BuffDebuff
{
    // 일정시간마다 데미지 주기
    protected override IEnumerator BuffDebuffEffect()
    {
        base.BuffDebuffEffect();

        while (time < skill.effectDuration)
        {
            living.TakeDamage(skill, skill.effectPower, transform.position, attackObj);
            yield return new WaitForSeconds(skill.effectTime);
            time += skill.effectTime;
        }
        Destroy(this);
    }
}