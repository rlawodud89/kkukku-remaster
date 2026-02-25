using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ShopInteriorManager : MonoBehaviour
{
    [Header("타일맵 연결")]
    public Tilemap floorTilemap; // 바닥 타일맵
    public Tilemap wallTilemap;  // 벽지 타일맵

    public Transform furnitureParent; // 가구들이 생성될 부모 객체

    // ==========================================
    // 데이터 매니저에서 호출할 초기화 함수
    // ==========================================
    public void InitializeShopInterior()
    {
        // 1. 가구 배치
        PlaceAllFurnitures();

        // 2. 바닥 타일 불러와서 깔기
        FloorItem currentFloor = ServiceLocator.Get<GameData>().Interior.GetCurrentFloorTile(TilePositionType.SHOP_FLOOR);
        if (currentFloor != null)
        {
            PlaceFloorEntirely(currentFloor);
            Debug.Log($"<color=cyan>[인테리어]</color> 바닥 타일 적용 완료: {currentFloor.itemName}");
        }

        // 3. 벽지 타일 불러와서 깔기
        WallpaperItem currentWallpaper = ServiceLocator.Get<GameData>().Interior.GetCurrentWallTile(TilePositionType.SHOP_WALL);
        if (currentWallpaper != null)
        {
            PlaceWallpaperEntirely(currentWallpaper);
            Debug.Log($"<color=cyan>[인테리어]</color> 벽지 적용 완료: {currentWallpaper.itemName}");
        }
    }

    // ==========================================
    // 1. 가구 배치 로직
    // ==========================================
    private void PlaceAllFurnitures()
    {
        ShopInteriorData data = ShopStorageDataManager.Instance.interiorData;

        if (data.Casher != null && data.Casher.prefab != null) SpawnFurniture(data.Casher);

        if (data.Interior != null)
        {
            foreach (var info in data.Interior) SpawnFurniture(info);
        }

        if (data.Table != null)
        {
            foreach (var info in data.Table) SpawnFurniture(info);
        }
    }

    private void SpawnFurniture(Interiorinfo info)
    {
        if (info.prefab == null) return;

        int gridWidth = ShopStorageDataManager.Instance.pathfinding.totalGridWidth;
        int x = info.placement % gridWidth;
        int y = info.placement / gridWidth;

        Vector3Int cellPos = new Vector3Int(x, y, 0);
        Vector3 spawnPosition = floorTilemap.GetCellCenterWorld(cellPos);

        Instantiate(info.prefab, spawnPosition, Quaternion.identity, furnitureParent);
    }

    // ==========================================
    // 2. 바닥 및 벽지 배치 로직
    // ==========================================
    public void PlaceFloorEntirely(FloorItem floorItem)
    {
        if (floorItem == null || floorItem.tileBase == null) return;

        BoundsInt bounds = floorTilemap.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (floorTilemap.HasTile(pos))
            {
                floorTilemap.SetTile(pos, floorItem.tileBase);
            }
        }
    }

    public void PlaceWallpaperEntirely(WallpaperItem wallpaperItem)
    {
        // 벽지는 타일이 3개(중간, 위, 아래) 필요하므로 배열 길이 확인
        if (wallpaperItem == null || wallpaperItem.wallTiles.Length < 3) return;

        BoundsInt bounds = wallTilemap.cellBounds;

        // 1. 벽 타일이 있는 곳 중 '가장 오른쪽 X 좌표(maxX)' 찾기
        int maxX = int.MinValue;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (wallTilemap.HasTile(pos))
            {
                if (pos.x > maxX)
                {
                    maxX = pos.x;
                }
            }
        }

        // 2. 본격적으로 조건에 맞춰 벽지 바르기
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (wallTilemap.HasTile(pos))
            {
                // 현재 칸이 '가장 오른쪽에서부터 두 번째 칸'인지 확인
                bool isSecondFromRight = (pos.x == maxX - 1);

                // [1층 / 가장 아래 줄] 
                if (isSecondFromRight)
                {
                    wallTilemap.SetTile(pos, null); // 아무 타일도 넣지 않음 (문 공간)
                }
                else
                {
                    wallTilemap.SetTile(pos, wallpaperItem.wallTiles[0]); // 기본 타일베이스
                }

                // [2층 / 그 다음 줄] 
                Vector3Int middlePos = pos + new Vector3Int(0, 1, 0);
                if (isSecondFromRight)
                {
                    wallTilemap.SetTile(middlePos, wallpaperItem.wallTiles[1]); // 상단 타일베이스
                }
                else
                {
                    wallTilemap.SetTile(middlePos, wallpaperItem.wallTiles[0]); // 기본 타일베이스
                }

                // [3층 / 가장 위의 줄]
                Vector3Int topPos = pos + new Vector3Int(0, 2, 0);
                wallTilemap.SetTile(topPos, wallpaperItem.wallTiles[2]); // 하단 타일베이스
            }
        }
    }
}

public class FurnitureItem
{
    public string itemName;       // 이름
    public Sprite itemImage;      // 이미지
    public Vector2Int gridSize;   // 그리드 개수 (예: 가로 2칸, 세로 1칸을 차지한다면 X:2, Y:1)
    public int quantity;          // 보유 개수
    public GameObject prefab;
}

public class FloorItem
{
    public string itemName;       // 이름
    public Sprite itemImage;      // 이미지
    public TileBase tileBase;
}
public class WallpaperItem
{
    public string itemName;       // 이름
    public Sprite itemImage;      // 이미지
    public TileBase[] wallTiles = new TileBase[3];
}
