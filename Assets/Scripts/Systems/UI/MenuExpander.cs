using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class MenuExpander : MonoBehaviour
{
    [Header("버튼들 (순서대로)")]
    public GameObject[] subButtons;
    // 버튼이 나오는 간격
    public float delayTime = 0.05f;
    // 현재 열려있는지 닫혀있는지 체크
    private bool _isOpen = false; 

    public void ToggleMenu()
    {
        // 작동 중인 코루틴이 있다면 멈춤 (광클 방지)
        StopAllCoroutines();

        // 열려있으면 닫음
        if (_isOpen)
        {
            StartCoroutine(CloseSequence());
        }
        // 닫혀있으면 염
        else
        {
            StartCoroutine(OpenSequence());
        }
    }

    IEnumerator OpenSequence()
    {
        _isOpen = true;

        for(int i = 0; i < subButtons.Length; i++)
        {
            subButtons[i].SetActive(true);
            yield return new WaitForSeconds(delayTime);  // 잠깐 대기
        }
    }

    IEnumerator CloseSequence()
    {
        _isOpen = false;

        for(int i = subButtons.Length-1;i>=0; i--)
        {
            subButtons[i].SetActive(false);
            yield return new WaitForSeconds(delayTime);  // 잠깐 대기
        }
    }
    
}
