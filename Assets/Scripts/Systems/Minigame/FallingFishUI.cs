using UnityEngine;
using UnityEngine.UI; // UI 관련 기능 필수

public class FallingFishUI : MonoBehaviour
{
    private float fallSpeed;
    private FishingManagerUI gameManager;
    private RectTransform rectTransform; // UI 위치 제어용

    private float startY;
    private float targetY;

    [Header("Perspective Settings")]
    [SerializeField] private float startScale = 0.5f; // 시작 크기
    [SerializeField] private float endScale = 1.5f;   // 도착 크기

    public void Setup(float speed, FishingManagerUI manager, float targetHeight)
    {
        rectTransform = GetComponent<RectTransform>();
        fallSpeed = speed;
        gameManager = manager;
        
        // UI 좌표 기준 시작/목표 높이 설정
        startY = rectTransform.anchoredPosition.y;
        targetY = targetHeight;

        // 초기 크기 설정
        transform.localScale = Vector3.one * startScale;
    }

    void Update()
    {
        // 1. 아래로 떨어지기 (UI 좌표계인 anchoredPosition 사용)
        rectTransform.anchoredPosition += Vector2.down * fallSpeed * Time.deltaTime;

        // 2. 원근감 (크기 변화)
        float currentY = rectTransform.anchoredPosition.y;
        float progress = Mathf.InverseLerp(startY, targetY, currentY);
        float currentScale = Mathf.Lerp(startScale, endScale, progress);
        
        transform.localScale = Vector3.one * currentScale;

        // 3. 화면 밖으로 나가면 삭제 (Target보다 더 아래로 200픽셀 정도 갔을 때)
        if (currentY < targetY - 200f)
        {
            gameManager.OnFishMiss(this);
            Destroy(gameObject);
        }
    }
}