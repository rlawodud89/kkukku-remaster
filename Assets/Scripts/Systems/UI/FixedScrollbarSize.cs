using UnityEngine;
using UnityEngine.UI;
using System.Collections;

public class FixedScrollbarSize : MonoBehaviour
{
    public ScrollRect scrollRect;
    public Scrollbar scrollbar;
    public float fixedSize = 0.1f; // 곰돌이 단추의 고정 크기

    void Start()
    {
        if (scrollRect == null || scrollbar == null) return;

        // 중요: ScrollRect 인스펙터의 Vertical Scrollbar 슬롯은 비워둬야 충돌이 없습니다.
        StartCoroutine(SyncRoutine());
    }

    IEnumerator SyncRoutine()
    {
        // 레이아웃이 완전히 계산될 때까지 대기
        yield return null;

        // 초기 사이즈 설정
        scrollbar.size = fixedSize;

        // 1. 컨텐츠 드래그 -> 스크롤바 이동 (기존 로직)
        scrollRect.onValueChanged.AddListener((Vector2 vec) => {
            scrollbar.size = fixedSize;
            scrollbar.value = vec.y;
        });

        // 2. 스크롤바(곰돌이) 드래그 -> 컨텐츠 이동 (추가된 로직!)
        scrollbar.onValueChanged.AddListener((float val) => {
            // 스크롤바의 값을 ScrollRect의 세로 위치로 전달
            scrollRect.verticalNormalizedPosition = val;
            // 곰돌이 크기가 변하지 않도록 다시 한번 고정
            scrollbar.size = fixedSize;
        });
    }
}