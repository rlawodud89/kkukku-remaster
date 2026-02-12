using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatheringToolStoreProvider : IStoreItemProvider
{
    public bool isCountable { get; private set; } = false;
    public bool isGold { get; private set; } = true;

    private List<ToolItemSO> tools = new List<ToolItemSO>();

    public GatheringToolStoreProvider()
    {
        tools.Add(ServiceLocator.Get<GameData>().Inventory.GetToolItemSO("기본채집망"));
    }

    public List<(string itemName, Sprite itemSprite, int price)> LoadItemData()
    {
        List<(string itemName, Sprite itemSprite, int price)> data = new();

        foreach (var item in tools)
        {
            data.Add((item.itemName, item.image, item.price));
        }

        return data;
    }

    public bool AddItem(string itemName, int count)
    {
        // 이미 존재하는 경우 return false

        return ServiceLocator.Get<GameData>().Inventory.AddToolItem(itemName);
    }

    public string GetDescription(string itemName)
    {
        ToolItemSO item = ServiceLocator.Get<GameData>().Inventory.GetToolItemSO(itemName);

        return $"클릭 필요 횟수: {item.needClickCount}번";
    }
}
