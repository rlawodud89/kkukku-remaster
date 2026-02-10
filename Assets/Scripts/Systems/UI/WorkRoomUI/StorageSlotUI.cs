using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class StorageSlotUI : MonoBehaviour
{
    [Header("UI Components")]
    [SerializeField] private TextMeshProUGUI countText; // 개수 표시
    [SerializeField] private Image iconImage;           // 아이콘 이미지

    // 내부 데이터 저장용 변수
    private int inventoryID;
    private string itemName; 
    private int currentCount; 

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
}