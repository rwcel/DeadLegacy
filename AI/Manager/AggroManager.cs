using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Networking;

[System.Serializable]
public struct PlayerAggroInfo
{
    //플레이어 정보
    public GameObject playerObject;         // 플레이어 오브젝트
    public short netCode;                   // 넷코드 (삭제시킬 때 이걸로 찾기)
    public int battleAINum;                 // 교전중인 적
}

public class AggroManager : MonoBehaviour
{
    public List<PlayerAggroInfo> playerAggroList;

    #region Singleton

    private static AggroManager _instance = null;

    public static AggroManager Instance
    {
        get
        {
            if (_instance == null)
            {
                _instance = FindObjectOfType(typeof(AggroManager)) as AggroManager;
                if (_instance == null)
                    Debug.LogError("No AggroManager object");
            }
            return _instance;
        }
    }

    #endregion


    /// <summary>
    /// 어그로 관리 한곳에서만 하게하기 (호스트에서만)
    /// </summary>
    private void Start()
    {
        if (RoomManager.Instance.m_MyInfo.inGameUserInfo.isGuest)
        {
            Destroy(this.gameObject);
        }
    }

    /// <summary>
    /// 어그로 추가하기 (처음, 되살아 났을때)
    /// 1. 중복체크
    /// 2. 데이터 넣고 리스트 추가
    /// </summary>
    /// <param name="player"></param>
    /// <param name="netCode"></param>
    public void AddPlayerInfo(GameObject player, short netCode)
    {
        // 1.
        for (int i = 0; i < playerAggroList.Count; i++)
        {
            if (playerAggroList[i].netCode == netCode)
            {
                return;
            }
        }

        // 2.
        PlayerAggroInfo _newPlayerInfo = new PlayerAggroInfo();
        _newPlayerInfo.playerObject = player;
        _newPlayerInfo.battleAINum = 0;
        _newPlayerInfo.netCode = netCode;

        playerAggroList.Add(_newPlayerInfo);

        // Debug.Log("접속인원 : " + playerAggroList.Count);
    }


    /// <summary>
    /// 플레이어 죽으면 어그로 없애기
    /// </summary>
    /// <param name="netCode"></param>
    public void PlayerDie(short netCode)
    {
        for (int i = 0; i < playerAggroList.Count; i++)
        {
            if (playerAggroList[i].netCode == netCode)
            {
                // Debug.Log(playerAggroInfo[i].netId);
                playerAggroList.Remove(playerAggroList[i]);
            }
        }
    }

    /// <summary>
    /// 게임오버 되면 해야할 행동
    /// </summary>
    public void GameOverCheck()
    {
        if (playerAggroList.Count == 0)
        {
            // Debug.Log("게임오버");
            InGameUIManager.Instance.EndGame(EndGameValue.GAME_OVER);
        }
    }
}