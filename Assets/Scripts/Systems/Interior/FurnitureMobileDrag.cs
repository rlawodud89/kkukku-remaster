using UnityEngine;
using UnityEngine.EventSystems;

public class FurnitureMobileDrag : MonoBehaviour, IPointerClickHandler, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    private Camera mainCamera;
    private SpriteRenderer spriteRenderer;

    [Header("Settings")]
    [SerializeField] private float dragThreshold = 0.5f; 

    private MaterialPropertyBlock mpb;
    private static readonly int OutlineAlphaID = Shader.PropertyToID("_OutlineAlpha");

    private bool isDragging = false;
    private bool isRealDrag = false;
    private Vector3 dragOffset;
    private Vector3 startDragPos;
    public int myID = -1;

    private void Start()
    {
        mainCamera = Camera.main;
        spriteRenderer = GetComponent<SpriteRenderer>();
        mpb = new MaterialPropertyBlock();
        
        if (TryGetComponent<WR_StorageController>(out var script))
        {
            myID = script.myStorageID;
        }
    }

    // =================================================================
    // ★ 씬 자동 판별 도우미 함수 (작업실인지 가게인지 알아서 판단합니다!)
    // =================================================================
    private bool IsEditModeActive()
    {
        if (RoomInteriorManager.Instance != null && RoomInteriorManager.Instance.IsEditMode) return true;
        if (ShopInteriorManager.Instance != null && ShopInteriorManager.Instance.IsEditMode) return true;
        return false;
    }

    private void RouteSelectFurniture()
    {
        if (RoomInteriorManager.Instance != null) RoomInteriorManager.Instance.SelectFurniture(this);
        else if (ShopInteriorManager.Instance != null) ShopInteriorManager.Instance.SelectFurniture(this);
    }

    private void RouteUpdateHighlight(Vector3 targetPos)
    {
        // (Clone) 글자가 붙어있으면 에러가 날 수 있으니 깔끔하게 떼고 보냅니다.
        string itemName = gameObject.name.Replace("(Clone)", "").Trim(); 
        
        if (RoomInteriorManager.Instance != null) RoomInteriorManager.Instance.UpdateGridHighlight(targetPos, myID, itemName);
        else if (ShopInteriorManager.Instance != null) ShopInteriorManager.Instance.UpdateGridHighlight(targetPos, myID, itemName);
    }
    // =================================================================

    public void OnPointerClick(PointerEventData eventData)
    {
        if (isRealDrag) return;
        HandleClick();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (!IsEditModeActive()) return;

        isDragging = true;
        isRealDrag = false;
        
        startDragPos = mainCamera.ScreenToWorldPoint(eventData.position);
        startDragPos.z = 0;
        
        dragOffset = transform.position - startDragPos;

        RouteSelectFurniture();
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
            if (InteractionUI.Instance != null) InteractionUI.Instance.HideMenu();
        }

        if (isRealDrag)
        {
            Vector3 targetPos = currentPos + dragOffset;
            SnapToGrid(targetPos);
            
            RouteUpdateHighlight(transform.position); 
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;

        if (isRealDrag)
        {
            // 드래그가 끝났을 때 상호작용 UI 열기
            if (InteractionUI.Instance != null) InteractionUI.Instance.OpenMenu(this);
        }
        else
        {
            HandleClick();
        }
        isRealDrag = false;
    }

    private void HandleClick()
    {
        if (!IsEditModeActive()) return;
        
        RouteSelectFurniture();
        
        // 클릭했을 때 상호작용 UI 열기
        if (InteractionUI.Instance != null) InteractionUI.Instance.OpenMenu(this);
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
    // ★ 씬에 맞춰서 스냅(자석) 기능도 알아서 분기 처리!
    // =================================================================
    private void SnapToGrid(Vector3 targetWorldPos)
    {
        if (RoomInteriorManager.Instance != null)
        {
            int gridIndex = RoomInteriorManager.Instance.WorldToGrid(targetWorldPos);
            if (gridIndex == -1) return;
            Vector3 finalPos = RoomInteriorManager.Instance.GridToWorld(gridIndex);
            transform.position = new Vector3(finalPos.x, finalPos.y, transform.position.z);
        }
        else if (ShopInteriorManager.Instance != null)
        {
            Vector3Int cellPos = ShopInteriorManager.Instance.floorTilemap.WorldToCell(targetWorldPos);
            int gridIndex = ShopStorageDataManager.Instance.pathfinding.PosToIndex(cellPos);
            if (gridIndex == -1) return;

            Vector3 finalPos = ShopInteriorManager.Instance.floorTilemap.CellToWorld(cellPos);
            transform.position = new Vector3(finalPos.x, finalPos.y, transform.position.z);
        }
    }
}