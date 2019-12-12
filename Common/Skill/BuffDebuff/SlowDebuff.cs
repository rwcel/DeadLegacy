using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// **PainKillerBuff와 recovery 의미가 반대임
public class SlowDebuff : BuffDebuff
{
    float recovery;         // 회복 비율
    float beforeSpeed;      // 기존 속도

    // 일정시간 슬로우
    protected override IEnumerator BuffDebuffEffect()
    {
        base.BuffDebuffEffect();

        beforeSpeed = living.speed;
        living.speed *= (100 - skill.effectPower) * 0.01f;
        recovery = living.speed - beforeSpeed;
        recovery /= (skill.effectDuration / skill.effectTime);

        do
        {
            yield return new WaitForSeconds(skill.effectTime);
            Debug.Log("회복량 : " + recovery);
            living.speed += recovery;
            time += skill.effectTime;
        } while (time < skill.effectDuration);

        Destroy(this);
    }
}