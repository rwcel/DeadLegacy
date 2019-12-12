using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

[System.Serializable]
public struct UserLoginData
{
    public string id;
    public string pw;
    public string nickname;
}

public class LoginProcess : MonoBehaviour
{
    [SerializeField] InputField inputField_ID;          // 아이디 입력
    [SerializeField] InputField inputField_PW;          // 비밀번호 입력

    [SerializeField] InputField input_nickname;    // NickName

    [SerializeField] GameObject introObj;

    [SerializeField] UserLoginData[] userLoginData;

    EventSystem system;

    public void OnEnable()
    {
        if (GameManager.instance.gameState == GameState.LOGIN)
            GameManager.instance.GameManagerState(GameState.LOGIN);
        else
        {
            Debug.Log(GameManager.instance.gameState);
            UIManager.instance.UpdateSceneState(GameManager.instance.gameState);
        }

        // system = EventSystem.current;
        introObj.SetActive(true);

        int _random = Random.Range(0, userLoginData.Length);
        RandomUserSetting(_random);
    }
    public void OnDisable()
    {
        introObj.SetActive(false);
    }

    void RandomUserSetting(int _userLoginData)
    {
        input_nickname.text = userLoginData[_userLoginData].nickname;
        //inputField_ID.text = userLoginData[_userLoginData].id;
        //inputField_PW.text = userLoginData[_userLoginData].pw;
    }

    // 버튼클릭으로 로그인
    public void Button_Login()
    {
        //if (!GameManager.instance.isLoading && inputField_ID.text != "" && inputField_PW.text != "")
        if(input_nickname.text != "")
        {
            Login();
        }
    }

    // 엔터키로 로그인
    private void Update()
    {
        if (Input.GetKeyDown(KeyCode.Return))
        {
            //if (inputField_ID.text != "" && inputField_PW.text != "")
            if (input_nickname.text != "")
            {
                Login();
            }
        }
        //if(Input.GetKeyDown(KeyCode.Tab))
        //{
        //    Selectable next = system.currentSelectedGameObject.GetComponent<Selectable>().FindSelectableOnDown();
        //    if(next != null)
        //    {
        //        inputField_ID = next.GetComponent<InputField>();
        //        if (inputField_ID != null)
        //            inputField.OnPointerClick(new PointerEventData(system));

        //        system.SetSelectedGameObject(next.gameObject, new BaseEventData(system));
        //    }
        //}
    }

    /// <summary>
    /// 1. ID중복검사하기
    /// 2. 새로운 아이디 List에 추가
    /// 3. 돌리카트 이동시키기
    /// </summary>
    void Login()
    {
        // Debug.Log(inputField.text);
        // Debug.Log(GameManager.instance.otherPlayerList.Count);

        // 2.
        PlayerInfo _player = new PlayerInfo();
        _player.name = input_nickname.text;
        //_player.id = inputField_ID.text;
        //_player.pw = inputField_PW.text;

        LoginManager.Instance.DisposableLogIn(_player.name);

        GameManager.instance.setCurrentPlayer(_player);
    }
}