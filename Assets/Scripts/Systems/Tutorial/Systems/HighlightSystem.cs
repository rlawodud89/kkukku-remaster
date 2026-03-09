using UnityEngine;
using UnityEngine.UI;

public class HighlightSystem : MonoBehaviour
{
    public static HighlightSystem Instance;

    [Header("References")]
    [SerializeField] private Image maskImage;
    [SerializeField] private RectTransform finger;
    [SerializeField] private Canvas rootCanvas;

    [Header("Settings")]
    [SerializeField] private float padding;

    private Material runtimeMaterial;
    public static Vector2 CurrentHoleCenter;
    public static float CurrentHoleSize;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        runtimeMaterial = Instantiate(maskImage.material);
        maskImage.material = runtimeMaterial;

        maskImage.gameObject.SetActive(false);
        finger.gameObject.SetActive(false);
    }


    public void Highlight(RectTransform target)
    {
        // 마스크 활성화
        maskImage.gameObject.SetActive(true);

        // LayoutGroup / ContentSizeFitter 등으로 인해
        // 아직 RectTransform 계산이 끝나지 않았을 수 있으므로
        // 강제로 캔버스 업데이트
        Canvas.ForceUpdateCanvases();

        RectTransform canvasRect = rootCanvas.transform as RectTransform;

        
        // 1. 타겟의 실제 화면 중심 계산

        // RectTransform.position은 pivot 기준이므로
        // 정확한 중앙을 얻기 위해 월드 코너 사용
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        // 대각선 기준으로 실제 사각형 중심 계산
        Vector3 worldCenter = (corners[0] + corners[2]) * 0.5f;

        // 월드 좌표 → 스크린 좌표
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(
            rootCanvas.worldCamera,
            worldCenter);


        // 2. 스크린 좌표 → 캔버스 로컬 좌표

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            rootCanvas.worldCamera,
            out Vector2 localPoint);


        // 3. 셰이더에서 사용하는 0~1 정규화 좌표 계산

        // 캔버스 기준 로컬 좌표를
        // 셰이더에서 사용할 수 있도록 0~1 범위로 변환
        Vector2 normalized = new Vector2(
            (localPoint.x - canvasRect.rect.x) / canvasRect.rect.width,
            (localPoint.y - canvasRect.rect.y) / canvasRect.rect.height
        );

        runtimeMaterial.SetVector("_HoleCenter", normalized);


        // 4. 타겟의 실제 화면 크기 기반으로 홀 반지름 계산

        float screenWidth = Vector3.Distance(corners[0], corners[3]);
        float screenHeight = Vector3.Distance(corners[0], corners[1]);

        // 가로/세로 중 더 큰 값을 기준으로 반지름 계산
        // padding을 더해 여유 공간 확보
        float radius = Mathf.Max(screenWidth, screenHeight) * 0.5f + padding;


        // 5. 마스크 전체 크기 대비 정규화 반지름 계산

        RectTransform dimRect = maskImage.rectTransform;
        Vector3[] dimCorners = new Vector3[4];
        dimRect.GetWorldCorners(dimCorners);

        float dimScreenWidth = Vector3.Distance(dimCorners[0], dimCorners[3]);
        float dimScreenHeight = Vector3.Distance(dimCorners[0], dimCorners[1]);

        // 원형 마스크 기준이므로 작은 축 기준으로 정규화
        float minDimSize = Mathf.Min(dimScreenWidth, dimScreenHeight);

        float normalizedRadius = radius / minDimSize;

        CurrentHoleCenter = normalized;
        CurrentHoleSize = normalizedRadius;

        runtimeMaterial.SetVector("_HoleCenter", normalized);
        runtimeMaterial.SetFloat("_HoleSize", normalizedRadius);

 
        // 6. 손가락 위치 이동

        // 계산된 스크린 좌표를 기준으로
        // 캔버스 로컬 좌표로 변환하여 배치
        MoveFinger(screenPos);
    }

    private void MoveFinger(Vector2 screenPos)
    {
        RectTransform canvasRect = rootCanvas.transform as RectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            rootCanvas.worldCamera,
            out Vector2 localPoint);

        finger.localPosition = localPoint + new Vector2(0, -80f);
        finger.gameObject.SetActive(true);
    }

    public void Clear()
    {
        maskImage.gameObject.SetActive(false);
        finger.gameObject.SetActive(false);
    }

    public void HideFinger()
    {
        finger.gameObject.SetActive(false);
    }
}