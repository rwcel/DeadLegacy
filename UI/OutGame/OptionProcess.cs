using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;
using UnityEngine.Rendering.PostProcessing;

public enum Option_Button
{
//  게임플레이   비디오/사운드 전체 수(확인용)
    GAME_PLAY, VIDEO_SOUND, NUM_STAT
}
public enum Option_GamePlay
{
    BATTLE_HIGHLIGHT, PING,
    BTHL_ON=10, BTHL_OFF,
    PING_ON=20, PING_OFF
}

public enum Option_Sound
{
    MASTER, BACKGROUND, SOUND_EFFECT, VOICE
}
public enum Option_Visual
{
    VISUAL, ANTIALIASING, POSTPROCESSING, TEXTURE,
    ANTI_ON = 10, ANTI_OFF,
    PP_ON = 20, PP_OFF,
    TT_LOW = 30, TT_MIDDLE, TT_HIGH, TT_ULTRA

}
public enum Option_Visual_Level
{
    LOW, MIDDLE, HIGH, ULTRA, USER_SET
}


[System.Serializable]
public struct OptionContent
{
    public GameObject selectImage;
    public GameObject contentPanel;
}

[System.Serializable]
public struct SliderContent
{
    public Text value;
    public Slider slider;
}

[System.Serializable]
public struct ToggleContent
{
    public ToggleGroup group;
    [HideInInspector] public Toggle[] toggle;
}


public class OptionProcess : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Button[] optionButton;
    int lastOpen = 0;                                   // 마지막으로 설정한 옵션 번호

    [SerializeField] OptionContent[] optionContent;     // 옵션 내용들
    int currentOption;                                  // 현재 선택한 옵션 번호 (게임플레이인지 비디오/사운드인지)
    [SerializeField] GameObject disablePanel;           // 비활성화된 화면

    [SerializeField] ScrollRect scrollRect;             // 스크롤바
    [Header("[게임플레이 옵션]")]
    [SerializeField] ToggleContent battleHLToggle;
    [SerializeField] ToggleContent pingToggle;

    [Header("비디오/사운드 옵션")]
    [SerializeField] ToggleContent[] toggleContent;
    [SerializeField] SliderContent[] sound;

    int startQuality;           // 시작 퀄리티
    float startVolume = 10f;    // 시작 볼륨
    int maxVolume = 40;         // 최대 볼륨

    bool mainChange = false;

    // 마지막으로 설정한 옵션 키기
    private void OnEnable()
    {
        Button_Option(lastOpen);
    }

    /// <summary>
    /// 비주얼, 사운드 초기세팅 : GameManager에서 실행시켜야함
    /// </summary>
    public void StartSetting()
    {
        battleHLToggle.toggle = battleHLToggle.group.GetComponentsInChildren<Toggle>();
        pingToggle.toggle = pingToggle.group.GetComponentsInChildren<Toggle>();
        for (int i = 0; i < toggleContent.Length; i++)
        {
            toggleContent[i].toggle = toggleContent[i].group.GetComponentsInChildren<Toggle>();
        }

        startQuality = Mathf.Clamp(QualitySettings.GetQualityLevel(), 0, 3);
        toggleContent[(int)Option_Visual.VISUAL].toggle[startQuality].isOn = true;

        for (int i = 0; i < sound.Length; i++)
        {
            sound[i].value.text = "100";
        }
    }

    /// <summary>
    /// 0,1 : 버튼 눌렀을 때 컨텐츠 보이게 하기 + 스크롤바 데이터 넣기
    /// </summary>
    /// <param name="content"></param>
    public void Button_Option(int content)
    {
        switch ((Option_Button)content)
        {
            case Option_Button.GAME_PLAY:
                currentOption = 0;
                optionButton[0].Select();
                break;
            case Option_Button.VIDEO_SOUND:
                currentOption = 1;
                optionButton[1].Select();
                break;
        }

        for (int i = 0; i < (int)Option_Button.NUM_STAT; i++)
        {
            if (i == currentOption)
            {
                optionContent[i].selectImage.SetActive(true);
                optionContent[i].contentPanel.SetActive(true);
            }
            else
            {
                optionContent[i].selectImage.SetActive(false);
                optionContent[i].contentPanel.SetActive(false);
            }
        }

        scrollRect.content = optionContent[currentOption].contentPanel.GetComponent<RectTransform>();

    }

    public void Button_Exit()
    {
        lastOpen = currentOption;
        UIManager.instance.optionPanel.SetActive(false);
        UIManager.instance.mainmenuPanel.SetActive(true);
    }

    public void GamePlaySetting(int num)
    {
        switch ((Option_GamePlay)num)
        {
            case Option_GamePlay.BTHL_ON:
                break;
            case Option_GamePlay.BTHL_OFF:
                break;
            case Option_GamePlay.PING_ON:
                break;
            case Option_GamePlay.PING_OFF:
                break;
        }
    }

    /// <summary>
    /// 그래픽 세팅
    /// 1. 하위가 만져서 사용자설정으로 간 경우에는 다시 하위를 변경하게 하지 않음
    /// * 최상, 상 - 안티, 포프 : 활성화 / 텍스처 품질 같음
    /// * 중, 하 - 안티, 포프 : 비활성화 / 텍스쳐 품질 같음
    /// * 안티, 포프, 텍스쳐 수정 시 사용자 설정으로 변환 
    /// </summary>
    public void MainGraphicSetting(int num)
    {
        // 1.
        if(!toggleContent[(int)(Option_Visual.VISUAL)].toggle[num].isOn)
            return;

        // Debug.Log((Option_Visual_Level)num);
        switch ((Option_Visual_Level)num)
        {
            case Option_Visual_Level.LOW:
            case Option_Visual_Level.MIDDLE:
                mainChange = true;
                toggleContent[(int)(Option_Visual.ANTIALIASING)].toggle[1].isOn = true;
                toggleContent[(int)(Option_Visual.POSTPROCESSING)].toggle[1].isOn = true;
                toggleContent[(int)(Option_Visual.TEXTURE)].toggle[num].isOn = true;

                //antiToggle.toggle[1].isOn = true;
                //ppToggle.toggle[1].isOn = true;
                //textureToggle.toggle[num].isOn = true;
                mainChange = false;
                break;
            case Option_Visual_Level.HIGH:
            case Option_Visual_Level.ULTRA:
                mainChange = true;
                toggleContent[(int)(Option_Visual.ANTIALIASING)].toggle[0].isOn = true;
                toggleContent[(int)(Option_Visual.POSTPROCESSING)].toggle[0].isOn = true;
                toggleContent[(int)(Option_Visual.TEXTURE)].toggle[num].isOn = true;
                mainChange = false;
                break;
            case Option_Visual_Level.USER_SET:
                break;
        }

        if(num != (int)Option_Visual_Level.USER_SET)
            QualitySettings.SetQualityLevel(num);
    }

    /// <summary>
    /// 화면 품질을 만졌을 경우는 무시
    /// 나머지 그래픽들 만졌을 때 사용자 설정으로 바뀌기
    /// </summary>
    /// <param name="graphicNum">건드린 설정 번호</param>
    public void SubGraphicSetting(int graphicNum)
    {
        // Debug.Log(graphicNum);
        if (!mainChange)
        {
            toggleContent[(int)Option_Visual.VISUAL].toggle[(int)Option_Visual_Level.USER_SET].isOn = true;
        }


        switch ((Option_Visual)graphicNum)
        {
            case Option_Visual.ANTI_ON:
                QualitySettings.antiAliasing = 2;
                break;
            case Option_Visual.ANTI_OFF:
                QualitySettings.antiAliasing = 0;
                break;
            case Option_Visual.PP_ON:
                // Debug.Log("키세요");
                Camera.main.GetComponent<PostProcessLayer>().enabled = true;
                break;
            case Option_Visual.PP_OFF:
                // Debug.Log("끄세요");
                Camera.main.GetComponent<PostProcessLayer>().enabled = false;
                break;
            case Option_Visual.TT_LOW:
                QualitySettings.masterTextureLimit = 3;
                break;
            case Option_Visual.TT_MIDDLE:
                QualitySettings.masterTextureLimit = 2;
                break;
            case Option_Visual.TT_HIGH:
                QualitySettings.masterTextureLimit = 1;
                break;
            case Option_Visual.TT_ULTRA:
                QualitySettings.masterTextureLimit = 0;
                break;
        }
    }

    public void Slider_MasterSound(float value)
    {
        audioMixer.SetFloat("Master", value);
        sound[(int)Option_Sound.MASTER].value.text = ((int)(value+maxVolume)*2).ToString();
    }
    public void Slider_BGMSound(float value)
    {
        audioMixer.SetFloat("BGM", value);
        sound[(int)Option_Sound.BACKGROUND].value.text = ((int)(value + maxVolume) * 2).ToString();
    }
    public void Slider_SESound(float value)
    {
        audioMixer.SetFloat("SE", value);
        sound[(int)Option_Sound.SOUND_EFFECT].value.text = ((int)(value + maxVolume) * 2).ToString();
    }
    public void Slider_VoiceSound(float value)
    {
        audioMixer.SetFloat("Voice", value);
        sound[(int)Option_Sound.VOICE].value.text = ((int)(value + maxVolume) * 2).ToString();
    }

    public void Button_KeyChange(string key)
    {
        disablePanel.SetActive(true);
    }

    public void Button_AllClaer()
    {
        OptionReset(currentOption);
    }

    /// <summary>
    /// 옵션 리셋하기 - 버튼 누른경우 / 처음(-1 : default) - 전부 리셋
    /// 
    /// </summary>
    /// <param name="num"></param>
    void OptionReset(int num)
    {
        switch ((Option_Button)num)
        {
            case Option_Button.GAME_PLAY:
                battleHLToggle.toggle[0].isOn = true;
                pingToggle.toggle[0].isOn = true;
                break;
            case Option_Button.VIDEO_SOUND:
                toggleContent[0].toggle[startQuality].isOn = true;
                MainGraphicSetting(startQuality);

                for (int i = 0; i < sound.Length; i++)
                {
                    sound[i].slider.value = sound[i].slider.maxValue * 0.5f;
                    sound[i].value.text = ((int)sound[i].slider.value).ToString();
                }
                break;
            default:
                OptionReset(0);
                OptionReset(1);
                break;
        }
    }

    public void Button_DisablePanel()
    {
        disablePanel.SetActive(false);
    }
}