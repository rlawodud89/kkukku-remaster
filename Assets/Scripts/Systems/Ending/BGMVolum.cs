using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class BGMVolum : MonoBehaviour
{
    public AudioSource audioSource;

    public void FadeOut(){
        StartCoroutine(FadeOutCoroutine(10f));
    }

    IEnumerator FadeOutCoroutine(float duration)
    {
        float startVolume = audioSource.volume;
        float endVol = 0f;

        for (float t = 0; t < duration; t += Time.deltaTime)
        {
            audioSource.volume = Mathf.Lerp(startVolume, endVol, t / duration);
            yield return null;
        }

        audioSource.volume = endVol;
    }
}
