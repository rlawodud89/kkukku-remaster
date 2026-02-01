using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingToolStoreProvider : IStoreItemProvider
{
    public bool isCountable { get; private set; } = false;
    public bool isGold { get; private set; } = true;

    private List<ToolItemSO> tools = new List<ToolItemSO>();

    public FishingToolStoreProvider()
    {
        tools.Add(ServiceLocator.Get<GameData>().Inventory.GetToolItemSO("기본낚시대"));
    }

    public List<(string itemName, Sprite itemSprite, int price)> LoadData()
    {
        List<(string itemName, Sprite itemSprite, int price)> data = new();

        foreach (var item in tools)
        {
            data.Add((item.itemName, item.image, item.price));
        }

        return data;
    }
}
