using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class InteriorAggregate : IAggregate
{
    // === 런타임 데이터 ===

    private Dictionary<int, ShopInteriorPlaced> shopPlaced; // Key: ID
    private Dictionary<int, RoomInteriorPlaced> roomPlaced; // Key: ID
    private Dictionary<TilePositionType, TileInteriorPlaced> tilePlaced; // Key: tilePositionType

    // === SO 데이터 ===

    private Dictionary<string, ShopInteriorItemSO> shopInteriorSOs;
    private Dictionary<string, RoomInteriorItemSO> roomInteriorSOs;
    private Dictionary<string, TileInteriorItemSO> tileInteriorSOs;

    // === 변경 사항 저장소 ===

    private Dictionary<int, SaveOperation> shopPlacedChanges = new();
    private Dictionary<int, SaveOperation> roomPlacedChanges = new();
    private HashSet<TilePositionType> updatedTilePlaced = new();

    // === 기타 데이터 ===

    private IDPool shopInteriorIDPool = new();
    private IDPool roomInteriorIDPool = new();


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

        shopPlacedChanges.Clear();
        roomPlacedChanges.Clear();
        updatedTilePlaced.Clear();
    }

    public IEnumerable<SavePayload> ToSavePayloads()
    {
        if (!IsDirty)
            yield break;

        // 가게 인테리어
        foreach (var (ID, change) in shopPlacedChanges)
        {
            switch (change)
            {
                case SaveOperation.INSERT:
                    ShopInteriorPlaced insertInterior = shopPlaced[ID];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.INSERT,
                        Table = "ShopInteriorPlaced",
                        Values = new Dictionary<string, object>
                        {
                            { "ID", insertInterior.ID },
                            { "gridNumber", insertInterior.gridNumber },
                            { "itemName", insertInterior.itemName },
                            { "interiorType", insertInterior.interiorType },
                        }
                    };

                    break;

                case SaveOperation.UPDATE:
                    ShopInteriorPlaced updateInterior = shopPlaced[ID];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.UPDATE,
                        Table = "ShopInteriorPlaced",
                        Values = new Dictionary<string, object>
                        {
                            { "gridNumber", updateInterior.gridNumber },
                            { "itemName", updateInterior.itemName },
                            { "interiorType", updateInterior.interiorType },

                        },
                        Conditions = new Dictionary<string, object>
                        {
                            { "ID", updateInterior.ID }
                        }
                    };

                    break;

                case SaveOperation.DELETE:
                    yield return new SavePayload
                    {
                        Operation = SaveOperation.DELETE,
                        Table = "ShopInteriorPlaced",
                        Conditions = new Dictionary<string, object>
                        {
                            { "ID", ID }
                        }
                    };

                    break;
            }
        }

        // 작업실 인테리어
        foreach (var (ID, change) in roomPlacedChanges)
        {
            switch (change)
            {
                case SaveOperation.INSERT:
                    RoomInteriorPlaced insertInterior = roomPlaced[ID];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.INSERT,
                        Table = "RoomInteriorPlaced",
                        Values = new Dictionary<string, object>
                        {
                            { "ID", insertInterior.ID },
                            { "gridNumber", insertInterior.gridNumber },
                            { "itemName", insertInterior.itemName },
                            { "interiorType", insertInterior.interiorType }
                        }
                    };

                    break;

                case SaveOperation.UPDATE:
                    RoomInteriorPlaced updateInterior = roomPlaced[ID];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.UPDATE,
                        Table = "RoomInteriorPlaced",
                        Values = new Dictionary<string, object>
                        {
                            { "gridNumber", updateInterior.gridNumber },
                            { "itemName", updateInterior.itemName },
                            { "interiorType", updateInterior.interiorType }
                        },
                        Conditions = new Dictionary<string, object>
                        {
                            { "ID", updateInterior.ID }
                        }
                    };

                    break;

                case SaveOperation.DELETE:
                    yield return new SavePayload
                    {
                        Operation = SaveOperation.DELETE,
                        Table = "RoomInteriorPlaced",
                        Conditions = new Dictionary<string, object>
                        {
                            { "ID", ID }
                        }
                    };

                    break;
            }
        }

        // 타일 변경
        foreach (var tilePositionType in updatedTilePlaced)
        {
            TileInteriorPlaced tile = tilePlaced[tilePositionType];

            yield return new SavePayload
            {
                Operation = SaveOperation.UPDATE,
                Table = "TileInteriorPlaced",
                Values = new Dictionary<string, object>
                {
                    { "itemName", tile.itemName }
                },
                Conditions = new Dictionary<string, object>
                {
                    { "tilePosition", tile.tilePosition }
                }
            };
        }
    }

    public void LoadInteriorAggregate(IEnumerable<ShopInteriorPlaced> shopPlaced, IEnumerable<RoomInteriorPlaced> roomPlaced,
        IEnumerable<TileInteriorPlaced> tilePlaced, Dictionary<string, ShopInteriorItemSO> shopInteriorSOs,
        Dictionary<string, RoomInteriorItemSO> roomInteriorSOs, Dictionary<string, TileInteriorItemSO> tileInteriorSOs)
    {
        this.shopPlaced = shopPlaced.ToDictionary(sp => sp.ID);
        this.roomPlaced = roomPlaced.ToDictionary(rp => rp.ID);
        this.tilePlaced = tilePlaced.ToDictionary(tp => tp.tilePosition);

        this.shopInteriorSOs = shopInteriorSOs;
        this.roomInteriorSOs = roomInteriorSOs;
        this.tileInteriorSOs = tileInteriorSOs;

        shopInteriorIDPool.InitializeFromExisting(this.shopPlaced.Keys);
        roomInteriorIDPool.InitializeFromExisting(this.roomPlaced.Keys);
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


    public RoomInteriorItemSO GetRoomInteriorInRoom(int ID)
    {
        RoomInteriorPlaced placed = roomPlaced.Values.FirstOrDefault(i => i.ID == ID);

        if (placed == null) return null;
        else return roomInteriorSOs[placed.itemName];
    }

    public List<ShopInteriorPlaced> GetCurrentShopInterior()
    {
        return shopPlaced.Values.ToList();
    }

    public List<RoomInteriorPlaced> GetCurrentRoomInterior()
    {
        return roomPlaced.Values.ToList();
    }

    public List<(RoomInteriorItemSO boxSO, int ID)> GetCurrentRoomBlanketBoxData()
    {
        List<RoomInteriorPlaced> roomBlanketBoxes = roomPlaced.Values
            .Where(i => i.interiorType == RoomInteriorType.BLANKET_BOX)
            .ToList();

        List<(RoomInteriorItemSO boxSO, int ID)> list = new();
        foreach (var box in roomBlanketBoxes)
        {
            list.Add((roomInteriorSOs[box.itemName], box.ID));
        }

        return list;
    }

    public List<(ShopInteriorItemSO tableSO, int ID)> GetCurrentShopTableData()
    {
        List<ShopInteriorPlaced> shopTables = shopPlaced.Values
            .Where(i => i.interiorType == ShopInteriorType.TABLE)
            .ToList();

        List<(ShopInteriorItemSO tableSO, int ID)> list = new();
        foreach (var table in shopTables)
        {
            list.Add((shopInteriorSOs[table.itemName], table.ID));
        }

        return list;
    }

    public List<(RoomInteriorItemSO boxSO, int ID)> GetCurrentRoomMaterialBoxData()
    {
        List<RoomInteriorPlaced> roomMaterialBoxes = roomPlaced.Values
            .Where(i => i.interiorType == RoomInteriorType.MATERIAL_BOX)
            .ToList();

        List<(RoomInteriorItemSO boxSO, int ID)> list = new();
        foreach (var box in roomMaterialBoxes)
        {
            list.Add((roomInteriorSOs[box.itemName], box.ID));
        }

        return list;
    }

    public List<(RoomInteriorItemSO boxSO, int ID)> GetCurrentRoomSnackBoxData()
    {
        List<RoomInteriorPlaced> roomSnackBoxes = roomPlaced.Values
            .Where(i => i.interiorType == RoomInteriorType.SNACK_BOX)
            .ToList();

        List<(RoomInteriorItemSO boxSO, int ID)> list = new();
        foreach (var box in roomSnackBoxes)
        {
            list.Add((roomInteriorSOs[box.itemName], box.ID));
        }

        return list;
    }

    public int AddShopInterior(int gridNumber, string itemName)
    {
        if (shopPlaced.Values.Any(v => v.gridNumber == gridNumber))
            return -1;

        var interiorType = shopInteriorSOs[itemName].shopInteriorType;

        var newshopInterior = new ShopInteriorPlaced
        {
            ID = shopInteriorIDPool.Generate(),
            gridNumber = gridNumber,
            itemName = itemName,
            interiorType = interiorType
        };

        shopPlaced.Add(newshopInterior.ID, newshopInterior);

        MergeChange(shopPlacedChanges,
            newshopInterior.ID,
            SaveOperation.INSERT);

        MarkDirty();

        return newshopInterior.ID;
    }

    public int AddRoomInterior(int gridNumber, string itemName)
    {
        if (roomPlaced.Values.Any(v => v.gridNumber == gridNumber))
            return -1;

        var interiorType = roomInteriorSOs[itemName].roomInteriorType;

        var newroomInterior = new RoomInteriorPlaced
        {
            ID = roomInteriorIDPool.Generate(),
            gridNumber = gridNumber,
            itemName = itemName,
            interiorType = interiorType
        };

        roomPlaced.Add(newroomInterior.ID, newroomInterior);

        MergeChange(roomPlacedChanges,
            newroomInterior.ID,
            SaveOperation.INSERT);

        MarkDirty();

        if (interiorType == RoomInteriorType.WORKER) ServiceLocator.Get<GameData>().ShopState.AddWorkerState(newroomInterior.ID);

        return newroomInterior.ID;
    }

    public void RemoveShopInterior(int targetID)
    {
        if (shopPlaced.Remove(targetID))
        {
            shopInteriorIDPool.Release(targetID);

            MergeChange(shopPlacedChanges,
                targetID,
                SaveOperation.DELETE);

            MarkDirty();
        }
    }

    public void RemoveRoomInterior(int targetID)
    {
        if (roomPlaced.TryGetValue(targetID, out var placed))
        {
            RoomInteriorItemSO interiorSO = roomInteriorSOs[placed.itemName];

            if (interiorSO.roomInteriorType == RoomInteriorType.WORKER)
                ServiceLocator.Get<GameData>().ShopState.RemoveWorkerState(targetID);

            roomPlaced.Remove(targetID);
            roomInteriorIDPool.Release(targetID);

            MergeChange(roomPlacedChanges,
                targetID,
                SaveOperation.DELETE);

            MarkDirty();
        }
    }

    public void TransferShopInterior(int targetID, int newGirdNumber)
    {
        if (shopPlaced.TryGetValue(targetID, out var shopInterior))
        {
            shopInterior.gridNumber = newGirdNumber;

            MergeChange(shopPlacedChanges,
                targetID,
                SaveOperation.UPDATE);

            MarkDirty();
        }
    }

    public void TransferRoomInterior(int targetID, int newGirdNumber)
    {
        if (roomPlaced.TryGetValue(targetID, out var roomInterior))
        {
            roomInterior.gridNumber = newGirdNumber;

            MergeChange(roomPlacedChanges,
                targetID,
                SaveOperation.UPDATE);

            MarkDirty();
        }
    }

    public TileInteriorItemSO GetCurrentTileInterior(TilePositionType tilePositionType)
    {
        return tileInteriorSOs[tilePlaced[tilePositionType].itemName];
    }

    public FloorItem GetCurrentFloorTile(TilePositionType tilePositionType)
    {
        var tileSO = tileInteriorSOs[tilePlaced[tilePositionType].itemName];

        if (tileSO.tileType != TileInteriorType.FLOOR)
            return null;

        return new FloorItem
        {
            itemName = tileSO.itemName,
            itemImage = tileSO.image,
            tileBase = tileSO.tileBase
        };
    }

    public WallpaperItem GetCurrentWallTile(TilePositionType tilePositionType)
    {
        var tileSO = tileInteriorSOs[tilePlaced[tilePositionType].itemName];

        if (tileSO.tileType != TileInteriorType.WALL)
            return null;

        WallpaperItem wallpaperItem = new WallpaperItem();
        wallpaperItem.itemName = tileSO.itemName;
        wallpaperItem.itemImage = tileSO.image;
        wallpaperItem.wallTiles[0] = tileSO.tileBase;
        wallpaperItem.wallTiles[1] = tileSO.topTileBase;
        wallpaperItem.wallTiles[2] = tileSO.bottomTileBase;

        return wallpaperItem;
    }

    public void SetTileInterior(TilePositionType tilePositionType, string itemName)
    {
        tilePlaced[tilePositionType].itemName = itemName;

        updatedTilePlaced.Add(tilePositionType);

        MarkDirty();
    }

    public List<(int tableID, int maxCount)> GetCurrentTableMaxCountList()
    {
        var list = new List<(int tableID, int maxCount)>();

        foreach (var (ID, interior) in shopPlaced)
        {
            if(interior.interiorType == ShopInteriorType.TABLE)
            {
                list.Add((
                    ID,
                    shopInteriorSOs[interior.itemName].slotCount
                    ));
            }
        }

        return list;
    }
}