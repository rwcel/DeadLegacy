using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// 소환스킬 함수
public class SummonSkill : MonoBehaviour
{
    Skill skill;

    /// <summary>
    /// 네비게이션 깔려있는 곳에 생성하기
    /// skill.effectPower의 번호를 소환하기(풀링)
    /// </summary>
    /// <param name="_skill"></param>
    /// <param name="_summonPos"></param>
    public void SummonObj(Skill _skill, Vector3 _summonPos)
    {
        skill = _skill;

        NavMeshHit hit;
        if(NavMesh.SamplePosition(_summonPos, out hit, skill.judgeRange, NavMesh.AllAreas))
        {
            // GameObject _minion = null;
            // ***
            for (int i = 0; i < AIPoolingManager.instance.aiPools.Length; i++)
            {
                //Debug.Log(skill.effectPower);
                //Debug.Log(AIPullingManager.instance.pools[i].id);
                if (skill.effectPower == AIPoolingManager.instance.aiPools[i].id)
                {
                    // _minion = 
                    AIPoolingManager.instance.SpawnObject((short)skill.effectPower, _summonPos);
                    break;
                }
            }
            gameObject.SetActive(false);
        }


    }
}
