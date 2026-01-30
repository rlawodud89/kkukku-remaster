using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryAggregate : IAggregate
{
    // === 런타임 데이터 ===

    private Dictionary<string, ShopInteriorInventory> shopInteriorInventory;
    private Dictionary<string, RoomInteriorInventory> roomInteriorInventory;
    private Dictionary<TileInteriorType, Dictionary<string, TileInteriorInventory>> tileInventory;

    private Dictionary<int, Dictionary<string, MaterialInventory>> materialInventory;
    private Dictionary<int, Dictionary<string, SnackInventory>> snackInventory;
    private Dictionary<int, Dictionary<string, BlanketInventory>> blanketInventory;

    private Dictionary<ToolType, Dictionary<string, ToolInventory>> toolInventory;

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
    private HashSet<(TileInteriorType tileType, string itemName)> insertedTileInventory = new();

    private Dictionary<(int inventoryID, string itemName), SaveOperation> materialInventoryChanges = new();
    private Dictionary<(int inventoryID, string itemName), SaveOperation> snackInventoryChagnes = new();
    private Dictionary<(int inventoryID, string itemName), SaveOperation> blanketInventoryChanges = new();

    private HashSet<(ToolType toolType, string toolName)> insertedToolInventory = new();


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
        foreach (var (tileType, itemName) in insertedTileInventory)
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
        foreach (var (toolType, toolName) in insertedToolInventory)
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
        this.tileInventory = tileInventory
        .GroupBy(ti => ti.tileType)
        .ToDictionary(
            g => g.Key,
            g => g.ToDictionary(ti => ti.itemName)
        );

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

        this.toolInventory = toolInventory
        .GroupBy(ti => ti.toolType)
        .ToDictionary(
            g => g.Key,
            g => g.ToDictionary(ti => ti.toolName)
        );

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

}
