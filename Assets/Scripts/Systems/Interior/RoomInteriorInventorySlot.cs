using UnityEngine.UI;
using TMPro;
using UnityEngine;

public class RoomInteriorInventorySlot : MonoBehaviour
{
    [SerializeField] private Image itemIcon;
    [SerializeField] private TextMeshProUGUI itemCountText;

    private FurnitureItem currentItem;


    public string myItemName; // SetItem 할 때 받아온 아이템 이름
    public Button myButton;   // 인스펙터에서 연결된 클릭 버튼

    private void Start()
    {
        // 버튼을 누르면 매니저의 스폰 함수를 부르도록 연결!
        if (myButton != null)
        {
            myButton.onClick.AddListener(OnClickSpawnFurniture);
        }
    }

    private void OnClickSpawnFurniture()
    {
        // 매니저야! 나 (나무상자) 맵에 소환해줘!
        InteriorManager.Instance.PlaceFurnitureFromInventory(myItemName);
        
    }

    public void SetItem(FurnitureItem item)
    {
        currentItem = item;
        myItemName = item.itemName; // 아이템 이름 저장
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (currentItem != null)
        {
            itemIcon.sprite = currentItem.itemImage;

            int count = currentItem.quantity;
            itemCountText.text = $"x{count}";
        }
        else
        {
            itemIcon.sprite = null;

            itemCountText.text = "";
        }
    }
}