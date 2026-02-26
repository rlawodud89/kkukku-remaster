using UnityEngine;
using UnityEngine.UI;
using TMPro;
using System.Collections.Generic;
using System.Linq;

public class RecipeCraftingSlot : MonoBehaviour // 이제 MonoBehaviour를 상속받습니다
{
    [Header("UI 연결")]
    public Image iconImage;
    public TextMeshProUGUI qtyText;
    
    [Header("버튼 연결")]
    public Button plusBtn;  // + 버튼
    public Button minusBtn; // - 버튼

    public string ItemName { get; private set; } // 아이템 이름
    public int CurrentSlotQty { get; private set; }  // 현재 슬롯에 올린 개수
    public bool IsEmpty { get; private set; } = true;


    public Dictionary<int, int> sourceBoxes = new Dictionary<int, int>();

    private int lastUsedBoxID = -1;
    private int lastBoxHaveQty = 0;

    private void Start()
    {
        // 버튼에 기능 연결
        plusBtn.onClick.AddListener(OnClickPlus);
        minusBtn.onClick.AddListener(OnClickMinus);
        
        // 처음엔 빈 슬롯이므로 숨기기 등 초기화
        Clear(); 

    }

    public void AddItem(int boxID, string name, Sprite icon, int boxHaveQty)
    {
        if (IsEmpty)
        {
            ItemName = name;
            CurrentSlotQty = 0;
            iconImage.sprite = icon;
            IsEmpty = false;
        }
        else if (ItemName != name)
        {
            Debug.LogWarning("다른 종류의 재료는 같은 슬롯에 올릴 수 없습니다!");
            return;
        }

        // 2. 이 상자에서 이미 가져온 개수 확인
        int currentFromThisBox = sourceBoxes.ContainsKey(boxID) ? sourceBoxes[boxID] : 0;

        // 3. 이 상자의 한도를 초과했는지 검사
        if (currentFromThisBox + 1 > boxHaveQty)
        {
            Debug.Log($"[{boxID}번 상자]의 {name} 잔여량이 부족하여 더 담을 수 없습니다!");
            return;
        }

        // 4. 장부에 기록 (어느 상자에서 몇 개 뺐는지)
        if (sourceBoxes.ContainsKey(boxID)) 
            sourceBoxes[boxID]++;
        else 
            sourceBoxes.Add(boxID, 1);

        // 5. 다음 + 버튼 클릭을 위해 정보 기억
        lastUsedBoxID = boxID;
        lastBoxHaveQty = boxHaveQty;
        CurrentSlotQty++;
        UpdateUI();
    }

    // 초기화
    public void Clear()
    {
        ItemName = "";
        CurrentSlotQty = 0;
        IsEmpty = true;
        
        sourceBoxes.Clear(); // 장부 초기화
        lastUsedBoxID = -1;
        lastBoxHaveQty = 0;
        
        qtyText.text = "0";
    }

    private void UpdateUI()
    {
        qtyText.text = CurrentSlotQty.ToString();
    }

    // + 버튼 눌렀을 때
    public void OnClickPlus()
    {
        if (IsEmpty || lastUsedBoxID == -1) return;

        // + 버튼은 '마지막으로 재료를 빼왔던 상자'에서 추가로 빼오도록 시도합니다.
        AddItem(lastUsedBoxID, ItemName, iconImage.sprite, lastBoxHaveQty);
    }

    // - 버튼 눌렀을 때
    public void OnClickMinus()
    {
        if (CurrentSlotQty <= 0) return;

        // 1. 장부에서 하나 빼기 (가장 최근에 기록된 상자부터 차감)
        int boxToReduce = sourceBoxes.Keys.LastOrDefault(); 

        if (sourceBoxes.ContainsKey(boxToReduce))
        {
            sourceBoxes[boxToReduce]--;
            
            // 만약 해당 상자에서 빼온 개수가 0이 되면 장부에서 아예 지움
            if (sourceBoxes[boxToReduce] <= 0)
            {
                sourceBoxes.Remove(boxToReduce);
            }
        }

        CurrentSlotQty--;

        // 2. 0개가 되면 슬롯 비우기
        if (CurrentSlotQty <= 0)
        {
            Clear();
        }
        else
        {
            UpdateUI();
        }
    }
    
}