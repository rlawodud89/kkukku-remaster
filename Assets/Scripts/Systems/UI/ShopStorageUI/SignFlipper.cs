using System.Collections;
using UnityEngine;
using UnityEngine.UI; // UI Image를 다루기 위해 필요
using UnityEngine.EventSystems; // 클릭 이벤트를 위해 필요

public class SignFlipper : MonoBehaviour, IPointerClickHandler
{
    [Header("설정")]
    [Tooltip("회전하는 데 걸리는 시간(초)")]
    public float flipDuration = 0.5f;

    [Header("스프라이트 참조")]
    public Sprite openSprite;   // OPEN 이미지 연결
    public Sprite closedSprite; // CLOSED 이미지 연결

    private Image signImage;
    private bool isAnimating = false; // 애니메이션 중복 실행 방지

    void Start()
    {
        // 컴포넌트 가져오기 및 초기화
        signImage = GetComponent<Image>();
        // 시작할 때 확실하게 OPEN 상태로 설정
        if (ShopManager.Instance.isStoreOpen)
        {
            signImage.sprite = openSprite;
        }
        else
        {
            signImage.sprite = closedSprite;
        }
    }

    // 오브젝트를 클릭했을 때 호출되는 함수 (EventTrigger 필요 없음)
    public void OnPointerClick(PointerEventData eventData)
    {
        if (GameManager.Instance.currentPhase == DayPhase.Night)
        {
            Debug.Log("밤에는 가게를 열 수 없습니다.");
            return;
        }
        // 애니메이션 중이 아닐 때만 실행
        if (!isAnimating)
        {
            StartCoroutine(FlipAnimation());
        }
    }
    public void ForceClose()
    {
        // 혹시 애니메이션 중 밤이 되었다면 코루틴 즉시 정지
        StopAllCoroutines();
        isAnimating = false;

        // 가게 상태를 강제로 '닫힘'으로 변경
        if (ShopManager.Instance.isStoreOpen)
        {
            ShopManager.Instance.ToggleStoreOpen();
        }

        // 시각적 상태 초기화
        signImage.sprite = closedSprite;
        transform.localRotation = Quaternion.Euler(0f, 0f, 0f);

        Debug.Log("밤이 되어 가게 문이 자동으로 닫혔습니다.");
    }
    IEnumerator FlipAnimation()
    {
        isAnimating = true;
        float timer = 0f;

        // 1. 절반(90도)까지 회전 (측면이 보일 때까지)
        while (timer < flipDuration / 2f)
        {
            timer += Time.deltaTime;
            // 0도에서 90도 사이를 시간 비례로 계산
            float angle = Mathf.Lerp(0f, 90f, timer / (flipDuration / 2f));
            // Y축 기준으로 회전 적용
            transform.localRotation = Quaternion.Euler(0f, angle, 0f);
            yield return null; // 다음 프레임까지 대기
        }

        // --- 딱 90도가 되어 안 보일 때 이미지 교체 및 상태 토글 ---
        ShopManager.Instance.ToggleStoreOpen();
        if (ShopManager.Instance.isStoreOpen)
        {
            signImage.sprite = openSprite;
        }
        else
        {
            signImage.sprite = closedSprite;
        }
        // -----------------------------------------------------------

        // 2. 나머지 절반(90도에서 0도) 회전 (반대편이 보이게)
        // *주의: 180도로 계속 돌리는 게 아니라, 90도에서 다시 0도로 돌아오는 것처럼 보여야 자연스럽습니다.
        timer = 0f;
        while (timer < flipDuration / 2f)
        {
            timer += Time.deltaTime;
            // 90도에서 0도 사이를 시간 비례로 계산
            float angle = Mathf.Lerp(90f, 0f, timer / (flipDuration / 2f));
            transform.localRotation = Quaternion.Euler(0f, angle, 0f);
            yield return null;
        }

        // 3. 애니메이션 종료 후 각도 확실하게 0으로 고정
        transform.localRotation = Quaternion.Euler(0f, 0f, 0f);
        isAnimating = false;
    }
}