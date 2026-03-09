using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SnackSlotUI : MonoBehaviour
{
    [Header("Inner Component")]
    [SerializeField] private Image snackIconImage;       // 안쪽 자식 이미지 연결
    [SerializeField] private TextMeshProUGUI snackAmountText;      // 안쪽 자식 텍스트 연결
    [SerializeField] private SnackDragHandler dragScript; // 안쪽 자식 스크립트 연결

    // 외부(팝업 매니저)에서 이 함수를 호출해서 데이터를 넣어줍니다.
    public void SetSlotData(int storageID, string itemName, Sprite icon, int count, int staminaAmount)
    {
        // 1. 아이콘 이미지 변경
        if (snackIconImage != null)
        {
            snackIconImage.sprite = icon;
        }

        // 2. 드래그 스크립트에 회복량 전달
        if (dragScript != null)
        {
            dragScript.staminaRecoverAmount = staminaAmount;
            dragScript.mySnackName = itemName; // 아이템 이름도 전달 (필요하면)
            dragScript.myStorageID = storageID;
        }
        
        // 3. 개수 텍스트 변경
        if (snackAmountText != null)
        {
            snackAmountText.text = count.ToString();
        }
    }
    
    // (추가 가능) 아이템 사용 후 슬롯을 비우거나 개수 줄이는 로직
    public void OnItemUsed()
    {
        // 개수 감소 로직 등...
        Destroy(gameObject); // 예시: 1회용이면 슬롯 삭제
    }
}