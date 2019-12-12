using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;

public class DollyCartManager : MonoBehaviour
{
    public Camera cam;

    [Header("<돌리트랙>")]
    [SerializeField] GameObject dollyCart;
    public GameObject loginTrack;
    public GameObject singleTrack;
    public GameObject multiTrack;
    public GameObject creditTrack;

    CinemachineSmoothPath smoothPath;
    //CinemachineDollyCart dollyCart;

    // 싱글톤 선언
    public static DollyCartManager m_instance;
    public static DollyCartManager instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<DollyCartManager>();
            }

            return m_instance;
        }
    }

    /// <summary>
    /// 카트 움직이기
    /// 1. 패스 지정해주기
    /// 2. 카메라 위치 등변경하기
    /// 3. 카트 active해서 움직이게 하기
    /// </summary>
    /// <param name="_dollyTrack">움직일 트랙</param>
    /// <param name="_curPanel">현재 패널</param>
    /// <param name="_nextPanel">다음(도착) 패널</param>
    /// /// <param name="_speed">속도</param>
    public void DollyCameraMove(GameObject _dollyTrack, GameObject _curPanel, GameObject _nextPanel, int _speed)
    {
        UIManager.instance.CartStartClosePanel(_curPanel, _nextPanel);

        smoothPath = _dollyTrack.GetComponent<CinemachineSmoothPath>();

        // 1.
        dollyCart.GetComponent<CinemachineDollyCart>().m_Path = smoothPath;
        dollyCart.GetComponent<CinemachineDollyCart>().m_Speed *= _speed;
        if (_speed > 0)
            dollyCart.GetComponent<CinemachineDollyCart>().m_Position = 0f;
        dollyCart.GetComponent<DollyCamera>().smoothPath = smoothPath;

        // 2.
        cam.gameObject.transform.SetParent(dollyCart.transform);
        cam.transform.position = dollyCart.transform.position;
        cam.transform.rotation = dollyCart.transform.rotation;

        // 3.
        dollyCart.SetActive(true);
    }

    /// <summary>
    /// 카트 도착하면 패널 열기
    /// 1. pos == 0이라면 (거꾸로 이동하는 돌리) 처음 패널 열기
    /// 1-2. 아니면 다음 패널 열기
    /// </summary>
    public void CartEnd()
    {
        UIManager.instance.CartEndOpenPanel();

        cam.gameObject.transform.SetParent(this.gameObject.transform);
        dollyCart.SetActive(false);
    }
}
