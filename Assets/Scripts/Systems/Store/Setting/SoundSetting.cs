using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.Audio;

public class SoundSetting : MonoBehaviour
{
    [Header("UI 연결")]
    public Slider bgmSlider;
    public Slider sfxSlider;

    // 오디오믹서
    public UnityEngine.Audio.AudioMixer audioMixer;

    [Header("저장 키 이름")]
    private const string BGM_KEY = "BGM_Volume";
    private const string SFX_KEY = "SFX_Volume";

    // Start is called before the first frame update
    void Start()
    {
        // 게임 시작 시 저장된 설정값 불러오기 (저장된 게 없으면 기본값 0.75f)
        float savedBGM = PlayerPrefs.GetFloat(BGM_KEY, 0.75f);
        float savedSFX = PlayerPrefs.GetFloat(SFX_KEY, 0.75f);

        // UI 슬라이더에 반영
        if (bgmSlider != null) bgmSlider.value = savedBGM;
        if (sfxSlider != null) sfxSlider.value = savedSFX;

        // 실제 오디오 소스에 적용 (최초 1회)
        ApplyBGMVolume(savedBGM);
        ApplySFXVolume(savedSFX);
    }

    // BGM 슬라이더 이벤트에 연결
    public void OnBGMChanged(float value)
    {
        ApplyBGMVolume(value);
        PlayerPrefs.SetFloat(BGM_KEY, value); 
        PlayerPrefs.Save(); // 즉시 물리적 저장

        //audioMixer.SetFloat("BGM", Mathf.Log10(bgmSlider.value) * 20);
    }

    // SFX 슬라이더 이벤트에 연결
    public void OnSFXChanged(float value)
    {
        ApplySFXVolume(value);
        PlayerPrefs.SetFloat(SFX_KEY, value);
        PlayerPrefs.Save();

        //audioMixer.SetFloat("SFX", Mathf.Log10(sfxSlider.value) * 20);
    }

    private void ApplyBGMVolume(float volume)
    {
        // 실제 AudioSource나 AudioMixer의 볼륨을 조절하는 로직을 여기에 넣으세요.
        float safeVolume = Mathf.Max(0.0001f, volume);
        audioMixer.SetFloat("BGM", Mathf.Log10(safeVolume) * 20);
        Debug.Log($"BGM 볼륨 적용 완료: {safeVolume}");
        
    }

    private void ApplySFXVolume(float volume)
    {
        float safeVolume = Mathf.Max(0.0001f, volume);
        audioMixer.SetFloat("SFX", Mathf.Log10(safeVolume) * 20);
        Debug.Log($"SFX 볼륨 적용 완료: {safeVolume}");
    }
}
