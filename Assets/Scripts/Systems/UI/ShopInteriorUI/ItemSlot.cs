using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems; // 드래그 기능을 위해 추가
using TMPro;

// 드래그 관련 3가지 인터페이스 상속
public class ItemSlot : MonoBehaviour, IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [Header("UI 연결")]
    public Image itemIcon;
    public TextMeshProUGUI countText;

    private string currentItemName;
    private int myCategory; // 내가 지금 무슨 탭의 아이템인지 기억 (0:가구, 1:타일, 2:벽지)
    private GameObject ghostIcon;

    // ✨ 추가됨: 이 슬롯이 잠겨있는지(사용 중이거나 개수가 없는지) 확인
    private bool isDisabled = false;

    // ✨ 인자 추가: isEquipped (현재 장착중인지 여부)
    public void UpdateSlot(Sprite icon, string itemName, int category, int count = 0, bool showCount = false, bool isEquipped = false)
    {
        myCategory = category;

        if (icon != null)
        {
            itemIcon.sprite = icon;
            itemIcon.gameObject.SetActive(true);
            currentItemName = itemName;

            if (showCount)
            {
                countText.text = count.ToString();
                countText.gameObject.SetActive(true);
            }
            else
            {
                countText.gameObject.SetActive(false);
            }

            // ✨ 잠금 조건 검사: 가구인데 개수가 0이거나, 타일/벽지인데 현재 장착 중일 때
            if ((category == 0 && count <= 0) || ((category == 1 || category == 2) && isEquipped))
            {
                isDisabled = true;
                itemIcon.color = new Color(0.5f, 0.5f, 0.5f, 1f); // 아이콘을 어두운 회색으로 만듦
            }
            else
            {
                isDisabled = false;
                itemIcon.color = Color.white; // 원래 색상으로 복구
            }
        }
        else
        {
            itemIcon.gameObject.SetActive(false);
            countText.gameObject.SetActive(false);
            currentItemName = "";
            isDisabled = false;
        }
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // 빈 슬롯이거나 비활성화 상태면 드래그 막음
        if (string.IsNullOrEmpty(currentItemName) || isDisabled) return;

        ghostIcon = new GameObject("GhostIcon");
        Canvas canvas = GetComponentInParent<Canvas>();
        ghostIcon.transform.SetParent(canvas.transform, false);
        ghostIcon.transform.SetAsLastSibling();

        Image ghostImage = ghostIcon.AddComponent<Image>();
        ghostImage.sprite = itemIcon.sprite;

        // ✨ 수정된 부분: SetNativeSize()를 지우고, 슬롯 아이콘의 실제 크기를 복사합니다!
        RectTransform ghostRect = ghostIcon.GetComponent<RectTransform>();
        RectTransform iconRect = itemIcon.GetComponent<RectTransform>();
        ghostRect.sizeDelta = new Vector2(iconRect.rect.width, iconRect.rect.height);

        // 투명도 설정 및 마우스 클릭 방해 금지
        Color c = ghostImage.color;
        c.a = 0.6f;
        ghostImage.color = c;
        ghostImage.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghostIcon != null)
        {
            ghostIcon.transform.position = eventData.position;
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (ghostIcon != null)
        {
            Destroy(ghostIcon);

            if (EventSystem.current.IsPointerOverGameObject())
            {
                Debug.Log("<color=yellow>[배치 취소]</color> UI 창 위에 놓아서 배치가 취소되었습니다.");
                return;
            }
            
            
            RoomInventoryManager roomUIManager = FindObjectOfType<RoomInventoryManager>();
            InventoryManager shopUIManager = FindObjectOfType<InventoryManager>();

            Vector3 worldPoint = Camera.main.ScreenToWorldPoint(eventData.position);
            worldPoint.z = 0;

            // =========================================================
            // 💡 2. 가구 탭 (0) - 각각의 인테리어 매니저에게 전달
            // =========================================================
            if (myCategory == 0)
            {
                if (roomUIManager != null && RoomInteriorManager.Instance != null)
                {
                    RoomInteriorManager.Instance.DragDropFurnitureFromInventory(currentItemName, worldPoint);
                }
                else if (shopUIManager != null && ShopInteriorManager.Instance != null)
                {
                    // [이불가게]
                    ShopInteriorManager.Instance.DragDropFurnitureFromInventory(currentItemName, worldPoint);
                }
                
                return; // 가구 배치는 여기서 종료!
            }

            // =========================================================
            // 💡 3. 타일/벽지 탭 (1, 2) - 각각의 UI 매니저에게 전달
            // =========================================================
            if (roomUIManager != null)
            {
                // [작업실] RoomInventoryManager의 타일 설치 함수 호출
                roomUIManager.PlaceTileOnMap(currentItemName, myCategory, Input.mousePosition);
            }
            else if (shopUIManager != null)
            {
                // [이불가게] InventoryManager의 타일 설치 함수 호출
                shopUIManager.PlaceTileOnMap(currentItemName, myCategory, Input.mousePosition);
            }
        }
    }
}