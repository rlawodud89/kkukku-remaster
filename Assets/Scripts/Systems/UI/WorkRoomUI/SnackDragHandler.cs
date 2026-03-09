using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class SnackDragHandler : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [HideInInspector] public int staminaRecoverAmount = 0; // 부모가 세팅해줄 예정
    
    private GameObject dragGhost; // 드래그할 때 따라다닐 임시 아이콘
    private Canvas parentCanvas;
    private Image myImage;
    private RectTransform myRect;

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
            // 직원 스크립트 찾기
            EmployeeController employee = hit.collider.GetComponent<EmployeeController>();
            if (employee != null)
            {
                employee.RestoreStamina(staminaRecoverAmount);
                Debug.Log($"냠냠! 스태미나 {staminaRecoverAmount} 회복!");
                
                // (선택) 여기서 아이템 소모 처리를 하려면 부모나 매니저에게 알려야 함
                // GetComponentInParent<SnackSlotUI>().OnItemUsed(); 
            }
        }

        // 2. 임시 아이콘 삭제 및 원상복구
        if (dragGhost != null) Destroy(dragGhost);
        myImage.color = Color.white;
    }
}