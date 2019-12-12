using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.SceneManagement;
using Global;

public enum RoomContent
{
    ROOM_SET, ABILITY, SHOP, READY, BACK, 
}

/// <summary>
/// 방 패널 컨텐츠
/// </summary>
[System.Serializable]
public struct RoomPlayerContent
{
    public GameObject characterParent;                      // 캐릭터 패널
    public Text nameText;                                   // 닉네임 텍스트
    public GameObject[] charObj;                          // 캐릭터 오브젝트(중에 선택)
    public GameObject readyObj;                             // 준비 체크 오브젝트
    public int selectNum;                                   // 선택한 캐릭터 번호
    [HideInInspector] public short netCode;                 // 넷코드
}

public class RoomProcess : MonoBehaviour
{
    [SerializeField] GameObject roomSettingPanel;           // 방 설정 패널
    [SerializeField] Text roomNameText;                     // RoomList대신 설정할 방 

    [SerializeField] InputField roomName;                   // 방 이름
    [SerializeField] Dropdown stageDropDown;                // 스테이지 이름
    [SerializeField] Dropdown difficultyDropDown;           // 난이도
    [SerializeField] Image stageImage;                      // 스테이지 이미지
    [SerializeField] Text gameReadyText;                    // 게임 준비 텍스트 (호스트:스타트 / 게스트:레디)

    [SerializeField] GameObject[] characterObj;             // 캐릭터 오브젝트
    [SerializeField] RoomPlayerContent[] roomPlayerContent; // 방에 들어올 수 있는 플레이어(4명)

    Vector3[] userRotation;                                 // 방 위치에 따른 캐릭터 오브젝트 돌리는 값

    [SerializeField] bl_SceneLoader sceneLoader;

    // 현재 접속중인 방, 유저
    Global.RoomInfo currentRoom;
    PlayerInfo currentPlayer;
    public List<PlayerInfo> roomPlayerList;                 // 방에있는 플레이어 리스트

    byte charNum;                                           // 캐릭터선택 번호
    bool isReady;                                           // 레디중인지
    bool isHost;                                            // 방장인지

    int gameSceneNum;                                       // 들어갈 게임씬 번호(스테이지에 따른 변화)

    public static RoomProcess instance;

    /// <summary>
    /// 게임시작 후 다시 돌아와도 해야하는 것들
    /// 1. 씬 매니저 넣기(룸씬에 들어오면 다른 오브젝트 끄기)
    /// 2. 버튼, 텍스트 넣어주기
    /// 3. 플레이어 수만큼 캐릭터 오브젝트 넣어주기
    /// </summary>
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

        // 1.
        SceneManager.sceneLoaded += OnSceneLoaded;

        // RoomManager.Instance.UpdateRoomUI();

        // 2.
        UserQuaternionSetting();

        // roomPlayer = new RoomPlayer[characterObj.Length];
        for (int i = 0; i < roomPlayerContent.Length; i++)
        {
            roomPlayerContent[i].charObj = new GameObject[characterObj.Length];
        }

        // 3.
        for (int i = 0; i < characterObj.Length; i++)
        {
            for (int j = 0; j < roomPlayerContent.Length; j++)
            {
                GameObject _obj = Instantiate(characterObj[i]);

                _obj.transform.SetParent(roomPlayerContent[j].characterParent.transform);
                _obj.transform.localPosition = Vector3.zero;
                _obj.SetActive(false);

                roomPlayerContent[j].charObj[i] = _obj;
            }
        }
    }

    // 유저 위치에 따른 쿼터니언 값
    void UserQuaternionSetting()
    {
        userRotation = new Vector3[characterObj.Length];
        userRotation[0] = new Vector3(0, 130f, 0);
        userRotation[1] = new Vector3(0, 170f, 0);
        userRotation[2] = new Vector3(0, 190f, 0);
        userRotation[3] = new Vector3(0, 210f, 0);
    }

    /// <summary>
    /// 씬 로딩됐을 때 저장해야할 것들 : 방 정보, 닉네임 등
    /// </summary>
    /// <param name="scnee"></param>
    /// <param name="mode"></param>
    void OnSceneLoaded(Scene scnee, LoadSceneMode mode)
    {
        // Debug.Log("로드 타이밍");
        GameManager.instance.GameManagerState(GameState.ROOM);
    }

    /// <summary>
    /// 1. 현재 방, 유저 넣기
    /// 2. 기본 플레이어 제공(처음 들어온 경우)
    /// 3. 게임 끝나고 나온경우라면 원래 캐릭터 보여주기
    /// </summary>
    private void Start()
    {
        // 1.
        currentRoom = RoomManager.Instance.m_RoomInfo;
        currentPlayer = GameManager.instance.getCurrentPlayer();

        isHost = RoomManager.Instance.m_MyInfo.inGameUserInfo.isHost;
        if (isHost)
            gameReadyText.text = "START";

        // Debug.Log(currentPlayer.name);
        Debug.Log(currentRoom.roomNumber + "," + currentRoom.roomName + "," + currentRoom.userCount);
        roomNameText.text = currentRoom.roomName;

        // 2.
        InputMakeRoomData();

        currentPlayer.roomEnterNum = (byte)(currentRoom.userCount - 1);
        Debug.Log("입장 : " + currentPlayer.roomEnterNum + "," + currentPlayer.name);

        // 3.
        EnterPlayer();
    }

    /// <summary>
    /// 방 정보 관련 변수들 적용하기
    /// </summary>
    void InputMakeRoomData()
    {
        for (int i = 0; i < stageDropDown.options.Count; i++)
        {
            stageDropDown.options[i].text = UIManager.instance.stageName[i];
        }
        for (int i = 0; i < difficultyDropDown.options.Count; i++)
        {
            difficultyDropDown.options[i].text = UIManager.instance.difficultyName[i];
        }

        roomName.text = currentRoom.roomName;
        stageDropDown.value = (int)currentRoom.stage;
        difficultyDropDown.value = (int)currentRoom.level;
        stageImage.sprite = UIManager.instance.stageImage[stageDropDown.value];
    }

    /// <summary>
    /// 서버에서 유저 들어오면 실시간 처리
    /// 1. 현재 접속 인원만큼 처리하기(이름, 캐릭터, 준비 버튼)  
    /// ** currentRoom.playerInfo[i] - 방에 접속한 플레이어들
    /// </summary>
    public void EnterPlayer()
    {
        // Debug.Log(currentRoom.userCount);

        int _num = 0;
        foreach (KeyValuePair<short, UserInfo> _user in RoomManager.Instance.m_UserDictionary)
        {
            Debug.Log("들어온 횟수");

            roomPlayerContent[_num].netCode = _user.Key;

            roomPlayerContent[_num].nameText.text = _user.Value.nickname;
            roomPlayerContent[_num].nameText.gameObject.SetActive(true);

            int _charNum = _user.Value.inGameUserInfo.characterCode - (int)CharacterCode.Richard;
            roomPlayerContent[_num].charObj[_charNum].transform.eulerAngles = userRotation[_num];
            roomPlayerContent[_num].charObj[_charNum].SetActive(true);

            if (_user.Value.inGameUserInfo.isReady)
                roomPlayerContent[_num].readyObj.SetActive(true);
            // ChangeCharacter(_num, (byte)0);

            _num++;
        }

        charNum = 0;
    }

    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.LeftArrow))
        {
            if (isReady)
                return;
            ChangeCharacter(currentPlayer.roomEnterNum, --charNum);
        }
        else if (Input.GetKeyDown(KeyCode.RightArrow))
        {
            if (isReady)
                return;
            ChangeCharacter(currentPlayer.roomEnterNum, ++charNum);
        }
    }

    /// <summary>
    /// 캐릭터 변경 및 코드 적용
    /// * 초기에 보이게 하는 용도로도 사용
    /// </summary>
    /// <param name="_playerNum">플레이어 룸 입장 번호</param>
    /// <param name="_charNum">캐릭터 번호</param>
    void ChangeCharacter(int _playerNum, byte _charNum)
    {
        charNum = (byte)Mathf.Clamp(_charNum, 0, 3);

        currentPlayer.charCode = charNum + CharacterCode.Richard;

        // **효찬코드
        RoomManager.Instance.ChangeUserInfo((byte)ChangeCategory.CharacterCode, (byte)currentPlayer.charCode);
    }

    /// <summary>
    /// 모든 클라 캐릭터 변경시키기
    /// </summary>
    /// <param name="_netCode">넷코드</param>
    /// <param name="_charNum">캐릭터 번호(201-204)</param>
    public void ChangeCharacterToServer(short _netCode, byte _charNum)
    {
        _charNum -= (byte)CharacterCode.Richard;
        charNum = (byte)Mathf.Clamp(_charNum, 0, 3);

        int _num = 0;
        foreach (KeyValuePair<short, UserInfo> _user in RoomManager.Instance.m_UserDictionary)
        {
            if (_user.Key == _netCode)
            {
                Debug.Log("바뀌는 캐릭터 : " + _netCode + ", " + charNum);
                for (int i = 0; i < roomPlayerContent.Length; i++)
                {
                    roomPlayerContent[_num].charObj[i].SetActive(false);
                }

                roomPlayerContent[_num].charObj[charNum].transform.eulerAngles = userRotation[_num];
                roomPlayerContent[_num].charObj[charNum].SetActive(true);
                break;
            }
            _num++;
        }

    }

    public void Button_Room(int content)
    {
        switch ((RoomContent)content)
        {
            case RoomContent.ROOM_SET:
                UIManager.instance.OpenPanel(roomSettingPanel);
                break;
            case RoomContent.ABILITY:
            case RoomContent.SHOP:
                break;
            // 준비 버튼 -> 방장인 경우와 아닌경우로 나누기
            case RoomContent.READY:
                if (isHost)
                {
                    Debug.Log("게임시작");
                    RoomManager.Instance.GameStart();
                    // StartGameLoading();
                }
                else
                {
                    RoomManager.Instance.GameReady();
                }

                UpdateData();

                break;
            case RoomContent.BACK:
                RoomManager.Instance.LeaveRoom();
                HostLeaveRoom();
                break;
        }
    }

    /// <summary>
    /// UI 갱신하기
    /// 다른 플레이어 들어오면 전체 플레이어 수, roomPlayerContent
    /// </summary>
    public void UpdateRoomUI()
    {
        int _num = 0;
        foreach (KeyValuePair<short, UserInfo> _user in RoomManager.Instance.m_UserDictionary)
        {
            // Debug.Log(i+"의 캐릭터 변경 : "+ _user.Value.inGameUserInfo.characterCode);
            roomPlayerContent[_num].nameText.text = _user.Value.nickname;
            roomPlayerContent[_num].nameText.gameObject.SetActive(true);
            ChangeCharacterToServer(_user.Key, (byte)(_user.Value.inGameUserInfo.characterCode - CharacterCode.Richard));

            _num++;
        }
    }

    /// <summary>
    /// 캐릭터 선택시 나머지 같은 캐릭터들 회색으로 변해야함
    /// **Renderer[] 저장해야할지
    /// </summary>
    public void CharacterReady(short p_NetCode)
    {
        int _num = 0;
        foreach (KeyValuePair<short, UserInfo> _user in RoomManager.Instance.m_UserDictionary)
        {
            if (_user.Key == p_NetCode)
            {
                roomPlayerContent[_num].readyObj.SetActive(true);

                if (currentPlayer.netCode == p_NetCode)
                    isReady = true;
            }
            //else
            //{
            //    Renderer[] _charRenderer = roomPlayerContent[_num].charObj[_user.Value.inGameUserInfo.characterCode-(byte)CharacterCode.Richard].GetComponentsInChildren<Renderer>();
            //    for (int j = 0; j < _charRenderer.Length; j++)
            //    {
            //        _charRenderer[_num].material.color = Color.gray;
            //    }
            //}
            _num++;
        }
    }

    /// <summary>
    ///  원래 캐릭터 색깔 돌아와야함(흰색)
    /// </summary>
    /// <param name="p_NetCode"></param>
    public void CharacterUnReady(int p_NetCode)
    {
        int _num = 0;
        foreach (KeyValuePair<short, UserInfo> _user in RoomManager.Instance.m_UserDictionary)
        {
            if (_user.Key == p_NetCode)
            {
                roomPlayerContent[_num].readyObj.SetActive(false);

                if (currentPlayer.netCode == p_NetCode)
                    isReady = false;
            }

            //Renderer[] _charRenderer = roomPlayerContent[_num].charObj[_user.Value.inGameUserInfo.characterCode - (byte)CharacterCode.Richard].GetComponentsInChildren<Renderer>();
            //for (int j = 0; j < _charRenderer.Length; j++)
            //{
            //    _charRenderer[_num].material.color = Color.white;
            //}
            _num++;
        }
    }

    /// <summary>
    /// 호스트가 방나가면 : 로비로 다 되돌리기
    /// </summary>
    public void HostLeaveRoom()
    {
        Debug.Log("호스트가 방을 나갔습니다");

        OutRoomScene("robby_Real 2");
        GameManager.instance.gameState = GameState.LOBBY;
    }

    /// <summary>
    /// 게스트 중 한명이 방 나갔을 때
    /// 오브젝트 지우기
    /// </summary>
    public void GuestLeaveRoom(short p_NetCode)
    {
        int _num = 0;
        foreach (KeyValuePair<short, UserInfo> _user in RoomManager.Instance.m_UserDictionary)
        {
            if (p_NetCode == _user.Key)
            {
                Debug.Log("나갑니다:" + p_NetCode);

                RoomPlayerContent _removeContent = roomPlayerContent[_num];

                int _charNum = _user.Value.inGameUserInfo.characterCode - (int)CharacterCode.Richard;

                _removeContent.charObj[_charNum].SetActive(false);
                _removeContent.nameText.gameObject.SetActive(false);

                if (_user.Value.inGameUserInfo.isReady)
                    _removeContent.readyObj.SetActive(false);

                break;
            }
            _num++;
        }
    }

    /// <summary>
    /// 플레이어가 4명이고 && 플레이어가 모두 준비했는지 -> 서버에서 판단
    /// 방 갱신하고() 작성한 목록들 저장
    /// 레벨 로드하기
    ///
    /// </summary>
    public void StartGameLoading()
    {
        UpdateData();

        if (currentPlayer.roomEnterNum == 0)
        {
            Debug.Log("방장");
            sceneLoader.FakeLoadLevel(GameManager.instance.stageSceneName[(int)currentRoom.stage], 5f);
        }
        else
        {
            Debug.Log("나머지");
            sceneLoader.FakeLoadLevel(GameManager.instance.stageSceneName[(int)currentRoom.stage], 3f);
        }
        // sceneLoader.LoadLevel(GameManager.instance.stageSceneName[(int)currentRoom.level]);
    }

    /// <summary>
    /// 씬 바뀌기 전에 데이터 저장하는 장소
    /// 1. 방 설정 변경확인
    /// 2. 플레이어 설정
    /// 3. 방에 해당하는 플레이어 설정
    /// 4. 방 설정
    /// </summary>
    public void UpdateData()
    {
        GameManager.instance.PlayerGameSetup(currentPlayer);
        // Debug.Log(currentPlayer.roomEnterNum);
    }

    public void OutRoomScene(string sceneName)
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
        UpdateData();

        GameManager.instance.StartCoroutine(GameManager.instance.AsyncLoadScene(sceneName));
    }
}
