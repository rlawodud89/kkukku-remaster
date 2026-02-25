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
        // ✨ 빈 슬롯이거나 비활성화(isDisabled) 상태면 드래그 시작 자체를 막음!
        if (string.IsNullOrEmpty(currentItemName) || isDisabled) return;

        ghostIcon = new GameObject("GhostIcon");
        Canvas canvas = GetComponentInParent<Canvas>();
        ghostIcon.transform.SetParent(canvas.transform, false);
        ghostIcon.transform.SetAsLastSibling();

        Image ghostImage = ghostIcon.AddComponent<Image>();
        ghostImage.sprite = itemIcon.sprite;
        ghostImage.SetNativeSize();

        Color c = ghostImage.color;
        c.a = 0.6f;
        ghostImage.color = c;
        ghostImage.raycastTarget = false;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (ghostIcon != null)
        {
            ghostIcon.transform.position = Input.mousePosition; // 마우스 따라다니기
        }
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (ghostIcon != null)
        {
            Destroy(ghostIcon);

            // 가구(0)는 다른 분이 담당하시니까 우리는 무시합니다!
            if (myCategory == 0) return;

            // 맵에 타일/벽지를 설치하라고 매니저에게 마우스 위치와 함께 전달
            InventoryManager manager = FindObjectOfType<InventoryManager>();
            if (manager != null)
            {
                manager.PlaceTileOnMap(currentItemName, myCategory, Input.mousePosition);
            }
        }
    }
}