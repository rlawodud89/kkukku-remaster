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

    public List<(string itemName, Sprite itemSprite, int price)> LoadItemData()
    {
        List<(string itemName, Sprite itemSprite, int price)> data = new();

        foreach (var worker in workers)
        {
            data.Add((worker.itemName, worker.image, worker.price));
        }

        return data;
    }

    public bool AddItem(string itemName, int count)
    {
        // TODO: 인테리어 재고 확인 후 추가
        // 남는 재고함이 없는 경우, false return

        return true;
    }
}
