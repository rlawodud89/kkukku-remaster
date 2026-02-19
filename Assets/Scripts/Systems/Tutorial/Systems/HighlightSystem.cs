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
        maskImage.gameObject.SetActive(true);


        RectTransform canvasRect = rootCanvas.transform as RectTransform;

        // 타겟 월드 → 스크린
        Vector2 screenPos = RectTransformUtility.WorldToScreenPoint(
            rootCanvas.worldCamera,
            target.position);

        // 스크린 → 캔버스 로컬
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            screenPos,
            rootCanvas.worldCamera,
            out Vector2 localPoint);

        // 0~1 정규화
        Vector2 normalized = new Vector2(
            (localPoint.x - canvasRect.rect.x) / canvasRect.rect.width,
            (localPoint.y - canvasRect.rect.y) / canvasRect.rect.height
        );

        runtimeMaterial.SetVector("_HoleCenter", normalized);


        // 실제 화면상 크기 계산
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        float screenWidth = Vector3.Distance(corners[0], corners[3]);
        float screenHeight = Vector3.Distance(corners[0], corners[1]);

        float radius = Mathf.Max(screenWidth, screenHeight) * 0.5f + padding;

        // DimMask의 실제 화면 크기
        RectTransform dimRect = maskImage.rectTransform;
        Vector3[] dimCorners = new Vector3[4];
        dimRect.GetWorldCorners(dimCorners);

        float dimScreenWidth = Vector3.Distance(dimCorners[0], dimCorners[3]);
        float dimScreenHeight = Vector3.Distance(dimCorners[0], dimCorners[1]);

        float minDimSize = Mathf.Min(dimScreenWidth, dimScreenHeight);

        float normalizedRadius = radius / minDimSize;

        CurrentHoleCenter = normalized;
        CurrentHoleSize = normalizedRadius;
        runtimeMaterial.SetVector("_HoleCenter", normalized);
        runtimeMaterial.SetFloat("_HoleSize", normalizedRadius);


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