using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SoundOption : MonoBehaviour
{
    // 오디오믹서
    public UnityEngine.Audio.AudioMixer audioMixer;

    // 슬라이더
    public Slider BGMSlider;
    public Slider SFXSlider;

    // 볼륨 조절
    public void SetBgmVolume(){
        audioMixer.SetFloat("BGM", Mathf.Log10(BGMSlider.value) * 20);
    }

    public void SetSfxVolume(){
        audioMixer.SetFloat("SFX", Mathf.Log10(SFXSlider.value) * 20);
    }
}
