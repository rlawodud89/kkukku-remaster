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
    private GameObject ghostIcon; // 드래그할 때 따라다닐 반투명 이미지

    // 매니저가 슬롯을 업데이트할 때 부르는 함수 (showCount로 텍스트 on/off 제어)
    public void UpdateSlot(Sprite icon, string itemName, int count = 0, bool showCount = false)
    {
        if (icon != null)
        {
            itemIcon.sprite = icon;
            itemIcon.gameObject.SetActive(true);
            currentItemName = itemName;

            // 가구일 때만 개수 텍스트 켜기
            if (showCount)
            {
                countText.text = count.ToString();
                countText.gameObject.SetActive(true);
            }
            else
            {
                countText.gameObject.SetActive(false);
            }
        }
        else
        {
            // 빈 슬롯 처리
            itemIcon.gameObject.SetActive(false);
            countText.gameObject.SetActive(false);
            currentItemName = "";
        }
    }

    // --- 여기서부터 드래그 기능 ---
    public void OnBeginDrag(PointerEventData eventData)
    {
        // 빈 슬롯이면 드래그 불가
        if (string.IsNullOrEmpty(currentItemName)) return;

        ghostIcon = new GameObject("GhostIcon");
        Canvas canvas = GetComponentInParent<Canvas>();
        ghostIcon.transform.SetParent(canvas.transform, false);
        ghostIcon.transform.SetAsLastSibling(); // 맨 앞으로 가져오기

        Image ghostImage = ghostIcon.AddComponent<Image>();
        ghostImage.sprite = itemIcon.sprite;
        ghostImage.SetNativeSize();

        Color c = ghostImage.color;
        c.a = 0.6f; // 반투명
        ghostImage.color = c;
        ghostImage.raycastTarget = false; // 마우스 클릭 방해 방지
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
            Destroy(ghostIcon); // 드래그 끝나면 가짜 이미지 삭제

            // TODO: 여기서 마우스를 놓은 위치(eventData.position)를 계산해서
            // currentItemName을 바탕으로 맵에 설치하는 로직을 부르면 됩니다!
            Debug.Log($"[{currentItemName}] 드래그 끝! 설치 시도");
        }
    }
}