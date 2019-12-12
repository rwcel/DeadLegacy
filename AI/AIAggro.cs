using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.AI;

// DB값 참조
public struct AggroData
{
    public float ragePoint;                 // 분노수치 : 맞으면 증가하는 정도
    public float ragePointFactor;           // 분노수치계수
    public float distanceFactor;            // 거리계수
    public float heightFactor;              // 높이계수
    public float targetCountFactor;         // 공격대상 수치계수

    public float aggroTime;                 // 어그로 확인 시간    **No_DB
    public float chaseDist;                 // 추적 거리          **Aiinfo 참조
    public float viewAngle;                 // 시야각
    public float viewDistance;              // 시야거리
    public bool heightRecognition;          // 높이 인식 가능 여부
    public float currentTargetMatch;        // 현재 타겟 보정점수   **No_DB
}

// 어그로 체크 스크립트
public class AIAggro : MonoBehaviour
{
    PlayerAggroInfo tmpPlayerAggroInfo;         // **List의 값을 수정할 수 없어서 임시로 데이터를 저장하는 공간

    public AggroData aggroData;
    float[] m_aggroPoint;

    Transform m_targetTr;                       // 타겟(플레이어) 트랜스폼
    Transform m_secondTargetTr;                 // 두번째로 어그로 높은 타겟(플루이드)

    int m_beforeAggroPlayerNum = -1;             // 최근 어그로가 가장 높은 플레이어
    int m_maxAggroPlayerNum;                    // 가장 높은 어그로 점수를 가진 플레이어
    float m_maxAggroPoint;                      // 가장 높은 어그로 점수

    private void Start()
    {
        tmpPlayerAggroInfo = new PlayerAggroInfo();
        m_aggroPoint = new float[4];
    }

    /// <summary>
    /// id에 해당하는 db데이터 넣기
    /// </summary>
    /// <param name="id"></param>
    public void AggroDBSetting(short id)
    {
        aggroData = CSV_AI.instance.aggroDictionary[id];

        aggroData.aggroTime = 2f;
        aggroData.currentTargetMatch = 20f;
    }

    /// <summary>
    /// 1. 모든 플레이어 어그로 확인(+수치계산 : SetAggroPoint)
    /// 2. 가장 어그로 높은 플레이어 확인
    /// </summary>
    void PlayerAggroCheck()
    {
        // 1.
        // Debug.Log("플레이어 수 : " + AggroManager.Instance.GetConnectedPlayer());
        for (int i = 0; i < AggroManager.Instance.playerAggroList.Count; i++)
        {
            m_aggroPoint[i] = SetAggroPoint(i);
        }

        // 2.
        SetMaxAggroPlayer();
    }

    /// <summary>
    /// 어그로 수치 계산하기 : 거리, 교전중인 적, 현재 타겟인지
    /// RagePoint + (Distance * DistanceFactor) – (Height * HeightFactor) – (TargetCount * TargetCountFactor) + NowTargetCorrection
    /// </summary>
    /// <param name="playerNum">플레이어 번호</param>
    /// <returns></returns>
    float SetAggroPoint(int playerNum)
    {
        float point = 0;

        float _dist = (AggroManager.Instance.playerAggroList[playerNum].playerObject.transform.position - transform.position).sqrMagnitude;

        if (_dist >= aggroData.chaseDist * aggroData.chaseDist)
            _dist = aggroData.chaseDist * aggroData.chaseDist;
        point += (aggroData.chaseDist * aggroData.chaseDist - _dist) * aggroData.distanceFactor;

        float targetCount = AggroManager.Instance.playerAggroList[playerNum].battleAINum * aggroData.targetCountFactor;
        point -= targetCount;

        if (m_targetTr == AggroManager.Instance.playerAggroList[playerNum].playerObject.transform)
            point += aggroData.currentTargetMatch;

        // Debug.Log(gameObject.GetComponent<AIController>().aiInfo.hashcodeId + "의 점수 : " + point);
        return point;
    }

    /// <summary>
    /// 가장 어그로가 높은 플레이어 확인
    /// 1. 어그로 초기화
    /// 2. 어그로가 가장 높은 플레이어의 정보 받기
    /// 3. 두번째 어그로 높은 플레이어 지정
    /// 4. 타겟플레이어 지정
    /// 5. 타겟이 바뀐경우에만
    /// 5-1. 교전중인 적 증감 (처음만 감소 안함. 기존타겟이 없으니까)
    /// </summary>
    void SetMaxAggroPlayer()
    {
        // 1.
        m_maxAggroPlayerNum = -2;
        m_maxAggroPoint = 0;
        
        // 2.
        for (int i = 0; i < AggroManager.Instance.playerAggroList.Count; i++)
        {
            if (m_aggroPoint[i] > m_maxAggroPoint)
            {
                // 3. 
                if (m_maxAggroPlayerNum == -2)
                {
                    m_secondTargetTr = AggroManager.Instance.playerAggroList[i].playerObject.transform;
                }
                else
                    m_secondTargetTr = AggroManager.Instance.playerAggroList[m_maxAggroPlayerNum].playerObject.transform;

                m_maxAggroPlayerNum = i;
                m_maxAggroPoint = m_aggroPoint[i];
            }
        }

        if (m_maxAggroPlayerNum != -2)
        {
            // 4.
            m_targetTr = AggroManager.Instance.playerAggroList[m_maxAggroPlayerNum].playerObject.transform;
            // Debug.Log(m_targetTr.gameObject.name);

            // 5.
            if (m_maxAggroPlayerNum != m_beforeAggroPlayerNum)
            {
                // 5-1.
                if (m_beforeAggroPlayerNum != -1)
                {
                    tmpPlayerAggroInfo = AggroManager.Instance.playerAggroList[m_beforeAggroPlayerNum];
                    tmpPlayerAggroInfo.battleAINum--;
                    AggroManager.Instance.playerAggroList[m_beforeAggroPlayerNum] = tmpPlayerAggroInfo;
                    // AggroManager.Instance.playerAggroList[beforeAggroPlayerNum].battleAINum--;
                }

                tmpPlayerAggroInfo = AggroManager.Instance.playerAggroList[m_maxAggroPlayerNum];
                tmpPlayerAggroInfo.battleAINum++;
                AggroManager.Instance.playerAggroList[m_maxAggroPlayerNum] = tmpPlayerAggroInfo;
                // AggroManager.Instance.playerAggroList[_maxAggroPlayerNum].battleAINum++;

                m_beforeAggroPlayerNum = m_maxAggroPlayerNum;
            }
        }
    }

    // [PlayerController] 자신을 공격한 플레이어 찾아서 수치 올리기
    public void IncreaseRagePoint(GameObject _player)
    {
        for (int i = 0; i < AggroManager.Instance.playerAggroList.Count; i++)
        {
            if (_player == AggroManager.Instance.playerAggroList[i].playerObject.gameObject)
            {
                m_aggroPoint[i] += aggroData.ragePoint * aggroData.ragePointFactor;
            }
        }
    }

    // [AIController] 플레이어 어그로체크해주고 타겟 넘겨주기
    public Transform getTargetTransform()
    {
        PlayerAggroCheck();
        return m_targetTr;
    }

    // 두번째 어그로 높은 플레이어 : Fluid
    public Transform getSecondTarget()
    {
        Debug.Log(m_secondTargetTr.name);
        return m_secondTargetTr;
    }

}