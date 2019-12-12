using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 타입에 맞춰 오브젝트 풀링해주는 함수
public class SpawnPooling : MonoBehaviour
{
    SpawnEvent spawnEvent;

    int m_idNum;
    int m_spawnArrayNum;
    int m_spawnNum;          // 한번에 스폰개수
    int m_spawnCount;        // 몇번 스폰할지

    private void Start()
    {
        spawnEvent = gameObject.GetComponent<SpawnEvent>();
    }
    /// <summary>
    /// 끝나고 몇번 배열을 부를지
    /// 1. aiSpawn에 맞는 번호를 찾아 스폰 실행시키기
    /// </summary>
    /// <param name="spawnInfo">스폰 정보</param>
    /// <param name="num">배열 번호</param>
    public void Pooling(SpawnInfo spawnInfo, int num)
    {
        m_spawnArrayNum = num;
        for (int i = 0; i < AIPoolingManager.instance.aiPools.Length; i++)
        {
            // 1.
            if (spawnInfo.prefabID == AIPoolingManager.instance.aiPools[i].id)
            {
                // Debug.Log(idNum);
                m_idNum = i;
                StartCoroutine(CreateSpawn(spawnInfo, m_idNum));
                break;
            }
        }
    }

    /// <summary>
    /// ai 오브젝트 풀링하는 함수
    /// 1. 초기 딜레이
    /// 2. 정해진 수 꺼내줄때까지 계속
    /// 3. 넷코드 확인해서 스폰매니저로부터 꺼내게하기
    /// 4. 다음스포너 부르기
    /// </summary>
    /// <param name="aiSpawn">스포너 정보</param>
    /// <param name="idNum">id코드</param>
    /// <returns></returns>
    IEnumerator CreateSpawn(SpawnInfo aiSpawn, int idNum)
    {
        m_spawnNum = 0;
        m_spawnCount = 0;

        // 1.
        yield return new WaitForSeconds(aiSpawn.delayTime);

        // 2.
        while (m_spawnCount < aiSpawn.spawnCount)
        {
            for (int i = 0; i < AIPoolingManager.instance.aiPools[idNum].max; i++)
            {
                // 3.
                short _netcode = spawnEvent.netcodeDictionary[aiSpawn.prefabID].Dequeue();
                AIPoolingManager.instance.SpawnObject(aiSpawn.prefabID, _netcode, transform.position, transform.rotation.y);
                m_spawnNum++;
                // 소환 개체 수 만족하면 넘어가기
                if (m_spawnNum >= aiSpawn.spawnNum)
                {
                    m_spawnNum = 0;
                    break;
                }
            }
            m_spawnCount++;
            yield return new WaitForSeconds(aiSpawn.spawnTerm);
        }

        // 4.
        if(++m_spawnArrayNum < spawnEvent.spawnInfo.Length)
        {
            // if (spawnEvent.spawnInfo[m_spawnArrayNum].spawnActionTime == SpawnActionTime.SEQUENCE)
                spawnEvent.spawnPlay(m_spawnArrayNum);
        }
    }
}