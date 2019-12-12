using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public enum EndGameValue
{
    GAME_OVER, GAME_CLEAR
}

public class EndGameUI : MonoBehaviour
{
    public EndGameValue m_EndGameValue;

    public Text[] nameText;

    public Text m_StageText;
    public Text m_ScoreText;
    public Text m_TimeText;

    /// <summary>
    /// 값 넣어주기
    /// </summary>
    public void UISetting(EndGameValue p_EndGameValue)
    {
        m_EndGameValue = p_EndGameValue;

        //nameText[0].text = InGameUIManager.Instance.playerUI.nickName.text;
        //for (int i = 0; i < InGameUIManager.Instance.remotePlayer.Count; i++)
        //{
        //    nameText[i + 1].transform.parent.gameObject.SetActive(true);
        //    nameText[i + 1].text = InGameUIManager.Instance.otherPlayerUI[i].nickName.text;
        //}

        //m_StageText.text = UIManager.instance.stageName[(int)RoomManager.Instance.m_RoomInfo.stage];

        //m_ScoreText = ;
        //m_TimeText = ;
    }

    public void Button_CloseGameEnd()
    {
        UIManager.instance.ClosePanel();

        switch (m_EndGameValue)
        {
            case EndGameValue.GAME_OVER:
                GameManager.instance.gameState = GameState.LOBBY;
                GameManager.instance.StartCoroutine(GameManager.instance.AsyncLoadScene("robby_Real 2"));
                break;
            case EndGameValue.GAME_CLEAR:
                GameManager.instance.StartCoroutine(GameManager.instance.AsyncLoadScene("RoomScene"));
                break;
        }
    }
}