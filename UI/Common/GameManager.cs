using System.Collections;
using System.Collections.Generic;
using Lovatto.SceneLoader;
using UnityEngine;
using UnityEngine.UI;
using Global;
using UnityEngine.SceneManagement;

public enum GameState
{
    LOGIN, MAIN_MENU, LOBBY, ROOM, GAME_START
}

// 플레이어 정보 구조체
public struct PlayerInfo
{
    public string id;                   // 아이디
    public string pw;                   // 비밀번호
    public string name;                 // 닉네임
    public byte roomEnterNum;           // 방에 입장한 번호

    public short netCode;               // 넷코드
    public CharacterCode charCode;      // 캐릭터 코드
    public GameObject playerObj;        // 플레이어 게임 오브젝트 : 이를 통해 플레이어 스크립트 접근 가능
    // public WeaponSystem weaponSystem;   // 무기 정보 - 참조용

    public Slider hpSlider;             // 체력 ui
}

// 방 구조체
public struct RoomInfo
{
    // 만들 때 입력하는 값
    public byte roomNum;              // 방 번호
    public string roomName;             // 방 이름
    public int stageValue;              // 스테이지 이름 - DropDown_Value
    public int difficultyValue;         // 난이도 - DropDown_Value

    // 스크립트로 설정해주는 값
    public byte userCount;              // 방 안에 있는 유저 수 : 1~4  **0이면 방폭파
    public int ping;                    // 핑 차이 : 호스트-게스트
    public List<PlayerInfo> playerInfo; // 플레이어 정보
    // public PlayerInfo[] playerInfo;     // 

    public bool isGameStart;            // 게임 시작했는지  *미사용
    public int life;                    // 게임 내 부활 수  *미사용
}

public class GameManager : MonoBehaviour
{
    public GameState gameState;

    public List<PlayerInfo> otherPlayerList;        // 다른 플레이어 리스트
    public List<RoomInfo> roomList;                 // 방 리스트

    PlayerInfo currentPlayer;
    RoomInfo currentRoom;
    [HideInInspector] public byte roomCount = 0;              // 방 개수

    AsyncOperation asyncOper;               // 실행중인 asyncOperation
    protected bool isLoad;                  // 로딩여부(두번 연속 처리 못하게)
    public bool isLoading
    {
        get { return isLoad; }
        set { isLoad = value; }
    }

    public string[] stageSceneName;


    // 싱글톤 선언
    public static GameManager instance = null;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
            otherPlayerList = new List<PlayerInfo>();
            roomList = new List<RoomInfo>();
            gameState = GameState.LOGIN;
            UIManager.instance.optionPanel.GetComponent<OptionProcess>().StartSetting();
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    // 씬 로딩
    public IEnumerator AsyncLoadScene(string sceneName)
    {
        isLoad = true;

        asyncOper = SceneManager.LoadSceneAsync(sceneName);
        while (!asyncOper.isDone)
        {
            yield return null;
            // Debug.Log(asyncOper.progress);
        }

        isLoad = false;
    }

    // 자기 캐릭터 확인
    public void setCurrentPlayer(PlayerInfo _player)
    {
        currentPlayer = _player;
    }
    public PlayerInfo getCurrentPlayer()
    {
        return currentPlayer;
    }

    // 자기 방 확인
    public void setCurrentRoom(RoomInfo _room)
    {
        currentRoom = _room;
    }
    public RoomInfo getCurrentRoom()
    {
        return currentRoom;
    }

    // 게임 시작할 때 참조내용들 넣어주기
    public void PlayerGameSetup(PlayerInfo _playerInfo)
    {
        currentPlayer = _playerInfo;
        setCurrentPlayer(currentPlayer);
    }


    /// <summary>
    /// 상태에 따른 출력종류
    /// </summary>
    /// <param name="state"></param>
    public void GameManagerState(GameState state)
    {
        switch (state)
        {
            case GameState.LOGIN:
                SoundManager.instance.PlayBGMSound(BGMSoundID.OutGame);
                break;
            case GameState.MAIN_MENU:
                // SoundManager.instance.PlayBGMSound(1);
                break;
            case GameState.LOBBY:
                // SoundManager.instance.PlayBGMSound(2);
                break;
            case GameState.ROOM:
                // SoundManager.instance.PlayBGMSound(3);
                break;
            case GameState.GAME_START:
                SoundManager.instance.PlayBGMSound(BGMSoundID.InGame);
                break;
        }

        gameState = state;
        Debug.Log("게임 상태 바뀜 :" + gameState);
        UIManager.instance.UpdateSceneState(state);
    }
}
