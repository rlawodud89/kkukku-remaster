using UnityEngine;
using UnityEngine.EventSystems;

public class FurnitureMobileDrag : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Dependencies")]
    // [변경] Grid 컴포넌트 직접 참조 대신 매니저를 통합니다.
    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;

    [Header("Settings")]
    // [중요] Pivot이 Bottom-Left면 사이즈 보정 계산이 필요 없으므로 objectSize는 단순 데이터용입니다.
    [SerializeField] private Vector2Int objectSize = new Vector2Int(1, 1);
    [SerializeField] private float dragThreshold = 0.5f; 

    // 쉐이더 제어용 변수
    private MaterialPropertyBlock mpb;
    private static readonly int OutlineAlphaID = Shader.PropertyToID("_OutlineAlpha");

    private bool isDragging = false;
    private bool isRealDrag = false;
    private Vector3 dragOffset;
    private Vector3 startDragPos;

    private void Start()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        mpb = new MaterialPropertyBlock();
        
        // Grid 변수는 이제 SnapToGrid에서 InteriorManager를 직접 쓰므로 제거해도 됩니다.
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isRealDrag) return;
        HandleClick();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (InteriorManager.Instance == null || !InteriorManager.Instance.IsEditMode) return;

        isDragging = true;
        isRealDrag = false;
        
        startDragPos = mainCamera.ScreenToWorldPoint(eventData.position);
        startDragPos.z = 0;
        
        // [중요] 드래그 오프셋 계산
        // 내 현재 위치(BottomLeft)와 마우스 찍은 위치의 차이
        dragOffset = transform.position - startDragPos;

        InteriorManager.Instance.SelectFurniture(this);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!isDragging) return;

        Vector3 currentPos = mainCamera.ScreenToWorldPoint(eventData.position);
        currentPos.z = 0;

        if (!isRealDrag)
        {
            float distance = Vector3.Distance(startDragPos, currentPos);
            if (distance < dragThreshold) return; 
            
            isRealDrag = true;
            InteractionUI.Instance.HideMenu();
        }

        if (isRealDrag)
        {
            // 오프셋을 더한 "가구의 원점(Bottom-Left)이 있어야 할 위치"를 넘김
            SnapToGrid(currentPos + dragOffset);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        if (isRealDrag)
        {
            InteractionUI.Instance.OpenMenu(this);
        }
        else
        {
            HandleClick();
        }
        isRealDrag = false;
    }

    private void HandleClick()
    {
        if (InteriorManager.Instance == null || !InteriorManager.Instance.IsEditMode) return;
        InteriorManager.Instance.SelectFurniture(this);
        InteractionUI.Instance.OpenMenu(this);
    }

    public void SetHighlight(bool isOn)
    {
        if (spriteRenderer == null) return;
        spriteRenderer.GetPropertyBlock(mpb);
        float value = isOn ? 1f : 0f;
        mpb.SetFloat(OutlineAlphaID, value);
        spriteRenderer.SetPropertyBlock(mpb);
    }

    // =================================================================
    // ★ [핵심 수정] InteriorManager와 로직 통일 (이상한 움직임 해결)
    // =================================================================
    private void SnapToGrid(Vector3 targetWorldPos)
    {
        if (InteriorManager.Instance == null) return;

        // 1. 현재 좌표가 "몇 번 그리드"인지 매니저에게 물어봅니다.
        // (매니저가 -2.8 기준점 로직을 갖고 있으므로 이게 가장 정확합니다)
        int gridIndex = InteriorManager.Instance.WorldToGrid(targetWorldPos);

        // 2. 범위를 벗어났다면 이동하지 않습니다. (혹은 가장자리로 Clamp 가능)
        if (gridIndex == -1) return;

        // 3. 그 그리드 번호의 "정확한 좌표(Bottom-Left)"를 받아옵니다.
        Vector3 finalPos = InteriorManager.Instance.GridToWorld(gridIndex);

        // 4. 적용 (Width/Height 보정 계산 삭제함 -> Pivot이 구석이니까 필요 없음!)
        transform.position = new Vector3(finalPos.x, finalPos.y, transform.position.z);
    }

    public void StoreInInventory()
    {
        InteriorManager.Instance.RemoveFurnitureData(this);
        Destroy(gameObject);
    }
}