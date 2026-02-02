using System.Collections;
using System.Collections.Generic;
using System.Linq;
using Unity.VisualScripting;
using UnityEngine;

public class InventoryAggregate : IAggregate
{
    // === 런타임 데이터 ===

    private Dictionary<string, ShopInteriorInventory> shopInteriorInventory;
    private Dictionary<string, RoomInteriorInventory> roomInteriorInventory;
    private Dictionary<string, TileInteriorInventory> tileInventory;

    private Dictionary<int, Dictionary<string, MaterialInventory>> materialInventory;
    private Dictionary<int, Dictionary<string, SnackInventory>> snackInventory;
    private Dictionary<int, Dictionary<string, BlanketInventory>> blanketInventory;
    private Dictionary<string, ToolInventory> toolInventory;

    // === SO 데이터 ===

    private Dictionary<string, ShopInteriorItemSO> shopInteriorSOs;
    private Dictionary<string, RoomInteriorItemSO> roomInteriorSOs;
    private Dictionary<string, TileInteriorItemSO> tileInteriorSOs;
    private Dictionary<string, MaterialItemSO> materialSOs;
    private Dictionary<string, SnackItemSO> snackSOs;
    private Dictionary<string, BlanketItemSO> blanketSOs;
    private Dictionary<string, ToolItemSO> toolSOs;

    // === 변경 사항 저장소 ===

    private Dictionary<string, SaveOperation> shopInteriorInventoryChanges = new();
    private Dictionary<string, SaveOperation> roomInteriorInventoryChanges = new();
    private HashSet<(string itemName, TileInteriorType tileType)> insertedTileInventory = new();

    private Dictionary<(int inventoryID, string itemName), SaveOperation> materialInventoryChanges = new();
    private Dictionary<(int inventoryID, string itemName), SaveOperation> snackInventoryChagnes = new();
    private Dictionary<(int inventoryID, string itemName), SaveOperation> blanketInventoryChanges = new();

    private HashSet<(string toolName, ToolType toolType)> insertedToolInventory = new();


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

        shopInteriorInventoryChanges.Clear();
        roomInteriorInventoryChanges.Clear();
        insertedTileInventory.Clear();

        materialInventoryChanges.Clear();
        snackInventoryChagnes.Clear();
        blanketInventoryChanges.Clear();

        insertedToolInventory.Clear();
    }

    public IEnumerable<SavePayload> ToSavePayloads()
    {
        if (!IsDirty)
            yield break;

        // 가게 인테리어 인벤토리
        foreach (var (itemName, change) in shopInteriorInventoryChanges)
        {
            switch (change)
            {
                case SaveOperation.INSERT:
                    ShopInteriorInventory insertInven = shopInteriorInventory[itemName];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.INSERT,
                        Table = "ShopInteriorInventory",
                        Values = new Dictionary<string, object>
                        {
                            { "itemName", insertInven.itemName },
                            { "shopInteriorType", insertInven.shopInteriorType },
                            { "count", insertInven.count }
                        }
                    };

                    break;

                case SaveOperation.UPDATE:
                    ShopInteriorInventory updateInven = shopInteriorInventory[itemName];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.UPDATE,
                        Table = "ShopInteriorInventory",
                        Values = new Dictionary<string, object>
                        {
                            { "shopInteriorType", updateInven.shopInteriorType },
                            { "count", updateInven.count }
                        },
                        Conditions = new Dictionary<string, object>
                        {
                            { "itemName", updateInven.itemName }
                        }
                    };

                    break;

                case SaveOperation.DELETE:
                    yield return new SavePayload
                    {
                        Operation = SaveOperation.DELETE,
                        Table = "ShopInteriorInventory",
                        Conditions = new Dictionary<string, object>
                        {
                            { "itemName", itemName }
                        }
                    };

                    break;
            }
        }

        // 작업실 인테리어 인벤토리
        foreach (var (itemName, change) in roomInteriorInventoryChanges)
        {
            switch (change)
            {
                case SaveOperation.INSERT:
                    RoomInteriorInventory insertInven = roomInteriorInventory[itemName];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.INSERT,
                        Table = "RoomInteriorInventory",
                        Values = new Dictionary<string, object>
                        {
                            { "itemName", insertInven.itemName },
                            { "roomInteriorType", insertInven.roomInteriorType },
                            { "count", insertInven.count }
                        }
                    };

                    break;

                case SaveOperation.UPDATE:
                    RoomInteriorInventory updateInven = roomInteriorInventory[itemName];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.UPDATE,
                        Table = "RoomInteriorInventory",
                        Values = new Dictionary<string, object>
                        {
                            { "roomInteriorType", updateInven.roomInteriorType },
                            { "count", updateInven.count }
                        },
                        Conditions = new Dictionary<string, object>
                        {
                            { "itemName", updateInven.itemName }
                        }
                    };

                    break;

                case SaveOperation.DELETE:
                    yield return new SavePayload
                    {
                        Operation = SaveOperation.DELETE,
                        Table = "RoomInteriorInventory",
                        Conditions = new Dictionary<string, object>
                        {
                            { "itemName", itemName }
                        }
                    };

                    break;
            }
        }

        // 타일 인벤토리
        foreach (var (itemName, tileType) in insertedTileInventory)
        {
            yield return new SavePayload
            {
                Operation = SaveOperation.INSERT,
                Table = "TileInteriorInventory",
                Values = new Dictionary<string, object>
                {
                    { "itemName", itemName },
                    { "tileType", tileType }
                }
            };
        }

        // 재료 인벤토리
        foreach (var ((inventoryID, itemName), change) in materialInventoryChanges)
        {
            switch (change)
            {
                case SaveOperation.INSERT:
                    MaterialInventory insertInven = materialInventory[inventoryID][itemName];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.INSERT,
                        Table = "MaterialInventory",
                        Values = new Dictionary<string, object>
                        {
                            { "inventoryID", insertInven.inventoryID },
                            { "itemName", insertInven.itemName },
                            { "count", insertInven.count }
                        }
                    };

                    break;

                case SaveOperation.UPDATE:
                    MaterialInventory updateInven = materialInventory[inventoryID][itemName];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.UPDATE,
                        Table = "MaterialInventory",
                        Values = new Dictionary<string, object>
                        {
                            { "count", updateInven.count }
                        },
                        Conditions = new Dictionary<string, object>
                        {
                            { "inventoryID", updateInven.inventoryID },
                            { "itemName", updateInven.itemName }
                        }
                    };

                    break;

                case SaveOperation.DELETE:
                    yield return new SavePayload
                    {
                        Operation = SaveOperation.DELETE,
                        Table = "MaterialInventory",
                        Conditions = new Dictionary<string, object>
                        {
                            { "inventoryID", inventoryID },
                            { "itemName", itemName }
                        }
                    };

                    break;
            }
        }

        // 간식 인벤토리
        foreach (var ((inventoryID, itemName), change) in snackInventoryChagnes)
        {
            switch (change)
            {
                case SaveOperation.INSERT:
                    SnackInventory insertInven = snackInventory[inventoryID][itemName];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.INSERT,
                        Table = "SnackInventory",
                        Values = new Dictionary<string, object>
                        {
                            { "inventoryID", insertInven.inventoryID },
                            { "itemName", insertInven.itemName },
                            { "count", insertInven.count }
                        }
                    };

                    break;

                case SaveOperation.UPDATE:
                    SnackInventory updateInven = snackInventory[inventoryID][itemName];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.UPDATE,
                        Table = "SnackInventory",
                        Values = new Dictionary<string, object>
                        {
                            { "count", updateInven.count }
                        },
                        Conditions = new Dictionary<string, object>
                        {
                            { "inventoryID", updateInven.inventoryID },
                            { "itemName", updateInven.itemName }
                        }
                    };

                    break;

                case SaveOperation.DELETE:
                    yield return new SavePayload
                    {
                        Operation = SaveOperation.DELETE,
                        Table = "SnackInventory",
                        Conditions = new Dictionary<string, object>
                        {
                            { "inventoryID", inventoryID },
                            { "itemName", itemName }
                        }
                    };

                    break;
            }
        }

        // 이불 인벤토리
        foreach (var ((inventoryID, itemName), change) in blanketInventoryChanges)
        {
            switch (change)
            {
                case SaveOperation.INSERT:
                    BlanketInventory insertInven = blanketInventory[inventoryID][itemName];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.INSERT,
                        Table = "BlanketInventory",
                        Values = new Dictionary<string, object>
                        {
                            { "inventoryID", insertInven.inventoryID },
                            { "itemName", insertInven.itemName },
                            { "count", insertInven.count }
                        }
                    };

                    break;

                case SaveOperation.UPDATE:
                    BlanketInventory updateInven = blanketInventory[inventoryID][itemName];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.UPDATE,
                        Table = "BlanketInventory",
                        Values = new Dictionary<string, object>
                        {
                            { "count", updateInven.count }
                        },
                        Conditions = new Dictionary<string, object>
                        {
                            { "inventoryID", updateInven.inventoryID },
                            { "itemName", updateInven.itemName }
                        }
                    };

                    break;

                case SaveOperation.DELETE:
                    yield return new SavePayload
                    {
                        Operation = SaveOperation.DELETE,
                        Table = "BlanketInventory",
                        Conditions = new Dictionary<string, object>
                        {
                            { "inventoryID", inventoryID },
                            { "itemName", itemName }
                        }
                    };

                    break;
            }
        }

        // 도구 인벤토리
        foreach (var (toolName, toolType) in insertedToolInventory)
        {
            yield return new SavePayload
            {
                Operation = SaveOperation.INSERT,
                Table = "ToolInventory",
                Values = new Dictionary<string, object>
                {
                    { "toolName", toolName },
                    { "toolType", toolType }
                }
            };
        }
    }

    public void LoadInventoryAggregate(IEnumerable<ShopInteriorInventory> shopInteriorInventory, IEnumerable<RoomInteriorInventory> roomInteriorInventory,
        IEnumerable<TileInteriorInventory> tileInventory, IEnumerable<MaterialInventory> materialInventory,
        IEnumerable<SnackInventory> snackInventory, IEnumerable<BlanketInventory> blanketInventory, IEnumerable<ToolInventory> toolInventory,
        Dictionary<string, ShopInteriorItemSO> shopInteriorSOs, Dictionary<string, RoomInteriorItemSO> roomInteriorSOs,
        Dictionary<string, TileInteriorItemSO> tileInteriorSOs, Dictionary<string, MaterialItemSO> materialSOs,
        Dictionary<string, SnackItemSO> snackSOs, Dictionary<string, BlanketItemSO> blanketSOs, Dictionary<string, ToolItemSO> toolSOs)
    {
        this.shopInteriorInventory = shopInteriorInventory.ToDictionary(sii => sii.itemName);
        this.roomInteriorInventory = roomInteriorInventory.ToDictionary(rii => rii.itemName);
        this.tileInventory = tileInventory.ToDictionary(ti => ti.itemName);

        this.materialInventory = materialInventory
        .GroupBy(mi => mi.inventoryID)
        .ToDictionary(
            g => g.Key,
            g => g.ToDictionary(mi => mi.itemName)
        );
        this.snackInventory = snackInventory
        .GroupBy(si => si.inventoryID)
        .ToDictionary(
            g => g.Key,
            g => g.ToDictionary(si => si.itemName)
        );
        this.blanketInventory = blanketInventory
        .GroupBy(bi => bi.inventoryID)
        .ToDictionary(
            g => g.Key,
            g => g.ToDictionary(bi => bi.itemName)
        );

        this.toolInventory = toolInventory.ToDictionary(ti => ti.toolName);

        this.shopInteriorSOs = shopInteriorSOs;
        this.roomInteriorSOs = roomInteriorSOs;
        this.tileInteriorSOs = tileInteriorSOs;
        this.materialSOs = materialSOs;
        this.snackSOs = snackSOs;
        this.blanketSOs = blanketSOs;
        this.toolSOs = toolSOs;
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


    public ShopInteriorItemSO GetShopInteriorItemSO(string itemName)
    {
        if (shopInteriorSOs.TryGetValue(itemName, out var item)) return item;
        else return null;
    }

    public RoomInteriorItemSO GetRoomInteriorItemSO(string itemName)
    {
        if (roomInteriorSOs.TryGetValue(itemName, out var item)) return item;
        else return null;
    }

    public TileInteriorItemSO GetTileInteriorItemSO(string itemName)
    {
        if (tileInteriorSOs.TryGetValue(itemName, out var item)) return item;
        else return null;
    }

    public MaterialItemSO GetMaterialItemSO(string itemName)
    {
        if (materialSOs.TryGetValue(itemName, out var item)) return item;
        else return null;
    }

    public SnackItemSO GetSnackItemSO(string itemName)
    {
        if (snackSOs.TryGetValue(itemName, out var item)) return item;
        else return null;
    }

    public BlanketItemSO GetBlanketItemSO(string itemName)
    {
        if (blanketSOs.TryGetValue(itemName, out var item)) return item;
        else return null;
    }

    public ToolItemSO GetToolItemSO(string itemName)
    {
        if (toolSOs.TryGetValue(itemName, out var item)) return item;
        else return null;
    }



    public void AdjustMaterialCount(int inventoryID, string itemName, int amount)
    {
        if (amount == 0) return;

        if (!materialInventory.TryGetValue(inventoryID, out var dict))
        {
            dict = new Dictionary<string, MaterialInventory>();
            materialInventory[inventoryID] = dict;
        }

        if (dict.TryGetValue(itemName, out var inven))
        {
            inven.count += amount;

            if (inven.count <= 0)
            {
                dict.Remove(itemName);

                MergeChange(materialInventoryChanges,
                (inventoryID, itemName),
                SaveOperation.DELETE);
            }
            else
            {
                MergeChange(materialInventoryChanges,
                (inventoryID, itemName),
                SaveOperation.UPDATE);
            }

        }
        else
        {
            if (amount < 0) return;

            dict[itemName] = new MaterialInventory
            {
                inventoryID = inventoryID,
                itemName = itemName,
                count = amount
            };

            MergeChange(materialInventoryChanges,
                (inventoryID, itemName),
                SaveOperation.INSERT);
        }

        MarkDirty();
    }

    public List<MaterialInventory> GetMaterialItems(int targetInventoryID)
    {
        if (!materialInventory.ContainsKey(targetInventoryID)) return null;

        return materialInventory[targetInventoryID].Values.ToList();
    }

    public void AdjustSnackCount(int inventoryID, string itemName, int amount)
    {
        if (amount == 0) return;

        if (!snackInventory.TryGetValue(inventoryID, out var dict))
        {
            dict = new Dictionary<string, SnackInventory>();
            snackInventory[inventoryID] = dict;
        }

        if (dict.TryGetValue(itemName, out var inven))
        {
            inven.count += amount;

            if (inven.count <= 0)
            {
                dict.Remove(itemName);

                MergeChange(snackInventoryChagnes,
                (inventoryID, itemName),
                SaveOperation.DELETE);
            }
            else
            {
                MergeChange(snackInventoryChagnes,
                (inventoryID, itemName),
                SaveOperation.UPDATE);
            }
        }
        else
        {
            if (amount < 0) return;

            dict[itemName] = new SnackInventory
            {
                inventoryID = inventoryID,
                itemName = itemName,
                count = amount
            };

            MergeChange(snackInventoryChagnes,
                (inventoryID, itemName),
                SaveOperation.INSERT);
        }

        MarkDirty();
    }

    public List<SnackInventory> GetSnackItems(int targetInventoryID)
    {
        if (!snackInventory.ContainsKey(targetInventoryID)) return null;

        return snackInventory[targetInventoryID].Values.ToList();
    }

    public void AdjustBlanketCount(int inventoryID, string itemName, int amount)
    {
        if (amount == 0) return;

        if (!blanketInventory.TryGetValue(inventoryID, out var dict))
        {
            dict = new Dictionary<string, BlanketInventory>();
            blanketInventory[inventoryID] = dict;
        }

        if (dict.TryGetValue(itemName, out var inven))
        {
            inven.count += amount;

            if (inven.count <= 0)
            {
                dict.Remove(itemName);

                MergeChange(blanketInventoryChanges,
                (inventoryID, itemName),
                SaveOperation.DELETE);
            }
            else
            {
                MergeChange(blanketInventoryChanges,
                (inventoryID, itemName),
                SaveOperation.UPDATE);
            }
        }
        else
        {
            if (amount < 0) return;

            dict[itemName] = new BlanketInventory
            {
                inventoryID = inventoryID,
                itemName = itemName,
                count = amount
            };

            MergeChange(blanketInventoryChanges,
                (inventoryID, itemName),
                SaveOperation.INSERT);
        }

        MarkDirty();
    }

    public List<BlanketInventory> GetBlanketsInBox(int targetinventoryID)
    {
        if (!blanketInventory.ContainsKey(targetinventoryID)) return null;

        return blanketInventory[targetinventoryID].Values.ToList();
    }

    public List<StorageClass> GetCurrentRoomBlanketBoxData()
    {
        List<StorageClass> list = new List<StorageClass>();

        List<(RoomInteriorItemSO boxSO, int ID)> boxData = ServiceLocator.Get<GameData>().Interior.GetCurrentRoomBlanketBoxData();

        foreach (var box in boxData)
        {
            StorageClass storeClass = new StorageClass();
            storeClass.storageID = box.ID;
            storeClass.max = box.boxSO.slotCount;

            storeClass.count = 0;
            if (blanketInventory.TryGetValue(box.ID, out var dict))
            {
                foreach (var (itemName, inven) in dict)
                {
                    storeClass.count += inven.count;
                }
            }

            list.Add(storeClass);
        }

        return list;
    }


    public bool AddMaterialFromEntire(string itemName, int count)
    {
        var boxData = ServiceLocator.Get<GameData>().Interior.GetCurrentRoomMaterialBoxData();

        // 각 박스에 얼마를 넣을지 임시 저장
        Dictionary<int, int> plan = new();

        int remainCount = count;

        // 수용 가능 여부 계산
        foreach (var box in boxData)
        {
            int current = 0;

            if (materialInventory.TryGetValue(box.ID, out var dict))
            {
                foreach (var item in dict.Values)
                {
                    current += item.count;
                }
            }

            int capacity = box.boxSO.slotCount - current;
            if (capacity <= 0)
                continue;

            int toAdd = Mathf.Min(capacity, remainCount);
            // capacity가 더 커서 다 들어가는 경우, 남은 remainCount 선택돼서 다 들어감
            // ramainCount가 더 커서 다 들어가지 못하는 경우, capacity 선택돼서 남은 슬롯 개수만큼만 들어감
            plan[box.ID] = toAdd;
            remainCount -= toAdd;

            if (remainCount <= 0)
                break;
        }


        // 전부 못 넣는 경우, 아무 것도 안 넣음
        if (remainCount > 0)
            return false;


        // 실제 반영
        foreach (var (boxID, addCount) in plan)
        {
            if (!materialInventory.TryGetValue(boxID, out var dict))
            {
                dict = new Dictionary<string, MaterialInventory>();
                materialInventory[boxID] = dict;
            }

            if (dict.TryGetValue(itemName, out var inven))
            {
                inven.count += addCount;

                MergeChange(materialInventoryChanges,
                    (boxID, itemName),
                    SaveOperation.UPDATE);
            }
            else
            {
                inven = new MaterialInventory
                {
                    inventoryID = boxID,
                    itemName = itemName,
                    count = addCount,
                };
                dict[itemName] = inven;

                MergeChange(materialInventoryChanges,
                    (boxID, itemName),
                    SaveOperation.INSERT);
            }
        }

        MarkDirty();

        return true;
    }

    public bool AddSnackFromEntire(string itemName, int count)
    {
        var boxData = ServiceLocator.Get<GameData>().Interior.GetCurrentRoomSnackBoxData();

        // 각 박스에 얼마를 넣을지 임시 저장
        Dictionary<int, int> plan = new();

        int remainCount = count;

        // 수용 가능 여부 계산
        foreach (var box in boxData)
        {
            int current = 0;

            if (snackInventory.TryGetValue(box.ID, out var dict))
            {
                foreach (var item in dict.Values)
                {
                    current += item.count;
                }
            }

            int capacity = box.boxSO.slotCount - current;
            if (capacity <= 0)
                continue;

            int toAdd = Mathf.Min(capacity, remainCount);
            // capacity가 더 커서 다 들어가는 경우, 남은 remainCount 선택돼서 다 들어감
            // ramainCount가 더 커서 다 들어가지 못하는 경우, capacity 선택돼서 남은 슬롯 개수만큼만 들어감
            plan[box.ID] = toAdd;
            remainCount -= toAdd;

            if (remainCount <= 0)
                break;
        }


        // 전부 못 넣는 경우, 아무 것도 안 넣음
        if (remainCount > 0)
            return false;


        // 실제 반영
        foreach (var (boxID, addCount) in plan)
        {
            if (!snackInventory.TryGetValue(boxID, out var dict))
            {
                dict = new Dictionary<string, SnackInventory>();
                snackInventory[boxID] = dict;
            }

            if (dict.TryGetValue(itemName, out var inven))
            {
                inven.count += addCount;

                MergeChange(snackInventoryChagnes,
                    (boxID, itemName),
                    SaveOperation.UPDATE);
            }
            else
            {
                inven = new SnackInventory
                {
                    inventoryID = boxID,
                    itemName = itemName,
                    count = addCount,
                };
                dict[itemName] = inven;

                MergeChange(snackInventoryChagnes,
                    (boxID, itemName),
                    SaveOperation.INSERT);
            }
        }

        MarkDirty();

        return true;
    }

    public bool AddBlanketFromEntire(string itemName, int count)
    {
        var boxData = ServiceLocator.Get<GameData>().Interior.GetCurrentRoomBlanketBoxData();

        // 각 박스에 얼마를 넣을지 임시 저장
        Dictionary<int, int> plan = new();

        int remainCount = count;

        // 수용 가능 여부 계산
        foreach (var box in boxData)
        {
            int current = 0;

            if (blanketInventory.TryGetValue(box.ID, out var dict))
            {
                foreach (var item in dict.Values)
                {
                    current += item.count;
                }
            }

            int capacity = box.boxSO.slotCount - current;
            if (capacity <= 0)
                continue;

            int toAdd = Mathf.Min(capacity, remainCount);
            // capacity가 더 커서 다 들어가는 경우, 남은 remainCount 선택돼서 다 들어감
            // ramainCount가 더 커서 다 들어가지 못하는 경우, capacity 선택돼서 남은 슬롯 개수만큼만 들어감
            plan[box.ID] = toAdd;
            remainCount -= toAdd;

            if (remainCount <= 0)
                break;
        }


        // 전부 못 넣는 경우, 아무 것도 안 넣음
        if (remainCount > 0)
            return false;


        // 실제 반영
        foreach (var (boxID, addCount) in plan)
        {
            if (!blanketInventory.TryGetValue(boxID, out var dict))
            {
                dict = new Dictionary<string, BlanketInventory>();
                blanketInventory[boxID] = dict;
            }

            if (dict.TryGetValue(itemName, out var inven))
            {
                inven.count += addCount;

                MergeChange(blanketInventoryChanges,
                    (boxID, itemName),
                    SaveOperation.UPDATE);
            }
            else
            {
                inven = new BlanketInventory
                {
                    inventoryID = boxID,
                    itemName = itemName,
                    count = addCount,
                };
                dict[itemName] = inven;

                MergeChange(blanketInventoryChanges,
                    (boxID, itemName),
                    SaveOperation.INSERT);
            }
        }

        MarkDirty();

        return true;
    }


    public bool AddToolItem(string itemName)
    {
        if (toolInventory.ContainsKey(itemName)) return false;

        ToolInventory tool = new ToolInventory();
        tool.toolName = itemName;
        tool.toolType = toolSOs[itemName].toolType;
        toolInventory.Add(itemName, tool);

        insertedToolInventory.Add((itemName, tool.toolType));
        MarkDirty();

        return true;
    }

    public bool AddShopInteriorItem(string itemName, int count)
    {
        int currentCount = 0;
        foreach (var (name, item) in shopInteriorInventory)
        {
            currentCount += item.count;
        }

        if (ServiceLocator.Get<GameData>().User.GetInteriorInventoryLevel().invenCount < (currentCount + count))
            return false;


        bool isInsert = false;
        if (!shopInteriorInventory.TryGetValue(itemName, out var inven))
        {
            inven = new ShopInteriorInventory
            {
                itemName = itemName,
                count = 0
            };
            shopInteriorInventory[itemName] = inven;
            isInsert = true;
        }

        inven.count += count;

        MergeChange(shopInteriorInventoryChanges,
            itemName,
            isInsert ? SaveOperation.INSERT : SaveOperation.UPDATE);

        MarkDirty();

        return true;
    }

    public bool AddRoomInteriorItem(string itemName, int count)
    {
        if (count <= 0)
            return false;

        int currentCount = 0;
        foreach (var item in roomInteriorInventory.Values)
        {
            currentCount += item.count;
        }

        int maxCount = ServiceLocator.Get<GameData>().User.GetInteriorInventoryLevel().invenCount;

        if (maxCount < currentCount + count)
            return false;


        bool isInsert = false;
        if (!roomInteriorInventory.TryGetValue(itemName, out var inven))
        {
            inven = new RoomInteriorInventory
            {
                itemName = itemName,
                count = 0
            };
            roomInteriorInventory[itemName] = inven;
            isInsert = true;
        }

        inven.count += count;

        MergeChange(shopInteriorInventoryChanges,
            itemName,
            isInsert ? SaveOperation.INSERT : SaveOperation.UPDATE);

        MarkDirty();

        return true;
    }


    public bool AddTileInteriorItem(string itemName)
    {
        if (tileInventory.ContainsKey(itemName)) return false;

        TileInteriorInventory tile = new TileInteriorInventory();
        tile.itemName = itemName;
        tile.tileType = tileInteriorSOs[itemName].tileType;
        tileInventory.Add(itemName, tile);

        insertedTileInventory.Add((itemName, tile.tileType));
        MarkDirty();

        return true;
    }

}
