using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomInteriorStoreProvider : IStoreItemProvider
{
    public bool isCountable { get; private set; } = true;
    public bool isGold { get; private set; } = true;

    public List<(string itemName, Sprite itemSprite, int price)> LoadItemData()
    {
        List<(string itemName, Sprite itemSprite, int price)> data = new();

        // DB에서 오늘 판매하는 아이템 리스트 받아오기
        foreach (var item in ServiceLocator.Get<GameData>().Store.GetRoomInteriorStoreItemList())
        {
            data.Add((item.itemName, item.image, item.price));
        }

        return data;
    }

    public bool AddItem(string itemName, int count)
    {
        // 인테리어 재고 확인 후 추가
        // 남는 재고함이 없는 경우, false return

        return ServiceLocator.Get<GameData>().Inventory.AddRoomInteriorItem(itemName, count);
    }

    public string GetDescription(string itemName)
    {
        RoomInteriorItemSO item = ServiceLocator.Get<GameData>().Inventory.GetRoomInteriorItemSO(itemName);

        switch (item.roomInteriorType)
        {
            case RoomInteriorType.BLANKET_BOX:
            case RoomInteriorType.MATERIAL_BOX:
            case RoomInteriorType.SNACK_BOX:
                return $"최대 저장량: {item.slotCount}";

            case RoomInteriorType.CRAFTING_TABLE:
                return "레시피 발견을 위한 제작대";
            case RoomInteriorType.INTERIOR:
                return "단순 인테리어 아이템";
            default:
                return "";
        }
    }
}
