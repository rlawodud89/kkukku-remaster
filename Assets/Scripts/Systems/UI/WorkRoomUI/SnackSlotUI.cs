using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class SnackSlotUI : MonoBehaviour
{
    [Header("Inner Component")]
    [SerializeField] private Image snackIconImage;       
    [SerializeField] private TextMeshProUGUI snackAmountText;      
    [SerializeField] private SnackDragHandler dragScript; 

    private int currentCount = 0; // ★ 현재 개수를 기억할 변수 추가

    public void SetSlotData(int storageID, string itemName, Sprite icon, int count, int staminaAmount)
    {
        currentCount = count; // ★ 데이터가 들어올 때 개수 저장

        if (snackIconImage != null) snackIconImage.sprite = icon;

        if (dragScript != null)
        {
            dragScript.staminaRecoverAmount = staminaAmount;
            dragScript.mySnackName = itemName; 
            dragScript.myStorageID = storageID;

            // ★ 중요: 드래그 스크립트가 나(UI)를 조종할 수 있게 연결해줍니다!
            dragScript.mySlotUI = this; 
        }
        
        if (snackAmountText != null) snackAmountText.text = currentCount.ToString();
    }
    
    // ★ 간식을 성공적으로 먹였을 때 호출될 함수!
    public void UseItem()
    {
        currentCount--; // 개수 1 감소

        if (currentCount > 0)
        {
            // 아직 남았으면 숫자 텍스트만 갱신
            if (snackAmountText != null) snackAmountText.text = currentCount.ToString();
        }
        else
        {
            // 0개가 되면 슬롯 자체를 파괴해서 안 보이게 만듦
            Destroy(gameObject); 
        }
    }
}