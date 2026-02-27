using UnityEngine;
using UnityEngine.UI;

public class FallingFishUI : MonoBehaviour
{
    [Header("UI 연결")]
    public Image myImage;

    [Header("Growing Effect (커지는 효과)")]
    [SerializeField] private float growDuration = 0.5f; // 커지는 데 걸리는 시간 (초)
    [SerializeField] private Vector3 startScale = new Vector3(0.1f, 0.1f, 1f); // 시작 크기 (아주 작게)
    
    private Vector3 targetScale = Vector3.one; // 최종 크기 (1,1,1)
    private float growTimer = 0f; // 타이머
    public string itemName; // 어떤 아이템인지 저장 (획득 시 사용)
    private float speed;
    private FishingManagerUI manager;
    private float targetY;
    private RectTransform myRect; // 매번 GetComponent 하면 느리니까 캐싱

    public void Setup(float fallSpeed, FishingManagerUI uiManager, float barY, Sprite sprite, string itemName)
    {
        this.itemName = itemName;
        speed = fallSpeed;
        manager = uiManager;
        targetY = barY;
        myRect = GetComponent<RectTransform>();

        if (myImage != null && sprite != null)
        {
            myImage.sprite = sprite;
            myImage.preserveAspect = true; // 비율 유지
            
            // ★ 중요: 시작할 때 크기를 아주 작게 설정
            myRect.localScale = startScale;
            growTimer = 0f; // 타이머 초기화
            
        }
    }

    void Update()
    {
        // 1. 아래로 떨어지기
        transform.Translate(Vector3.down * speed * Time.deltaTime);

        // ★ 2. 시간 경과에 따라 크기 키우기 (Lerp)
        if (growTimer < growDuration)
        {
            growTimer += Time.deltaTime;
            // 진행률 계산 (0% -> 100%)
            float progress = growTimer / growDuration; 
            // 부드럽게 크기 변경 (시작크기 -> 목표크기)
            myRect.localScale = Vector3.Lerp(startScale, targetScale, progress);
        }

        // 3. 화면 밖 Miss 처리
        if (myRect.anchoredPosition.y < targetY - 200f)
        {
            manager.OnFishMiss(this);
            Destroy(gameObject);
        }
    }
}