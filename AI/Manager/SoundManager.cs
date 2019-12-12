using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using DG.Tweening;

public enum BGMSoundID
{
    OutGame,
    InGame,
    BGM1,
    BGM2,
    BGM3,
    BGM4,
    BGM5
}


public enum PlayerSoundID
{
    Parkour,
    Shielding,
    ShieldAttack,
    ShieldRush,
    Sniping,
    Trap,
    StimPistol,
    Die
}
public class SoundManager : MonoBehaviour
{
    private static SoundManager m_instance;         // 사운드매니저 싱글톤

    private float masterVolumeSFX = 1f;
    private float masterVolumeBGM = 1f;

    float soundFadeTime = 2.0f;                     // 소리 사라지는 시간 : (배경음)

    public AudioSource bgmAudioSource;              // 배경음 소스
    int bgmSoundNum;
    [SerializeField] AudioClip[] bgmSound;          // 배경음

    public AudioSource zombieAudioSource;
    int randomZombieDeathSound;                     // 무작위 좀비 죽는 소리
    [SerializeField] AudioClip[] zombieDeathSound;  // 좀비 죽는 소리

    public AudioSource playerAudioSource;
    [SerializeField] AudioClip[] playerSoundClip;  // 플레이어 관련 소리

    /// <summary>
    /// 싱글톤 접근 프로퍼티
    /// 싱글톤 변수에 아직 오브젝트가 할당되지 않았다면
    /// 씬에서 SoundManager 오브젝트를 찾아 할당
    /// </summary>
    public static SoundManager instance
    {
        get
        {
            if (m_instance == null)
            {
                m_instance = FindObjectOfType<SoundManager>();
            }
            return m_instance;
        }
    }


    /// <summary>
    /// 싱글톤 오브젝트가 이미 있으면 파괴하기 (중복방지)
    /// </summary>
    public void Awake()
    {
        if (instance != this)
        {
            Destroy(gameObject);
        }
    }

    public void PlayerSound(PlayerSoundID _playerSound)
    {
        playerAudioSource.PlayOneShot(playerSoundClip[(int)_playerSound], 1f * masterVolumeSFX);
    }


    /// <summary>
    /// 한번에 한 소리만 나오게 하기
    /// 저장된 소리 중 랜덤으로 나오기
    /// 성별에 따른 피치 조절
    /// </summary>
    /// <param name="gender"></param>
    public void ZombieHitSound(Gender gender)
    {
        if (zombieAudioSource.isPlaying)
            return;

        randomZombieDeathSound = Random.Range(0, zombieDeathSound.Length);
        switch (gender)
        {
            case Gender.FEMALE:
                zombieAudioSource.pitch = 1.3f;
                break;
            case Gender.MALE:
                zombieAudioSource.pitch = 1.0f;
                break;
            // **피치가 아닌 다른것 수정해야함
            case Gender.NEUTRAL:
                zombieAudioSource.pitch = 0.5f;
                break;
        }
        zombieAudioSource.PlayOneShot(zombieDeathSound[randomZombieDeathSound]);
    }

    /// <summary>
    /// BGM 플레이
    /// </summary>
    /// <param name="_bgmNumber"></param>
    public void PlayBGMSound(BGMSoundID _bgmNumber)
    {
        bgmAudioSource.DOFade(0, soundFadeTime);
        bgmSoundNum = (int)_bgmNumber;
        StartCoroutine(BgmFadeOut(soundFadeTime));
    }

    IEnumerator BgmFadeOut(float _time)
    {
        yield return new WaitForSeconds(_time);
        bgmAudioSource.clip = bgmSound[bgmSoundNum];
        bgmAudioSource.clip = bgmAudioSource.clip;
        bgmAudioSource.Play();
        bgmAudioSource.DOFade(1, 1.0f);
    }
}