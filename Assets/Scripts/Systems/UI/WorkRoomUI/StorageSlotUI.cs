using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems; // 마우스 이벤트 처리를 위해 추가

// IPointerEnterHandler, IPointerExitHandler 인터페이스 상속 추가
public class StorageSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI countText; // 개수 표시
    [SerializeField] private Image iconImage;           // 아이콘 이미지

    [Header("체력 툴팁")]
    public GameObject TooltipObj; // 껐다 켤 툴팁 전체 오브젝트 (배경 포함)
    public TextMeshProUGUI TooltipText;
    
    // 내부 데이터 저장용 변수
    private int inventoryID;
    private string itemName; 
    private int currentCount; 

    private void Start()
    {
        // 시작 시 툴팁이 켜져 있다면 강제로 꺼줍니다.
        if (TooltipObj != null)
        {
            TooltipObj.SetActive(false);
        }
    }

    // 데이터 세팅 함수 (초기화)
    public void SetData(int id, string name, int count, Sprite icon)
    {
        inventoryID = id;      // ID 저장
        itemName = name;       // 이름 저장
        currentCount = count;  // 현재 보유량 저장

        // UI 갱신
        if (countText != null) countText.text = count.ToString();
        if (iconImage != null) iconImage.sprite = icon;
    }

    // 버튼 클릭 시 실행 (Inspector에서 Button OnClick에 연결)
    public void OnClickMaterialSlot()
    {
        if (RecipeCraftController.Instance == null)
        {
            Debug.LogError("[StorageSlotUI] RecipeCraftController가 씬에 없습니다!");
            return;
        }

        if (currentCount <= 0) return;

        RecipeCraftController.Instance.AddIngredient(inventoryID, itemName, iconImage.sprite, currentCount);
    }

    // ==========================================
    // 마우스 이벤트 처리 영역
    // ==========================================

    // 마우스 포인터가 UI 위로 올라왔을 때 (Hover)
    public void OnPointerEnter(PointerEventData eventData)
    {
        if (TooltipObj != null)
        {
            // 툴팁 활성화
            TooltipObj.SetActive(true);
            
            // 툴팁 텍스트에 아이템 이름 표시 (필요에 따라 정보 수정 가능)
            if (TooltipText != null)
            {
                TooltipText.text = itemName; 
            }
        }
    }

    // 마우스 포인터가 UI 밖으로 나갔을 때
    public void OnPointerExit(PointerEventData eventData)
    {
        if (TooltipObj != null)
        {
            // 툴팁 비활성화
            TooltipObj.SetActive(false);
        }
    }
}