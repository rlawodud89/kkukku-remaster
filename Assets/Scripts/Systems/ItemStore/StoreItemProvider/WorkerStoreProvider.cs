using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class WorkerStoreProvider : IStoreItemProvider
{
    public bool isCountable { get; private set; } = true;
    public bool isGold { get; private set; } = false;

    private List<RoomInteriorItemSO> workers = new List<RoomInteriorItemSO>();

    public WorkerStoreProvider()
    {
        workers.Add(ServiceLocator.Get<GameData>().Inventory.GetRoomInteriorItemSO("여우"));
        workers.Add(ServiceLocator.Get<GameData>().Inventory.GetRoomInteriorItemSO("양"));
        workers.Add(ServiceLocator.Get<GameData>().Inventory.GetRoomInteriorItemSO("고양이"));
    }

    public List<(string itemName, Sprite itemSprite, int price)> LoadData()
    {
        List<(string itemName, Sprite itemSprite, int price)> data = new();

        foreach (var worker in workers)
        {
            data.Add((worker.itemName, worker.image, worker.price));
        }

        return data;
    }
}
