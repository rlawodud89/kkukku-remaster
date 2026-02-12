using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class RecipeCraftingSlot : MonoBehaviour // 이제 MonoBehaviour를 상속받습니다
{
    [Header("UI 연결")]
    public Image iconImage;
    public TextMeshProUGUI qtyText;
    
    [Header("버튼 연결")]
    public Button plusBtn;  // + 버튼
    public Button minusBtn; // - 버튼

    // 데이터
public int InventoryID { get; private set; } // 인벤토리 ID
    public string ItemName { get; private set; } // 아이템 이름
    public int CurrentSlotQty { get; private set; }  // 현재 슬롯에 올린 개수
    private int HaveQty;
    private bool isEmpty = true;

    private void Start()
    {
        // 버튼에 기능 연결
        plusBtn.onClick.AddListener(OnClickPlus);
        minusBtn.onClick.AddListener(OnClickMinus);
        
        // 처음엔 빈 슬롯이므로 숨기기 등 초기화
        Clear(); 

    }

    public void AddItem(int inventoryID,string name, Sprite icon, int totalHavequantity)
    {
        InventoryID = inventoryID;
        HaveQty = totalHavequantity;

        if (isEmpty)
        {
            ItemName = name;
            CurrentSlotQty = 1;
            iconImage.sprite = icon;
            isEmpty = false;
        }
        else
        {
            if (CurrentSlotQty+1 > HaveQty)
            {
                Debug.Log("더 이상 담을 수 없습니다!"+ CurrentSlotQty+" / "+ HaveQty);
            }
            else 
            {
            CurrentSlotQty++;
            }


        }
        UpdateUI();
    }

    // 초기화
    public void Clear()
    {
        ItemName = "";
        InventoryID = -1;
        CurrentSlotQty = 0;
        isEmpty = true;
        qtyText.text = "0";

    }

    private void UpdateUI()
    {
        qtyText.text = CurrentSlotQty.ToString();
    }

    // + 버튼 눌렀을 때
    public void OnClickPlus()
    {
        if (CurrentSlotQty+1 > HaveQty)
          {
              Debug.Log("더 이상 담을 수 없습니다!");
          }
        else 
          {
          CurrentSlotQty++;
          }
        UpdateUI();
    }

    // - 버튼 눌렀을 때
    public void OnClickMinus()
    {
        // 1개 줄이기
        CurrentSlotQty--;
        
        // TODO: 줄어든 1개만큼 가방(Inventory)으로 돌려줘야 함

        if (CurrentSlotQty <= 0)
        {
            // 0개가 되면 슬롯 비우기
            Clear();
        }
        else
        {
            UpdateUI();
        }
    }
    
    // 외부(매니저)에서 현재 상태 확인할 때 사용
    public bool IsEmpty => isEmpty;
}