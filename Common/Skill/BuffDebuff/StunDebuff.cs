using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StunDebuff : BuffDebuff
{
    // 일정시간 스턴
    protected override IEnumerator BuffDebuffEffect()
    {
        base.BuffDebuffEffect();

        living.bStun = true;
        yield return new WaitForSeconds(skill.effectDuration);
        living.bStun = false;
        Destroy(this);
    }
}