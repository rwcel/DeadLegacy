using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using Cinemachine;
using Global;
public class UIManager : MonoBehaviour
{
    [Header("<메인 패널들>")]
    public static GameObject openedPanel;               // 현재 열려져 있는 패널(다른 씬에서 오류남)
    public GameObject OutGameCanvas;
    public GameObject loginPanel;
    public GameObject mainmenuPanel;
    public GameObject optionPanel;
    public GameObject creditPanel;
    public GameObject lobbyPanel;
    public GameObject roomPanel;

    [Header("<룸에 필요한 패널>")]
    [HideInInspector] public GameObject roomInfoPanel;
    [HideInInspector] public GameObject abilityPanel;
    [HideInInspector] public GameObject shopPanel;
    public string[] stageName;
    public string[] difficultyName;
    public Color[] difficultyColor;
    public Sprite[] stageImage;

    GameObject startPanel;                      // 시작 패널 (행동을 하면 돌리카트 이동)
    GameObject endPanel;                        // 끝 패널 (돌리카트 도착했을 떄 띄울)

    [Header("<인게임 정보>")]
    public Sprite[] playerImage;
    public Sprite[] playerPortrait;
    public Sprite[] skillIcon;
    public Sprite[] mainWeaponImage;
    public Sprite subWeaponImage;

    public GameObject inGameCanvas;

    // 싱글톤 선언
    public static UIManager instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            //DontDestroyOnLoad(OutGameCanvas);
        }
        else if (instance != this)
        {
            Destroy(gameObject);
            //Destroy(OutGameCanvas);
        }
    }

    public void CartStartClosePanel(GameObject _curPanel, GameObject _nextPanel)
    {
        startPanel = _curPanel;
        endPanel = _nextPanel;

        startPanel.SetActive(false);
    }

    public void CartEndOpenPanel()
    {
        endPanel.SetActive(true);
    }

    // 패널 열기
    public void OpenPanel(GameObject _panelObj)
    {
        if (openedPanel == null)
        {
            openedPanel = _panelObj;
            openedPanel.SetActive(true);
            // Debug.Log("열린패널 : "+openedPanel.name);
        }
    }

    // 열려진 패널 닫기
    public void ClosePanel()
    {
        if(openedPanel != null)
        {
            // Debug.Log("닫힌패널 : " + openedPanel.name);
            openedPanel.SetActive(false);
            openedPanel = null;
        }
    }

    /// <summary>
    /// 룸 -> 로비로 가면 FInd해서 재설정하게 하기
    /// </summary>
    public void Resetting()
    {
        OutGameCanvas = GameObject.Find("OutCanvas");
        loginPanel = OutGameCanvas.transform.GetChild(0).gameObject;
        mainmenuPanel = OutGameCanvas.transform.GetChild(1).gameObject;
        lobbyPanel = OutGameCanvas.transform.GetChild(4).gameObject;

        Debug.Log(OutGameCanvas.name + "," + lobbyPanel.name);
    }

    /// <summary>
    /// 상태에 따른 UI 출력 종류
    /// </summary>
    /// <param name="state"></param>
    public void UpdateSceneState(GameState state)
    {
        switch (state)
        {
            case GameState.LOGIN:
                break;
            case GameState.MAIN_MENU:
                break;
            case GameState.LOBBY:
                try
                {
                    DollyCartManager.instance.cam.transform.position = new Vector3(-4.8f, 11.6f, -35f);
                    DollyCartManager.instance.cam.transform.eulerAngles = new Vector3(-6.1f, 180f, 0f);

                    loginPanel.SetActive(false);
                    lobbyPanel.SetActive(true);
                }
                catch { }
                break;
            case GameState.ROOM:
                try
                {
                    roomPanel = GameObject.Find("RoomCanvas");
                    roomPanel.SetActive(true);
                }
                catch { }
                break;
            case GameState.GAME_START:
                break;
        }
    }

}
