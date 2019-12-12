using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct SpawnSkillObj
{
    public GameObject prefab;        // ai 객체 종류
    public int max;                  // 만들 개수
                                     // 풀링하기위해 넣을 리스트
    [HideInInspector] public List<GameObject> skillPool;
}

public class SkillPoolingManager : MonoBehaviour
{
    public SpawnSkillObj[] skillObj;

    public static SkillPoolingManager instance = null;      // 싱글톤

    // 리스트 생성
    private void Awake()
    {
        instance = this;

        for (int i = 0; i < skillObj.Length; i++)
        {
            skillObj[i].skillPool = new List<GameObject>();
        }
    }

    // 풀링 객체들 생성하기
    void Start()
    {
        for (int i = 0; i < skillObj.Length; i++)
        {
            GameObject _parent = new GameObject();
            _parent.name = skillObj[i].prefab.name + "_Pool";

            for (int j = 0; j < skillObj[i].max; j++)
            {   
                GameObject _obj = Instantiate(skillObj[i].prefab);
                _obj.name = skillObj[i].prefab.name + "_" + j.ToString();
                _obj.SetActive(false);
                _obj.transform.SetParent(_parent.transform);

                skillObj[i].skillPool.Add(_obj);
            }
        }
    }
}