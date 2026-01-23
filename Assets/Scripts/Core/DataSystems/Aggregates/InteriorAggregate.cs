using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

public class InteriorAggregate : IAggregate
{
    private Dictionary<Position, ShopInteriorPlaced> shopPlaced;
    private Dictionary<Position, RoomInteriorPlaced> roomPlaced;
    private Dictionary<TilePositionType, TileInteriorPlaced> tilePlaced;


    private HashSet<Position> insertedShopPlaced = new();
    private HashSet<Position> updatedShopPlaced = new();
    private HashSet<Position> deletedShopPlaced = new();

    private HashSet<Position> insertedRoomPlaced = new();
    private HashSet<Position> updatedRoomPlaced = new();
    private HashSet<Position> deletedRoomPlaced = new();

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
        this.shopPlaced = shopPlaced.ToDictionary(sp => new Position(sp.x, sp.y));
        this.roomPlaced = roomPlaced.ToDictionary(rp => new Position(rp.x, rp.y));
        this.tilePlaced = tilePlaced.ToDictionary(tp => tp.tilePosition);
    }

    // === 게임 플레이 메서드 ===

}


// 임시로 사용하는 위치 표현 구조체 (float 기반이 아니라 칸 번호 int 기반으로 바꾸면 변경 필요)
public struct Position
{
    public float x;
    public float y;

    public Position(float x, float y)
    {
        this.x = x;
        this.y = y;
    }

    public static bool operator ==(Position left, Position right)
    {
        return (left.x == right.x) && (left.y == right.y);
    }
    public static bool operator !=(Position left, Position right)
    {
        return !((left.x == right.x) && (left.y == right.y));
    }

    public override bool Equals(object obj)
    {
        if (!(obj is Position)) return false;
        return Equals((Position)obj);
    }

    public override int GetHashCode() => x.GetHashCode();
}