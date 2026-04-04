using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class StorageSlotUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI countText; // 개수 표시
    [SerializeField] private Image iconImage;           // 아이콘 이미지

    
    [Header("툴팁 UI 연결")]
    public GameObject tooltipObj;          // 마우스를 올리면 켜질 툴팁 배경 오브젝트
    public TextMeshProUGUI tooltipText;
    
    // 내부 데이터 저장용 변수
    private int inventoryID;
    private string itemName; 
    private int currentCount; 

    private void Start()
    {
        // 시작할 때는 툴팁이 안 보이게 꺼둡니다.
        if (tooltipObj != null) tooltipObj.SetActive(false);
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
    
    public void OnPointerEnter(PointerEventData eventData)
    {
        tooltipText.text = itemName; // 데이터에서 이름을 가져와 적어줍니다.
        tooltipObj.SetActive(true);         // 툴팁을 켭니다.
    }

    // 마우스가 이 UI 밖으로 빠져나갔을 때 자동으로 실행됩니다.
    public void OnPointerExit(PointerEventData eventData)
    {
        if (tooltipObj != null)
        {
            tooltipObj.SetActive(false); // 툴팁을 다시 끕니다.
        }
    }
    
}