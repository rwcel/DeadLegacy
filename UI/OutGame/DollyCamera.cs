using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class DollyCamera : MonoBehaviour
{
    public CinemachineSmoothPath smoothPath;
    CinemachineDollyCart cinemachineDollyCart;

    [SerializeField] float normalSpeed = 5f;
    [SerializeField] float skipMulSpeed = 4f;

    private void Start()
    {
        cinemachineDollyCart = gameObject.GetComponent<CinemachineDollyCart>();
    }

    /// <summary>
    /// 업데이트지만 DollyEndOpenPanel이후 setActive(False)를 하기때문에 문제 없음
    /// -> 여러번 호출 안하고 연산계속 안잡아먹음 ㅇㅇ
    /// 1. 카메라 이동중에 한번 더 누르면 속도 가속화
    /// 
    /// </summary>
    public void Update()
    {
        if (Input.anyKeyDown)
        {
            if(Mathf.Abs(cinemachineDollyCart.m_Speed) < normalSpeed * skipMulSpeed)
                cinemachineDollyCart.m_Speed *= skipMulSpeed;
        }
        if (cinemachineDollyCart.m_Speed > 0 && cinemachineDollyCart.m_Position >= smoothPath.PathLength)
        {
            DollyEndOpenPanel();
        }
        else if(cinemachineDollyCart.m_Speed < 0 && cinemachineDollyCart.m_Position == 0)
        {
            DollyEndOpenPanel();
        }
    }

    /// <summary>
    /// 카트가 목적지에 도달한 경우
    /// 1. 패널 열기
    /// 2. 상태값 원래대로 돌려주기
    /// </summary>
    public void DollyEndOpenPanel()
    {
        // 1.
        DollyCartManager.instance.CartEnd();

        // 2.
        cinemachineDollyCart.m_Speed = normalSpeed;
    }
}
