using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InventoryAggregate : IAggregate
{
    private Dictionary<string, ShopInteriorInventory> shopInteriorInventory;
    private Dictionary<string, RoomInteriorInventory> roomInteriorInventory;
    private Dictionary<TileInteriorType, Dictionary<string, TileInteriorInventory>> tileInventory;

    private Dictionary<int, Dictionary<string, MaterialInventory>> materialInventory;
    private Dictionary<int, Dictionary<string, SnackInventory>> snackInventory;
    private Dictionary<int, Dictionary<string, BlanketInventory>> blanketInventory;

    private Dictionary<ToolType, Dictionary<string, ToolInventory>> toolInventory;




    public bool IsDirty { get; private set; }

    private void MarkDirty()
    {
        IsDirty = true;
        ServiceLocator.Get<DirtyDataRegistry>().RegisterDirty(this);
    }

    public void ClearDirty()
    {
        IsDirty = false;


    }

    public IEnumerable<SavePayload> ToSavePayloads()
    {

    }

    public void LoadInventoryAggregate(IEnumerable<ShopInteriorInventory> shopInteriorInventory, IEnumerable<RoomInteriorInventory> roomInteriorInventory,
        IEnumerable<TileInteriorInventory> tileInventory, IEnumerable<MaterialInventory> materialInventory,
        IEnumerable<SnackInventory> snackInventory, IEnumerable<BlanketInventory> blanketInventory, IEnumerable<ToolInventory> toolInventory)
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
    }

    // === 게임 플레이 메서드 ===

}
