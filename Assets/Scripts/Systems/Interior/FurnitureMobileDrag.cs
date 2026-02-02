using UnityEngine;
using UnityEngine.EventSystems;

public class FurnitureMobileDrag : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("Settings")]
    [SerializeField] private Grid grid;
    
    // [변경됨] 인스펙터에서 입력받지 않고 코드에서 자동 설정하므로 SerializeField 제거
    // (디버깅용으로 보고 싶다면 [SerializeField]를 다시 붙여도 되지만, 코드 값이 덮어씁니다)
    private Vector2Int objectSize; 

    private bool isDragging = false;
    private Camera mainCamera;
    
    // (선택사항) 드래그 시 위치 보정을 위한 변수 (이전 질문 피드백 반영)
    private Vector3 dragOffset; 

    private void Start()
    {
        mainCamera = Camera.main;
        if (InteriorManager.Instance != null && InteriorManager.Instance.mainGrid != null)
        {
            grid = InteriorManager.Instance.mainGrid;
        }

        if (grid == null)
        {
            grid = FindObjectOfType<Grid>();
            if (grid == null) Debug.LogError("씬에 Grid가 하나도 없습니다!");
        }
        // [핵심] 태그에 따라 사이즈 자동 설정
        SetSizeByTag();
    }

    // 태그별 사이즈 정의 함수
    private void SetSizeByTag()
    {
        switch (gameObject.tag)
        {
            case "BlanketStorage": 
                objectSize = new Vector2Int(2, 1);
                break;

            case "SnackBox": // 이불장 (예: 1x1)
                objectSize = new Vector2Int(1, 1);
                break;

            case "PersonalCraftBox": // 계산대 (예: 2x1)
                objectSize = new Vector2Int(2, 1);
                break;

            case "FoxEmployee":
                objectSize = new Vector2Int(2, 1);
                break;

            case "CatEmployee":
                objectSize = new Vector2Int(2, 1);
                break;

            case "LeopardEmployee":
                objectSize = new Vector2Int(2, 1);
                break;
            default: // 태그가 없거나 모르는 태그일 때 기본값
                objectSize = new Vector2Int(1, 1);
                // Debug.LogWarning($"[FurnitureMobileDrag] '{gameObject.tag}' 태그의 사이즈가 정의되지 않아 기본값(1,1)을 사용합니다.");
                break;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 편집 모드가 아니면 드래그 불가
        if (InteriorManager.Instance == null || !InteriorManager.Instance.IsEditMode) 
            return;

        isDragging = true;
        
        // (선택사항) 드래그 자연스럽게 하는 오프셋 계산 (아까 scale 0.7 문제 해결용)
        Vector3 mouseWorldPos = mainCamera.ScreenToWorldPoint(eventData.position);
        mouseWorldPos.z = 0;
        dragOffset = transform.position - mouseWorldPos;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (isDragging)
        {
            Vector3 touchPos = mainCamera.ScreenToWorldPoint(eventData.position);
            touchPos.z = 0;

            // 오프셋 적용 (자연스러운 드래그를 위해)
            // 만약 오프셋 없이 딱딱 붙는 게 좋다면 'touchPos'를 그대로 넣으세요.
            SnapToGrid(touchPos + dragOffset);
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        isDragging = false;
    }

    private void SnapToGrid(Vector3 targetWorldPos)
    {
        Vector3Int cellPos = grid.WorldToCell(targetWorldPos);
        Vector3 finalPos = grid.GetCellCenterWorld(cellPos);

        // objectSize가 위에서 설정된 값에 따라 계산됨
        if (objectSize.x % 2 == 0) finalPos.x += grid.cellSize.x * 0.5f; 
        if (objectSize.y % 2 == 0) finalPos.y += grid.cellSize.y * 0.5f;

        transform.position = new Vector3(finalPos.x, finalPos.y, transform.position.z);
    }
}