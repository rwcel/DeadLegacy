using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Cinemachine;
using UnityEngine.UI;

public enum MainMenu_Button
{
    SINGLE_PLAY, MULTI_PLAY, SETTING, CREDIT, BACK
}

public class MainMenuProcess : MonoBehaviour
{
    [SerializeField] GameObject mainmenuObj;
    public void OnEnable()
    {
        GameManager.instance.GameManagerState(GameState.MAIN_MENU);
        //mainmenuObj.SetActive(true);
    }
    public void OnDisable()
    {
        //mainmenuObj.SetActive(false);
    }

    public void Button_MainMenu(int content)
    {
        switch ((MainMenu_Button)content)
        {
            case MainMenu_Button.SINGLE_PLAY:
                // UIManager.instance.DollyCameraMove(UIManager.instance.singleTrack, UIManager.instance.mainmenuPanel, UIManager.instance.lobbyPanel, 1);
                break;
            case MainMenu_Button.MULTI_PLAY:
                LobbyManager.Instance.EnterLobby();
                break;
            case MainMenu_Button.SETTING:
                UIManager.instance.mainmenuPanel.SetActive(false);
                UIManager.instance.optionPanel.SetActive(true);
                break;
            case MainMenu_Button.CREDIT:
                DollyCartManager.instance.DollyCameraMove(DollyCartManager.instance.creditTrack, UIManager.instance.mainmenuPanel, UIManager.instance.creditPanel, 1);
                break;
            case MainMenu_Button.BACK:
                DollyCartManager.instance.DollyCameraMove(DollyCartManager.instance.loginTrack, UIManager.instance.mainmenuPanel, UIManager.instance.loginPanel, - 1);
                break;
        }

    }
}