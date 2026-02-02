using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class YarnStoreProvider : IStoreItemProvider
{
    public bool isCountable { get; private set; } = true;
    public bool isGold { get; private set; } = false;

    private List<MaterialItemSO> items = new List<MaterialItemSO>();

    public YarnStoreProvider()
    {
        items.Add(ServiceLocator.Get<GameData>().Inventory.GetMaterialItemSO("꿈실"));
        items.Add(ServiceLocator.Get<GameData>().Inventory.GetMaterialItemSO("별빛꿈실"));
        items.Add(ServiceLocator.Get<GameData>().Inventory.GetMaterialItemSO("은하꿈실"));
    }

    public List<(string itemName, Sprite itemSprite, int price)> LoadItemData()
    {
        List<(string itemName, Sprite itemSprite, int price)> data = new();

        // int itemStoreLevel = ServiceLocator.Get<GameData>().User.
        int itemStoreLevel = 3;
        for (int i = 0; i < itemStoreLevel; i++)
        {
            data.Add((items[i].itemName, items[i].image, items[i].price));
        }

        return data;
    }

    public bool AddItem(string itemName, int count)
    {
        // TODO: 재고함 현황 보고, 자리 남는 곳에 추가
        // 남는 재고함이 없는 경우, false return

        return true;
    }
}
