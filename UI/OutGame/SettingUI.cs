using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Audio;
using UnityEngine.UI;

public class SettingUI : MonoBehaviour
{
    public AudioMixer audioMixer;
    public Dropdown[] graphicDropdown;
    private void Start()
    {
        for (int i = 0; i < graphicDropdown.Length; i++)
        {
            graphicDropdown[i].value = QualitySettings.GetQualityLevel();
        }
    }

    // 그래픽
    public void SetGraphicQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }
    public void SetAntiQuality(bool value)
    {
        
    }
    public void SetPPQuality(bool value)
    {
        
    }
    public void SetTextureQuality(int qualityIndex)
    {
        QualitySettings.SetQualityLevel(qualityIndex);
    }

    // -80 ~ 0 : -40
    // (0~80)*1.25-80
    // 0 ~ 100 : 50
    // 사운드
    public void SetMasterVolume(float volume)
    {
        audioMixer.SetFloat("Master", volume);
    }
    public void SetBGMVolume(float volume)
    {
        audioMixer.SetFloat("BGM", volume);
    }
    public void SetSEVolume(float volume)
    {
        audioMixer.SetFloat("SE", volume);
    }
    public void SetVoiceVolume(float volume)
    {
        audioMixer.SetFloat("Voice", volume);
    }

    // 게임플레이
    public void SetMouseSens(float value)
    {

    }
    public void BattleHighlight(bool value)
    {

    }
    public void ShowPing(bool value)
    {

    }
    public void ChangeKey()
    {

    }


}
