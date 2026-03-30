using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SnackDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public int staminaRecoverAmount = 0; // 부모가 세팅해줄 예정
    [HideInInspector] public string mySnackName;
    [HideInInspector] public int myStorageID; // 이 간식이 원래 어디 저장고에 있던 건지 (사용 후 개수 조정 위해)
    
    private GameObject dragGhost; // 드래그할 때 따라다닐 임시 아이콘
    private Canvas parentCanvas;
    private Image myImage;
    private RectTransform myRect;
    public SnackSlotUI mySlotUI;
    private void Awake()
    {
        myImage = GetComponent<Image>();
        myRect = GetComponent<RectTransform>();
        parentCanvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 1. 드래그 시작 시 임시 아이콘(Ghost) 생성
        dragGhost = new GameObject("DragGhost_Icon");
        dragGhost.transform.SetParent(parentCanvas.transform); // 캔버스 바로 아래로 (맨 위로 그리기 위해)
        dragGhost.transform.SetAsLastSibling(); // 제일 위에 그리기

        // 2. 이미지 복사
        Image ghostImg = dragGhost.AddComponent<Image>();
        ghostImg.sprite = myImage.sprite;
        ghostImg.raycastTarget = false; // 마우스 클릭 방해 금지 (매우 중요!)
        ghostImg.preserveAspect = true;

        // 3. 크기 맞추기
        RectTransform ghostRect = dragGhost.GetComponent<RectTransform>();
        ghostRect.sizeDelta = myRect.sizeDelta;
        
        // 4. 원래 아이콘은 투명하게 하거나 유지 (취향껏)
        // myImage.color = new Color(1,1,1, 0.5f); 
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragGhost == null) return;

        // 마우스 따라다니기 (UI 좌표 변환)
        Vector2 pos;
        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentCanvas.transform as RectTransform,
            eventData.position,
            parentCanvas.worldCamera,
            out pos);
        
        dragGhost.transform.localPosition = pos;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        // 1. 드래그 끝난 위치에 직원이 있는지 체크 (World 좌표 Raycast)
        Vector3 mouseWorldPos = Camera.main.ScreenToWorldPoint(Input.mousePosition);
        RaycastHit2D hit = Physics2D.Raycast(mouseWorldPos, Vector2.zero);

        if (hit.collider != null && hit.collider.CompareTag("Employee"))
        {
            EmployeeController employee = hit.collider.GetComponent<EmployeeController>();
                if (employee.currentState == EmployeeState.Idle)
                {
                    // 쉬고 있을 때만 간식을 먹고, DB에서 깎고, UI를 줄입니다.
                    employee.EatSnack(staminaRecoverAmount);
                    ServiceLocator.Get<GameData>().Inventory.AdjustSnackCount(myStorageID, mySnackName, -1);
                    mySlotUI.UseItem();
                }
                else
                {
                    // 일하는 중(Working)이라면 아무것도 안 하고 튕겨냅니다!
                    Debug.LogWarning("직원이 열심히 일하는 중이라 간식을 먹을 수 없습니다!");
                    // (선택 사항) 여기에 화면 중앙에 "일하는 중에는 먹일 수 없어요!" 같은 토스트 알림을 띄워주면 유저가 안 헷갈리겠죠?
                }
        }

        if (dragGhost != null) Destroy(dragGhost);
        myImage.color = Color.white;
    }
}