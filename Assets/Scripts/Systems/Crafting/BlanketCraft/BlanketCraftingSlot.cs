using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class BlanketCraftingSlot : MonoBehaviour
{
    [Header("UI 연결")]
    public Image iconImage;
    public TextMeshProUGUI itemNameText;

    // 데이터
    public int InventoryID { get; private set; }
    public string ItemName { get; private set; }
    public int CurrentSlotQty { get; private set; }
    public int HaveQty { get; private set; }
    private bool isEmpty = true;

    public bool IsSufficient => HaveQty >= CurrentSlotQty;

    private void Start()
    {
        Clear();
    }


    public void SetRecipeItem(string name, Sprite icon, int requireCount, int totalHave)
    {

        ItemName = name;
        HaveQty = totalHave;
        iconImage.sprite = icon;
        isEmpty = false;

        CurrentSlotQty = requireCount;

        UpdateUI();
    }

    public void Clear()
    {
        ItemName = "";
        InventoryID = -1;
        CurrentSlotQty = 0;
        HaveQty = 0;
        isEmpty = true;
        itemNameText.text = ""; // 0 보다는 빈칸이 깔끔할 수 있음
        iconImage.sprite = null; // 아이콘 제거
    }

private void UpdateUI()
{
    itemNameText.text = $"{HaveQty}/{CurrentSlotQty}";

    if (!IsSufficient)
    {
        itemNameText.color = Color.red; // 부족하면 빨간색
    }
    else
    {
        itemNameText.color = Color.black; 
    }
}

    public bool IsEmpty => isEmpty;
}