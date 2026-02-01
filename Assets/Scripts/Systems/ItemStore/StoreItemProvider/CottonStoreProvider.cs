using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class CottonStoreProvider : IStoreItemProvider
{
    public bool isCountable { get; private set; } = true;
    public bool isGold { get; private set; } = false;

    private List<MaterialItemSO> items = new List<MaterialItemSO>();

    public CottonStoreProvider()
    {
        items.Add(ServiceLocator.Get<GameData>().Inventory.GetMaterialItemSO("운무솜"));
        items.Add(ServiceLocator.Get<GameData>().Inventory.GetMaterialItemSO("햇빛운무솜"));
        items.Add(ServiceLocator.Get<GameData>().Inventory.GetMaterialItemSO("천공운무솜"));
    }

    public List<(string itemName, Sprite itemSprite, int price)> LoadData()
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
}
