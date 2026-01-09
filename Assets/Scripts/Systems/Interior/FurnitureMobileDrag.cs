using UnityEngine;
using UnityEngine.EventSystems;

public class FurnitureMobileDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Settings")]
    [SerializeField] private Grid grid;
    
    [Header("Object Size (Tile Count)")]
    // 가구가 차지하는 타일 개수 (예: 가로 2칸이면 x=2)
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
        isDragging = true;
    }

    public void OnDrag(PointerEventData eventData)
    {
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
        // 1. 기본 타일 좌표 구하기
        Vector3Int cellPos = grid.WorldToCell(targetWorldPos);
        
        // 2. 해당 타일의 정중앙 위치 가져오기
        Vector3 finalPos = grid.GetCellCenterWorld(cellPos);

        // [핵심 로직] 크기가 짝수(2, 4...)라면 반 칸(0.5)만큼 이동시켜야 경계선에 맞음
        // Grid의 실제 셀 크기(cellSize)를 가져와서 계산하므로 0.7 스케일이어도 잘 작동함
        
        // 가로가 짝수 칸이면 X축으로 반 칸 이동
        if (objectSize.x % 2 == 0)
        {
            finalPos.x += grid.cellSize.x * 0.5f; 
        }

        // 세로가 짝수 칸이면 Y축으로 반 칸 이동
        if (objectSize.y % 2 == 0)
        {
            finalPos.y += grid.cellSize.y * 0.5f;
        }

        transform.position = new Vector3(finalPos.x, finalPos.y, transform.position.z);
    }
}