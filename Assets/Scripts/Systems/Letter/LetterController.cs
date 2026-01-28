using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class LetterController : MonoBehaviour
{
    [Header("UI Settings")]
    public GameObject letterContentPanel; // 편지지 UI 패널 (활성화/비활성화 대상)

    // 이미 열려있는지 확인하기 위한 변수
    private GameObject currentLetterInstance;


    // 편지 열기 기능
    public void OpenLetter()
    {
        // 1. 현재 씬에 있는 Canvas를 찾습니다. (UI는 Canvas 자식이어야 보임)
        Canvas canvas = FindObjectOfType<Canvas>();

        if (canvas != null)
        {
            // 2. 프리팹을 Canvas의 자식으로 생성(Instantiate)합니다.
            currentLetterInstance = Instantiate(letterContentPanel, canvas.transform);

            // 3. (중요) 생성된 패널의 위치를 화면 중앙(0,0,0)으로 초기화합니다.
            // 프리팹 저장 시 위치가 엉뚱한 곳에 있을 수 있기 때문입니다.
            currentLetterInstance.transform.localPosition = Vector3.zero;
            
            // 혹시 크기가 이상해지는 것을 방지하기 위해 스케일도 1로 맞춤
            currentLetterInstance.transform.localScale = Vector3.one; 
        }
        else
        {
            Debug.LogError("Scene에 Canvas가 없습니다! UI > Canvas를 생성해주세요.");
        }
    }
}
