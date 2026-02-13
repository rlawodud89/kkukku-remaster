using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileInteriorStoreProvider : IStoreItemProvider
{
    public bool isCountable { get; private set; } = false;
    public bool isGold { get; private set; } = true;

    public List<(string itemName, Sprite itemSprite, int price)> LoadItemData()
    {
        List<(string itemName, Sprite itemSprite, int price)> data = new();

        // DB에서 오늘 판매하는 아이템 리스트 받아오기
        foreach (var item in ServiceLocator.Get<GameData>().Store.GetTileInteriorStoreItemList())
        {
            data.Add((item.itemName, item.image, item.price));
        }

        return data;
    }

    public bool AddItem(string itemName, int count)
    {
        // 이미 존재하는 경우 return false

        return ServiceLocator.Get<GameData>().Inventory.AddTileInteriorItem(itemName);
    }

    public string GetDescription(string itemName)
    {
        TileInteriorItemSO item = ServiceLocator.Get<GameData>().Inventory.GetTileInteriorItemSO(itemName);

        switch (item.tileType)
        {
            case TileInteriorType.WALL:
                return "벽타일";
            case TileInteriorType.FLOOR:
                return "바닥타일";
            default:
                return "";
        }
    }
}
