using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Security.Cryptography;
using UnityEngine;

public class InteriorAggregate : IAggregate
{
    // 현재는 x,y 좌표 float, 그리드 칸 번호 int 기반으로 바꾸면 변경 필요

    // === 런타임 데이터 ===

    private Dictionary<(float x, float y), ShopInteriorPlaced> shopPlaced;
    private Dictionary<(float x, float y), RoomInteriorPlaced> roomPlaced;
    private Dictionary<TilePositionType, TileInteriorPlaced> tilePlaced;

    // === SO 데이터 ===

    private Dictionary<string, ShopInteriorItemSO> shopInteriorSOs;
    private Dictionary<string, RoomInteriorItemSO> roomInteriorSOs;
    private Dictionary<string, TileInteriorItemSO> tileInteriorSOs;

    // === 변경 사항 저장소 ===

    private Dictionary<(float x, float y), SaveOperation> shopPlacedChanges = new();
    private Dictionary<(float x, float y), SaveOperation> roomPlacedChanges = new();
    private HashSet<TilePositionType> updatedTilePlaced = new();


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

        shopPlaced.Clear();
        roomPlaced.Clear();
        updatedTilePlaced.Clear();
    }

    public IEnumerable<SavePayload> ToSavePayloads()
    {
        if (!IsDirty)
            yield break;

        // 가게 인테리어
        foreach (var ((x, y), change) in shopPlacedChanges)
        {
            switch (change)
            {
                case SaveOperation.INSERT:
                    ShopInteriorPlaced insertInterior = shopPlaced[(x, y)];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.INSERT,
                        Table = "ShopInteriorPlaced",
                        Values = new Dictionary<string, object>
                        {
                            { "itemName", insertInterior.itemName },
                            { "interiorType", insertInterior.interiorType },
                            { "x", insertInterior.x },
                            { "y", insertInterior.y },
                            { "ID", insertInterior.ID }
                        }
                    };

                    break;

                case SaveOperation.UPDATE:
                    ShopInteriorPlaced updateInterior = shopPlaced[(x, y)];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.UPDATE,
                        Table = "ShopInteriorPlaced",
                        Values = new Dictionary<string, object>
                        {
                            { "itemName", updateInterior.itemName },
                            { "interiorType", updateInterior.interiorType },
                            { "ID", updateInterior.ID }
                        },
                        Conditions = new Dictionary<string, object>
                        {
                            { "x", updateInterior.x },
                            { "y", updateInterior.y }
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
                            { "x", x },
                            { "y", y }
                        }
                    };

                    break;
            }
        }



        // 작업실 인테리어
        foreach (var ((x, y), change) in roomPlacedChanges)
        {
            switch (change)
            {
                case SaveOperation.INSERT:
                    RoomInteriorPlaced insertInterior = roomPlaced[(x, y)];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.INSERT,
                        Table = "RoomInteriorPlaced",
                        Values = new Dictionary<string, object>
                        {
                            { "itemName", insertInterior.itemName },
                            { "interiorType", insertInterior.interiorType },
                            { "x", insertInterior.x },
                            { "y", insertInterior.y },
                            { "ID", insertInterior.ID }
                        }
                    };

                    break;

                case SaveOperation.UPDATE:
                    RoomInteriorPlaced updateInterior = roomPlaced[(x, y)];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.UPDATE,
                        Table = "RoomInteriorPlaced",
                        Values = new Dictionary<string, object>
                        {
                            { "itemName", updateInterior.itemName },
                            { "interiorType", updateInterior.interiorType },
                            { "ID", updateInterior.ID }
                        },
                        Conditions = new Dictionary<string, object>
                        {
                            { "x", updateInterior.x },
                            { "y", updateInterior.y }
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
                            { "x", x },
                            { "y", y }
                        }
                    };

                    break;
            }
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
                    { "itemName", tile.itemName
}
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
        this.shopPlaced = shopPlaced.ToDictionary(sp => (sp.x, sp.y));
        this.roomPlaced = roomPlaced.ToDictionary(rp => (rp.x, rp.y));
        this.tilePlaced = tilePlaced.ToDictionary(tp => tp.tilePosition);

        this.shopInteriorSOs = shopInteriorSOs;
        this.roomInteriorSOs = roomInteriorSOs;
        this.tileInteriorSOs = tileInteriorSOs;
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