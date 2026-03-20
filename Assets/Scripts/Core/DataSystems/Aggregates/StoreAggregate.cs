using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class StoreAggregate : IAggregate
{
    // === 런타임 데이터 ===

    private Dictionary<string, StoreItemList> storeItemList; // Key: itemName

    // === SO 데이터 ===

    private Dictionary<string, ShopInteriorItemSO> shopInteriorSOs;
    private Dictionary<string, RoomInteriorItemSO> roomInteriorSOs;
    private Dictionary<string, TileInteriorItemSO> tileInteriorSOs;
    private Dictionary<StoreType, StoreItemListSO> storeItemListSOs;

    // === 변경 사항 저장소 ===

    private Dictionary<string, SaveOperation> storeItemListChanges = new();

    // === 기타 데이터 === 

    private InteriorStoreSO interiorStoreSO;


    // === 저장 시스템 사용 메서드 ===

    public bool IsDirty { get; private set; }

    private void MarkDirty()
    {
        IsDirty = true;
        ServiceLocator.Get<DirtyDataRegistry>().RegisterDirty(this);
    }

    public void ClearDirty()
    {
        IsDirty = false;

        storeItemListChanges.Clear();
    }

    public IEnumerable<SavePayload> ToSavePayloads()
    {
        if (!IsDirty)
            yield break;

        // 판매 아이템 리스트
        foreach (var (itemName, change) in storeItemListChanges)
        {
            switch (change)
            {
                case SaveOperation.INSERT:
                    StoreItemList insertItem = storeItemList[itemName];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.INSERT,
                        Table = "StoreItemList",
                        Values = new Dictionary<string, object>
                        {
                            { "itemName", insertItem.itemName },
                            { "storeType", insertItem.storeType }
                        }
                    };
                    break;

                case SaveOperation.DELETE:
                    yield return new SavePayload
                    {
                        Operation = SaveOperation.DELETE,
                        Table = "StoreItemList",
                        Conditions = new Dictionary<string, object>
                        {
                            { "itemName", itemName }
                        }
                    };

                    break;
            }
        }

    }

    public void LoadStoreAggregate(IEnumerable<StoreItemList> storeItemList, Dictionary<string, ShopInteriorItemSO> shopInteriorSOs,
        Dictionary<string, RoomInteriorItemSO> roomInteriorSOs, Dictionary<string, TileInteriorItemSO> tileInteriorSOs,
        Dictionary<StoreType, StoreItemListSO> storeItemListSOs, InteriorStoreSO interiorStoreSO)
    {
        this.storeItemList = storeItemList.ToDictionary(i => i.itemName);

        this.shopInteriorSOs = shopInteriorSOs;
        this.roomInteriorSOs = roomInteriorSOs;
        this.tileInteriorSOs = tileInteriorSOs;
        this.storeItemListSOs = storeItemListSOs;
        this.interiorStoreSO = interiorStoreSO;
    }

    private void MergeChange<TKey>(Dictionary<TKey, SaveOperation> changes, TKey key, SaveOperation newOp)
    {
        if (!changes.TryGetValue(key, out var oldOp))
        {
            changes[key] = newOp;
            return;
        }

        switch (oldOp, newOp)
        {
            case (SaveOperation.INSERT, SaveOperation.UPDATE):
                // INSERT 유지
                break;

            case (SaveOperation.INSERT, SaveOperation.DELETE):
                // 생성했다가 삭제 → 아무 일도 없었던 것
                changes.Remove(key);
                break;

            case (SaveOperation.UPDATE, SaveOperation.UPDATE):
                // UPDATE 유지
                break;

            case (SaveOperation.UPDATE, SaveOperation.DELETE):
                changes[key] = SaveOperation.DELETE;
                break;

            case (SaveOperation.DELETE, SaveOperation.INSERT):
                // 삭제 후 다시 추가 → UPDATE로 취급
                changes[key] = SaveOperation.UPDATE;
                break;

            default:
                changes[key] = newOp;
                break;
        }
    }


    // === 게임 플레이 메서드 ===

    private ShopInteriorItemSO GetRandomShopInteriorItem()
    {
        ShopInteriorItemSO item = new ShopInteriorItemSO();

        do
        {
            int randomIdx = UnityEngine.Random.Range(0, shopInteriorSOs.Count);
            item = shopInteriorSOs.ElementAt(randomIdx).Value;
        } while (item.shopInteriorType == ShopInteriorType.CASHER);


        return item;
    }

    private RoomInteriorItemSO GetRandomRoomInteriorItem()
    {
        RoomInteriorItemSO item = new RoomInteriorItemSO();

        do
        {
            int randomIdx = UnityEngine.Random.Range(0, roomInteriorSOs.Count);
            item = roomInteriorSOs.ElementAt(randomIdx).Value;
        } while (item.roomInteriorType == RoomInteriorType.WORKER);


        return item;
    }

    private TileInteriorItemSO GetRandomTileInteriorItem()
    {
        TileInteriorItemSO item = new TileInteriorItemSO();

        do
        {
            int randomIdx = UnityEngine.Random.Range(0, tileInteriorSOs.Count);
            item = tileInteriorSOs.ElementAt(randomIdx).Value;
        } while (item.itemName == "가게기본바닥타일" || item.itemName == "가게기본벽타일");

        return item;
    }


    public List<ShopInteriorItemSO> GetShopInteriorStoreItemList()
    {
        var items = storeItemList.Values.
            Where(i => i.storeType == StoreType.SHOP_INTERIOR).
            ToList();

        List<ShopInteriorItemSO> list = new();

        foreach (var item in items)
        {
            list.Add(shopInteriorSOs[item.itemName]);
        }

        return list;
    }

    public List<RoomInteriorItemSO> GetRoomInteriorStoreItemList()
    {
        var items = storeItemList.Values.
            Where(i => i.storeType == StoreType.ROOM_INTERIOR).
            ToList();

        List<RoomInteriorItemSO> list = new();

        foreach (var item in items)
        {
            list.Add(roomInteriorSOs[item.itemName]);
        }

        return list;
    }

    public List<TileInteriorItemSO> GetTileInteriorStoreItemList()
    {
        var items = storeItemList.Values.
            Where(i => i.storeType == StoreType.TILE_INTERIOR).
            ToList();

        List<TileInteriorItemSO> list = new();

        foreach (var item in items)
        {
            list.Add(tileInteriorSOs[item.itemName]);
        }

        return list;
    }

    public List<MaterialItemSO> GetMaterialStoreItemList(StoreType materialStoreType)
    {
        if (materialStoreType == StoreType.YRAN_MATERIAL
            || materialStoreType == StoreType.COTTON_MATERIAL
            || materialStoreType == StoreType.MOONPIECE_MATERIAL)
        {
            return storeItemListSOs[materialStoreType].materialItems;
        }

        else return null;
    }

    public List<RoomInteriorItemSO> GetWorkerStoreItemList()
    {
        return storeItemListSOs[StoreType.WORKER].workerItems;
    }

    public List<ToolItemSO> GetToolStoreItemList(StoreType toolStoreType)
    {
        if (toolStoreType == StoreType.FISHING_TOOL
            || toolStoreType == StoreType.GATHERING_TOOL)
        {
            return storeItemListSOs[toolStoreType].toolItmes;
        }

        else return null;
    }

    public void ResetAllStoreItemList()
    {
        foreach (var (itemName, item) in storeItemList)
        {
            MergeChange(storeItemListChanges,
                itemName,
                SaveOperation.DELETE);
        }

        storeItemList.Clear();


        // 가게 인테리어
        int itemCount = 0;
        while (itemCount < interiorStoreSO.itemCount)
        {
            ShopInteriorItemSO shopItem = GetRandomShopInteriorItem();

            if (storeItemList.ContainsKey(shopItem.itemName)) continue;

            storeItemList.Add(shopItem.itemName, new StoreItemList
            {
                itemName = shopItem.itemName,
                storeType = StoreType.SHOP_INTERIOR
            });

            MergeChange(storeItemListChanges,
                shopItem.itemName,
                SaveOperation.INSERT);

            itemCount++;
        }

        // 작업실 인테리어
        itemCount = 0;
        while (itemCount < interiorStoreSO.itemCount)
        {
            RoomInteriorItemSO roomItem = GetRandomRoomInteriorItem();

            if (storeItemList.ContainsKey(roomItem.itemName)) continue;

            storeItemList.Add(roomItem.itemName, new StoreItemList
            {
                itemName = roomItem.itemName,
                storeType = StoreType.ROOM_INTERIOR
            });

            MergeChange(storeItemListChanges,
                roomItem.itemName,
                SaveOperation.INSERT);

            itemCount++;
        }

        // 타일 인테리어
        itemCount = 0;
        while (itemCount < interiorStoreSO.itemCount)
        {
            TileInteriorItemSO tileItem = GetRandomTileInteriorItem();

            if (storeItemList.ContainsKey(tileItem.itemName)) continue;

            storeItemList.Add(tileItem.itemName, new StoreItemList
            {
                itemName = tileItem.itemName,
                storeType = StoreType.TILE_INTERIOR
            });

            MergeChange(storeItemListChanges,
                tileItem.itemName,
                SaveOperation.INSERT);

            itemCount++;
        }


        MarkDirty();
    }

}
