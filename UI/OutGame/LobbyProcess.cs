using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;

public enum Lobby_Button
{
    //  방만들기    방접속         빠른참가       새로고침   특성    상점  뒤로가기
    MAKE_ROOM, ENTER_ROOM, FAST_ENTER_ROOM, REFRESH, ABILITY, SHOP, BACK
}

public enum LobbyContent
{
    //  방번호 이름  스테이지    레벨  플레이어 수  핑
    NUM = 0, NAME, GAMESTAGE, LEVEL, PLAYERS, PING
}

public struct CreateRoomContent
{
    //ROOMNAME = 0, STAGE, LEVEL,
    public InputField roomName;     // 방 이름
    public Dropdown stage;          // 스테이지
    public Dropdown difficulty;     // 난이도
}

public class LobbyProcess : MonoBehaviour
{
    public GameObject roomDataParent;             // 방 정보 항목을 넣어줄 장소(Viewport-Content하위)
    public GameObject roomDataContent;            // 방 정보 항목 객체
    [SerializeField] GameObject createRoomContentParent;    // 방 만드는 항목 부모
    CreateRoomContent createRoomContent;                    // 방만드는 정보 항목들

    [SerializeField] string[] exampleRoomName;              // 랜덤 방이름

    PlayerInfo currentPlayer;                               // 현재 플레이어
    public static RoomInfo selectRoom;               // 현재 선택한 방  **static으로 해야만 데이터가 저장이 됨

    [SerializeField] GameObject makeRoomPanel;              // 방만들기 패널
    [SerializeField] Dropdown stageDropDown;
    [SerializeField] Dropdown difficultyDropDown;
    [SerializeField] Image stageImage;                      // 스테이지 이미지

    // 특성, 상점은 DontDestroy가 되어야함
    [SerializeField] GameObject abilityPanel;               // 특성 패널
    [SerializeField] GameObject shopPanel;                  // 상점 패널

    [SerializeField] GameObject playerFullPanel;            // 방 안에 유저가 가득 찼을 때 보여줄 패널

    public static LobbyProcess instance;

    private void Awake()
    {
        if (instance == null)
        {
            instance = this;
        }
        else if (instance != this)
        {
            Destroy(gameObject);
        }
    }
    private void OnEnable()
    {
        GameManager.instance.GameManagerState(GameState.LOBBY);
    }

    /// <summary>
    /// 1. 방 설정 바꿈
    /// 2. 
    /// </summary>
    private void Start()
    {
        DropDownOptionsChange();

        currentPlayer = GameManager.instance.getCurrentPlayer();

        // TmpAddRoomList();

        ShowRoomList();

        SetOutGameUIPanel();

        SetCreateRoomContent();
    }

    private void Update()
    {
        float _refreshTime = 5f;
        float _currentTime = 0f;

        if (_currentTime >= _refreshTime)
        {
            _currentTime = 0f;
            ShowRoomList();
        }
    }

    /// <summary>
    /// 드랍다운 옵션 바꾸기
    /// </summary>
    void DropDownOptionsChange()
    {
        for (int i = 0; i < stageDropDown.options.Count; i++)
        {
            stageDropDown.options[i].text = UIManager.instance.stageName[i];
        }
        for (int i = 0; i < difficultyDropDown.options.Count; i++)
        {
            difficultyDropDown.options[i].text = UIManager.instance.difficultyName[i];
        }
    }

    // 임시로 3개 방만들기
    void TmpAddRoomList()
    {
        // 임시방 1번 : 비밀번호 걸린 방
        RoomInfo _room = new RoomInfo();
        _room.roomName = "임시1번방";
        _room.stageValue = 1;
        _room.difficultyValue = 0;

        _room.userCount = 1;
        _room.playerInfo = new List<PlayerInfo>();

        _room.roomNum = (++GameManager.instance.roomCount);

        GameManager.instance.roomList.Add(_room);

        //GameObject _newRoomContent = Instantiate(roomDataContent);

        //_newRoomContent.transform.GetChild(0).GetComponent<Text>().text = _room.roomNum;
        //_newRoomContent.transform.GetChild(1).GetComponent<Text>().text = _room.roomName;
        //_newRoomContent.transform.GetChild(2).GetComponent<Text>().text = UIManager.instance.stageName[_room.stageValue];
        //_newRoomContent.transform.GetChild(3).GetComponent<Text>().text = UIManager.instance.difficultyName[_room.difficultyValue];
        //_newRoomContent.transform.GetChild(3).GetComponent<Text>().color = UIManager.instance.difficultyColor[_room.difficultyValue];
        //_newRoomContent.transform.GetChild(4).GetComponent<Text>().text = _room.roomPlayer.ToString() + "/4";
        //_newRoomContent.transform.GetChild(5).GetComponent<Text>().text = "30";
        //_newRoomContent.transform.SetParent(roomDataParent.transform);

        //_newRoomContent.transform.localPosition = Vector3.zero;
        //_newRoomContent.transform.localScale = Vector3.one;


        // 임시방 2번 : 풀방
        _room.roomName = "임시2번방";
        _room.stageValue = 2;
        _room.difficultyValue = 2;

        _room.userCount = 4;
        _room.playerInfo = new List<PlayerInfo>();
        // _room.playerInfo = new PlayerInfo[4];

        _room.roomNum = (++GameManager.instance.roomCount);

        GameManager.instance.roomList.Add(_room);

        // 임시방 3번 : 일반 방
        _room.roomName = "임시3번방";
        _room.stageValue = 0;
        _room.difficultyValue = 0;

        _room.userCount = 2;
        _room.playerInfo = new List<PlayerInfo>();

        _room.roomNum = (++GameManager.instance.roomCount);

        GameManager.instance.roomList.Add(_room);
    }

    /// <summary>
    /// 새로고침위해서 방 리스트 다 지우기
    /// </summary>
    public void RoomListClear()
    {
        for (int i = 0; i < GameManager.instance.roomList.Count; i++)
        {
            try
            {
                Destroy(roomDataParent.transform.GetChild(0).gameObject);
            }
            catch { break; };
        }

        GameManager.instance.roomList.Clear();
    }

    public void RoomListAdd(Global.RoomInfo p_Roominfo)
    {
        Debug.Log("방생성");
        RoomInfo _room = new RoomInfo();
        _room.roomNum = p_Roominfo.roomNumber;
        _room.roomName = p_Roominfo.roomName;
        _room.stageValue = (int)p_Roominfo.stage;
        _room.difficultyValue = (int)p_Roominfo.level;

        _room.userCount = p_Roominfo.userCount;
        _room.playerInfo = new List<PlayerInfo>();

        GameManager.instance.roomList.Add(_room);
    }

    public void ShowRoomList()
    {
        for (int i = 0; i < GameManager.instance.roomList.Count; i++)
        {
            RoomInfo _roomInfo = GameManager.instance.roomList[i];

            GameObject _newRoomContent = Instantiate(LobbyProcess.instance.roomDataContent);
            _newRoomContent.transform.GetChild(0).GetComponent<Text>().text = _roomInfo.roomNum.ToString();
            _newRoomContent.transform.GetChild(1).GetComponent<Text>().text = _roomInfo.roomName;
            _newRoomContent.transform.GetChild(2).GetComponent<Text>().text = UIManager.instance.stageName[_roomInfo.stageValue];
            _newRoomContent.transform.GetChild(3).GetComponent<Text>().text = UIManager.instance.difficultyName[_roomInfo.difficultyValue];
            _newRoomContent.transform.GetChild(3).GetComponent<Text>().color = UIManager.instance.difficultyColor[_roomInfo.difficultyValue];
            _newRoomContent.transform.GetChild(4).GetComponent<Text>().text = _roomInfo.userCount.ToString() + "/4";
            _newRoomContent.transform.GetChild(5).GetComponent<Text>().text = "30";
            _newRoomContent.transform.SetParent(LobbyProcess.instance.roomDataParent.transform);

            _newRoomContent.transform.localPosition = Vector3.zero;
            _newRoomContent.transform.localScale = Vector3.one;
        }
    }

    // OutGameUIManager의 패널 값 세팅하기
    void SetOutGameUIPanel()
    {
        UIManager.instance.roomInfoPanel = makeRoomPanel;
        UIManager.instance.abilityPanel = abilityPanel;
        UIManager.instance.shopPanel = shopPanel;
    }

    // 방만들기 항목들 넣어주기
    void SetCreateRoomContent()
    {
        createRoomContent.roomName = createRoomContentParent.transform.GetChild(0).gameObject.GetComponentInChildren<InputField>();
        createRoomContent.stage = createRoomContentParent.transform.GetChild(1).gameObject.GetComponentInChildren<Dropdown>();
        createRoomContent.difficulty = createRoomContentParent.transform.GetChild(2).gameObject.GetComponentInChildren<Dropdown>();
    }

    public void Button_Lobby(int content)
    {
        switch ((Lobby_Button)content)
        {
            case Lobby_Button.MAKE_ROOM:
                int _randomRoomName = Random.Range(0, exampleRoomName.Length);
                createRoomContent.roomName.text = exampleRoomName[_randomRoomName];

                UIManager.instance.OpenPanel(makeRoomPanel);
                break;
            case Lobby_Button.ENTER_ROOM:
                EnterRoom();
                break;
            case Lobby_Button.FAST_ENTER_ROOM:
                foreach (var _roomInfo in GameManager.instance.roomList)
                {
                    //if (_roomInfo.roomPlayer < 4)
                    //    EnterRoomScene(_roomInfo);
                    RoomManager.Instance.EnterRoom(_roomInfo.roomNum);
                }
                break;
            case Lobby_Button.REFRESH:
                // **내 정보로 새로고침을 해야함
                RoomManager.Instance.RoomList();
                break;
            case Lobby_Button.ABILITY:
                UIManager.instance.OpenPanel(abilityPanel);
                break;
            case Lobby_Button.SHOP:
                UIManager.instance.OpenPanel(shopPanel);
                break;
            // 메인메뉴로
            case Lobby_Button.BACK:
                DollyCartManager.instance.DollyCameraMove(DollyCartManager.instance.multiTrack, UIManager.instance.lobbyPanel, UIManager.instance.mainmenuPanel, -1);
                break;
        }
    }

    /// <summary>
    /// 드롭다운으로 정한 스테이지의 이미지 바꾸기
    /// </summary>
    /// <param name="value"></param>
    public void ChangeStageImage()
    {
        stageImage.sprite = UIManager.instance.stageImage[createRoomContent.stage.value];
    }

    #region 방만들기
    // 버튼클릭으로 방만들기확정(정보 다 입력한 상태) - 씬 넘어감
    public void Button_MakeRoom()
    {
        if (!GameManager.instance.isLoading && createRoomContent.roomName.text != "")
        {
            CreateRoom();
        }
    }

    /// <summary>
    /// 방만들기
    /// 1. 방 정보 임시 값에 넣기
    /// 2. 호스트 정보 넣기
    /// 3. BeforeGameScene의 방 리스트에 추가시키기
    /// 4. 접속가능하게 Panel 만들기
    /// 5. 방만든 패널 닫기
    /// 6. 
    /// 7. 접속
    /// **방정보 초기화 안함(RoomScene에서도 써야함)
    /// </summary>
    void CreateRoom()
    {
        RoomInfo _room = new RoomInfo();
        // 1.
        _room.roomName = createRoomContent.roomName.text;
        _room.stageValue = createRoomContent.stage.value;
        _room.difficultyValue = createRoomContent.difficulty.value;

        RoomManager.Instance.CreateRoom(_room.roomName, (Global.STAGE)_room.stageValue, (Global.LEVEL)_room.difficultyValue);

        // 5.
        UIManager.instance.ClosePanel();
    }

    /// <summary>
    /// 방 설정하기
    /// 1. 입력한 텍스트 설정
    /// 2. transform값 변경  **UI Instantiate시, 위치가 잘못되어서 나오기때문
    /// </summary>
    /// <param name="_room">방 정보 데이터</param>
    void MakeRoomData(RoomInfo _room)
    {
        GameObject _newRoomContent = Instantiate(roomDataContent);
        // 1.
        _newRoomContent.transform.GetChild(0).GetComponent<Text>().text = _room.roomNum.ToString();
        _newRoomContent.transform.GetChild(1).GetComponent<Text>().text = _room.roomName;
        _newRoomContent.transform.GetChild(2).GetComponent<Text>().text = UIManager.instance.stageName[_room.stageValue];
        _newRoomContent.transform.GetChild(3).GetComponent<Text>().text = UIManager.instance.difficultyName[_room.difficultyValue];
        _newRoomContent.transform.GetChild(3).GetComponent<Text>().color = UIManager.instance.difficultyColor[_room.difficultyValue];
        _newRoomContent.transform.GetChild(4).GetComponent<Text>().text = _room.userCount.ToString() + "/4";
        // **핑 조절하기
        _newRoomContent.transform.GetChild(5).GetComponent<Text>().text = "30";
        _newRoomContent.transform.SetParent(roomDataParent.transform);

        // 2.
        _newRoomContent.transform.localPosition = Vector3.zero;
        _newRoomContent.transform.localScale = Vector3.one;
    }

    #endregion

    #region 방접속
    /// <summary>
    /// 선택한 방 데이터 저장하기
    /// 한번 더 선택했을 때는 그 방 접속하게 하기
    /// </summary>
    /// <param name="roomNumText"></param>
    public void SelectRoomInfo(Text roomNumText)
    {
        Debug.Log(roomNumText);

        if (selectRoom.roomName != null && selectRoom.roomNum.ToString() == roomNumText.text)
        {
            Debug.Log("더블클릭으로 접속");
            EnterRoom();
            // Debug.Log(selectRoom.roomName);
        }
        else
        {
            Debug.Log(GameManager.instance.roomList.Count);
            byte _roomNum = byte.Parse(roomNumText.text);
            // selectRoom = GameManager.instance.roomList[_roomNum - 1];
            for (int i = 0; i < GameManager.instance.roomList.Count; i++)
            {
                if (_roomNum == GameManager.instance.roomList[i].roomNum)
                {
                    selectRoom = GameManager.instance.roomList[i];
                    Debug.Log(selectRoom.roomName);
                }
            }
        }
    }

    /// <summary>
    /// 방 접속
    /// 유저수가 꽉찬경우에는 못들여보내게 하기
    /// </summary>
    public void EnterRoom()
    {
        if (selectRoom.roomName == "")
        {
            Debug.Log("방이름 없음");
            PlayerFull();
            return;
        }
        if (selectRoom.userCount >= 4)
        {
            Debug.Log("사람 꽉참:" + selectRoom.userCount);
            UIManager.instance.OpenPanel(playerFullPanel);
            return;
        }

        Debug.Log(selectRoom.roomNum);
        RoomManager.Instance.EnterRoom(selectRoom.roomNum);
    }

    public void PlayerFull()
    {
        UIManager.instance.OpenPanel(playerFullPanel);
    }
    #endregion

    /// <summary>
    /// 1. _room에 현재 플레이어 추가하기
    /// 2. _room이름을 가진 방 입장하기
    /// </summary>
    /// <param name="_room">방 이름</param>
    void EnterRoomScene(Global.RoomInfo _room)
    {
        // 1.
        //currentPlayer.roomEnterNum = _room.roomPlayer;
        //_room.playerInfo.Add(currentPlayer);
        GameManager.instance.setCurrentPlayer(currentPlayer);

        // 2.
        RoomInfo _enterRoomInfo = new RoomInfo();
        _enterRoomInfo.roomNum = _room.roomNumber;
        _enterRoomInfo.roomName = _room.roomName;
        _enterRoomInfo.userCount = _room.userCount;

        GameManager.instance.setCurrentRoom(_enterRoomInfo);

        // GameManager.instance.StartCoroutine(GameManager.instance.AsyncLoadScene("RoomScene"));
    }
}