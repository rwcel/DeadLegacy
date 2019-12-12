using System.Collections;
using System.Collections.Generic;
using UnityEngine;

// 트리거 발동하는 함수(어떤 이벤트인지는 상관 안함. 그저 발동)
public class EventTrigger : MonoBehaviour
{
    AIEvent[] childEvent;                               // 하위 이벤트(밟으면 실행될 항목)

    bool isEvent = false;                               // 이벤트 발동 여부

    private void Start()
    {
        childEvent = GetComponentsInChildren<AIEvent>();
    }

    /// <summary>
    /// 플레이어가 밟으면 스폰하는 함수
    /// 1. 한번만 스폰하도록 조절
    /// 2. 하위 스폰이벤트 스폰하도록 요청
    /// </summary>
    /// <param name="other"></param>
    private void OnTriggerEnter(Collider other)
    {
        if(other.CompareTag("Player"))
        {
            // 1.
            if(!isEvent)
            {
                if (RoomManager.Instance.m_MyInfo.inGameUserInfo.isHost)
                {
                    Debug.Log("이벤트 트리거 작동");

                    isEvent = true;
                    // Debug.Log(childSpawnEvent.Length);
                    for (int i = 0; i < childEvent.Length; i++)
                    {
                        childEvent[i].Play();
                    }
                }
                else
                {
                    for (int i = 0; i < childEvent.Length; i++)
                    {
                        if(childEvent[i].GetComponent<DeathPoolingEvent>())
                            childEvent[i].Play();
                    }
                }
            }
        }
    }
}
