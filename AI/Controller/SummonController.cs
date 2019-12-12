using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SummonController : MonoBehaviour
{
    [SerializeField] int m_id;
    [HideInInspector] public AIInfo aiInfo;

    // 소환이 되면 스킬사용
    private void Start()
    {
        // Debug.Log("소환 : " + aiInfo.id);
        m_id = aiInfo.id;
        StartCoroutine(CastingSkill());
    }

    /// <summary>
    /// SkillProcess 안거치고 바로 SkillManager로 가게하기
    /// 1. 소환시간동안 대기
    /// 2. 스킬발동
    /// 3. 스킬사용 후 재사용할 수 있게 넣기
    /// </summary>
    /// <returns></returns>
    IEnumerator CastingSkill()
    {
        for (int i = 0; i < aiInfo.skill.skillCount; i++)
        {
            // 1.
            yield return new WaitForSeconds(SkillManager.instance.dictSkill[aiInfo.skill.skillIds[i]].castingTime);
            // 2.
            SkillManager.instance.Skilling(aiInfo.skill.skillIds[i], this.gameObject, gameObject.transform, gameObject.transform.position, aiInfo.damage);
        }

        // 3. ***
        AIPoolingManager.instance.UnSpawnObject(aiInfo.id, this.gameObject);
    }
}
