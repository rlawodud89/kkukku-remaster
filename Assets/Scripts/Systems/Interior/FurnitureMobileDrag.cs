using UnityEngine;
using UnityEngine.EventSystems;

public class FurnitureMobileDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Settings")]
    [SerializeField] private Grid grid;
    
    [Header("Object Size (Tile Count)")]
    [SerializeField] private Vector2Int objectSize = new Vector2Int(1, 1); 

    private bool isDragging = false;
    private Camera mainCamera;

    private void Start()
    {
        mainCamera = Camera.main;
        if (grid == null) grid = FindObjectOfType<Grid>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // [추가된 부분] 편집 모드가 아니면 여기서 멈춤! (드래그 시작 안 함)
        if (InteriorManager.Instance == null || !InteriorManager.Instance.IsEditMode) 
            return;

        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
        // isDragging이 true일 때만 작동하므로, OnBeginDrag에서 막히면 여기도 실행 안 됨
        if (isDragging)
        {
            Vector3 touchPos = mainCamera.ScreenToWorldPoint(eventData.position);
            touchPos.z = 0;
            SnapToGrid(touchPos);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    private void SnapToGrid(Vector3 targetWorldPos)
    {
        // (기존 코드와 동일)
        Vector3Int cellPos = grid.WorldToCell(targetWorldPos);
        Vector3 finalPos = grid.GetCellCenterWorld(cellPos);

        if (objectSize.x % 2 == 0) finalPos.x += grid.cellSize.x * 0.5f; 
        if (objectSize.y % 2 == 0) finalPos.y += grid.cellSize.y * 0.5f;

        transform.position = new Vector3(finalPos.x, finalPos.y, transform.position.z);
    }
}