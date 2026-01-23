using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

public class InteriorAggregate : IAggregate
{
    // 현재는 x,y 좌표 float, 그리드 칸 번호 int 기반으로 바꾸면 변경 필요

    private Dictionary<(float x, float y), ShopInteriorPlaced> shopPlaced;
    private Dictionary<(float x, float y), RoomInteriorPlaced> roomPlaced;
    private Dictionary<TilePositionType, TileInteriorPlaced> tilePlaced;


    private HashSet<(float x, float y)> insertedShopPlaced = new();
    private HashSet<(float x, float y)> updatedShopPlaced = new();
    private HashSet<(float x, float y)> deletedShopPlaced = new();

    private HashSet<(float x, float y)> insertedRoomPlaced = new();
    private HashSet<(float x, float y)> updatedRoomPlaced = new();
    private HashSet<(float x, float y)> deletedRoomPlaced = new();

    private HashSet<TilePositionType> updatedTilePlaced = new();


    public bool IsDirty { get; private set; }

    private void MarkDirty()
    {
        IsDirty = true;
        ServiceLocator.Get<DirtyDataRegistry>().RegisterDirty(this);
    }

    public void ClearDirty()
    {
        IsDirty = false;

        insertedShopPlaced.Clear();
        updatedShopPlaced.Clear();
        deletedShopPlaced.Clear();

        insertedRoomPlaced.Clear();
        updatedRoomPlaced.Clear();
        deletedRoomPlaced.Clear();

        updatedTilePlaced.Clear();
    }

    public IEnumerable<SavePayload> ToSavePayloads()
    {
        if (!IsDirty)
            yield break;

        // 가게 인테리어
        foreach (var isp in insertedShopPlaced)
        {
            ShopInteriorPlaced interior = shopPlaced[isp];

            yield return new SavePayload
            {
                Operation = SaveOperation.INSERT,
                Table = "ShopInteriorPlaced",
                Values = new Dictionary<string, object>
                {
                    { "itemName", interior.itemName },
                    { "interiorType", interior.interiorType },
                    { "x", interior.x },
                    { "y", interior.y },
                    { "ID", interior.ID }
                }
            };
        }
        foreach (var usp in updatedShopPlaced)
        {
            ShopInteriorPlaced interior = shopPlaced[usp];

            yield return new SavePayload
            {
                Operation = SaveOperation.UPDATE,
                Table = "ShopInteriorPlaced",
                Values = new Dictionary<string, object>
                {
                    { "itemName", interior.itemName },
                    { "interiorType", interior.interiorType },
                    { "ID", interior.ID }
                },
                Conditions = new Dictionary<string, object>
                {
                    { "x", interior.x },
                    { "y", interior.y }
                }
            };
        }
        foreach (var dsp in deletedShopPlaced)
        {
            yield return new SavePayload
            {
                Operation = SaveOperation.DELETE,
                Table = "ShopInteriorPlaced",
                Conditions = new Dictionary<string, object>
                {
                    { "x", dsp.x },
                    { "y", dsp.y }
                }
            };
        }

        // 작업실 인테리어
        foreach (var irp in insertedRoomPlaced)
        {
            RoomInteriorPlaced interior = roomPlaced[irp];

            yield return new SavePayload
            {
                Operation = SaveOperation.INSERT,
                Table = "RoomInteriorPlaced",
                Values = new Dictionary<string, object>
                {
                    { "itemName", interior.itemName },
                    { "interiorType", interior.interiorType },
                    { "x", interior.x },
                    { "y", interior.y },
                    { "ID", interior.ID }
                }
            };
        }
        foreach (var urp in updatedRoomPlaced)
        {
            RoomInteriorPlaced interior = roomPlaced[urp];

            yield return new SavePayload
            {
                Operation = SaveOperation.UPDATE,
                Table = "RoomInteriorPlaced",
                Values = new Dictionary<string, object>
                {
                    { "itemName", interior.itemName },
                    { "interiorType", interior.interiorType },
                    { "ID", interior.ID }
                },
                Conditions = new Dictionary<string, object>
                {
                    { "x", interior.x },
                    { "y", interior.y }
                }
            };
        }
        foreach (var drp in deletedRoomPlaced)
        {
            yield return new SavePayload
            {
                Operation = SaveOperation.DELETE,
                Table = "RoomInteriorPlaced",
                Conditions = new Dictionary<string, object>
                {
                    { "x", drp.x },
                    { "y", drp.y }
                }
            };
        }

        // 타일 변경
        foreach (var utp in updatedTilePlaced)
        {
            TileInteriorPlaced tile = tilePlaced[utp];

            yield return new SavePayload
            {
                Operation = SaveOperation.UPDATE,
                Table = "TileInteriorPlaced",
                Values = new Dictionary<string, object>
                {
                    { "itemName", tile.itemName}
                },
                Conditions = new Dictionary<string, object>
                {
                    { "tilePosition", tile.tilePosition }
                }
            };
        }

    }

    public void LoadInteriorAggregate(IEnumerable<ShopInteriorPlaced> shopPlaced, IEnumerable<RoomInteriorPlaced> roomPlaced,
        IEnumerable<TileInteriorPlaced> tilePlaced)
    {
        this.shopPlaced = shopPlaced.ToDictionary(sp => (sp.x, sp.y));
        this.roomPlaced = roomPlaced.ToDictionary(rp => (rp.x, rp.y));
        this.tilePlaced = tilePlaced.ToDictionary(tp => tp.tilePosition);
    }

    // === 게임 플레이 메서드 ===

}