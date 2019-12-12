using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PainKillerBuff : BuffDebuff
{
    float decreaseHp;       // 줄어들 hp값
    float recoveryHp;       // 회복된 체력(Max를 넘기면 체력이 더 줄기때문에)

    /// <summary>
    /// 1. 한번만 회복 후 
    /// 2. 임시체력 감소 : 일정시간 후에 일정 주기마다 점점줄어듦
    /// 50회복 -> 총10초 1초마다, 50/(10/1) 5씩 깎임
    /// 40회복 -> 총10초 2초마다, 40/(10/2) 8씩 깎임
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator BuffDebuffEffect()
    {
        base.BuffDebuffEffect();

        decreaseHp = skill.effectDuration / skill.effectTime;
        recoveryHp = living.maxHp - living.currentHp;
        decreaseHp = recoveryHp / decreaseHp;

        // 1.
        living.currentHp += skill.effectPower;
        if (living.currentHp > living.maxHp)
            living.currentHp = living.maxHp;
        do
        {
            yield return new WaitForSeconds(skill.effectTime);
            //Debug.Log("감소량 : " + decreaseHp);
            // 2.
            living.currentHp -= decreaseHp;
            time += skill.effectTime;
        } while (time < skill.effectDuration);

        Destroy(this);
    }
}
