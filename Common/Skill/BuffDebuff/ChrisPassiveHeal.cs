using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ChrisPassiveHeal : BuffDebuff
{
    [SerializeField] float healValue;
    [SerializeField] float waitTime;

    /// <summary>
    /// 1. 한번은 무조건 회복하게 하기 (즉시회복의 경우)
    /// 2. Max Hp초과는 불가능
    /// </summary>
    /// <returns></returns>
    protected override IEnumerator BuffDebuffEffect()
    {
        base.BuffDebuffEffect();

        // 1.
        while(true)
        {
            living.currentHp += healValue;
            // 2.
            if (living.currentHp > living.maxHp)
                living.currentHp = living.maxHp;

            yield return new WaitForSeconds(waitTime);
            // **time.deltaTime써도 되는데 시간차이 거의 안남
            time += waitTime;
        }
    }
}
