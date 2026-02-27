using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ShopInteriorManager : MonoBehaviour
{
    [Header("타일맵 연결")]
    public Tilemap floorTilemap; // 바닥 타일맵
    public Tilemap wallTilemap;  // 벽지 타일맵

    public Transform furnitureParent; // 가구들이 생성될 부모 객체

    [Header("가구 부모 객체")]
    public Transform objectParent; // 유니티 에디터에서 'object'라는 이름의 게임오브젝트를 끌어다 넣으세요!

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

        // 1. Pathfinding을 통해 '왼쪽 위(Top-Left)' 그리드 좌표를 가져옵니다.
        Vector3Int topLeftCell = ShopStorageDataManager.Instance.pathfinding.IndexToPos(info.placement);

        // 2. 가구 피벗이 '왼쪽 아래(Bottom-Left)'로 설정되었으므로, 
        // 가구가 차지하는 전체 영역 중 가장 '왼쪽 아래 칸'의 좌표를 구합니다.
        // (Y축은 아래로 갈수록 작아지므로 Height - 1 을 빼줍니다.)
        Vector3Int bottomLeftCell = new Vector3Int(topLeftCell.x, topLeftCell.y - info.Height + 1, 0);

        // 3. 타일맵의 CellToWorld는 해당 타일 칸의 '왼쪽 아래 꼭짓점'을 정확히 반환합니다!
        Vector3 spawnPosition = floorTilemap.CellToWorld(bottomLeftCell);

        // 4. 가구 오브젝트 생성
        GameObject spawnedFurniture = Instantiate(info.prefab, spawnPosition, Quaternion.identity, objectParent);
        spawnedFurniture.name = info.prefab.name;

        // ✨ 5. 핵심: 생성된 가구에 고유 ID 전달하기
        // 생성된 오브젝트에서 ShopStorageClick 컴포넌트를 찾습니다.
        ShopStorageClick storageClick = spawnedFurniture.GetComponent<ShopStorageClick>();
        if (storageClick != null)
        {
            // DB에서 가져온 진짜 고유 ID를 스크립트에 덮어씌워 줍니다.
            storageClick.storageID = info.ID;

            // 혹시 모를 딜레이를 방지하기 위해, ID를 넣자마자 바로 이미지를 업데이트하도록 강제로 한 번 더 불러줍니다.
            storageClick.UpdateSpriteState();
        }
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
        if (wallpaperItem == null || wallpaperItem.wallTiles.Length < 3) return;

        BoundsInt bounds = wallTilemap.cellBounds;
        int minY = int.MaxValue;
        int maxX = int.MinValue;
        HashSet<int> wallXCoords = new HashSet<int>();

        // 1. 벽이 있는 X 좌표들을 싹 다 수집하고, '진짜 1층(가장 아래)'의 Y 좌표를 찾습니다.
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (wallTilemap.HasTile(pos))
            {
                wallXCoords.Add(pos.x);
                if (pos.x > maxX) maxX = pos.x;
                if (pos.y < minY) minY = pos.y;
            }
        }

        if (wallXCoords.Count == 0) return;

        // 2. 수집한 X 좌표마다, 기존에 꼬여있던 1~4층 타일들을 싹 다 지우고 새로 바릅니다.
        foreach (int x in wallXCoords)
        {
            // ✨ 핵심: 공중에 뜬 버그 타일까지 포함해서 해당 세로줄을 싹 지워버림
            for (int y = bounds.yMin; y <= bounds.yMax; y++)
            {
                wallTilemap.SetTile(new Vector3Int(x, y, 0), null);
            }

            // 이제 minY를 기준으로 정확하게 3단을 쌓아 올립니다.
            Vector3Int basePos = new Vector3Int(x, minY, 0);
            bool isSecondFromRight = (x == maxX - 1);

            // [1층 / 가장 아래 줄] 문 공간은 비우기
            if (!isSecondFromRight) wallTilemap.SetTile(basePos, wallpaperItem.wallTiles[0]);

            // [2층 / 중간 줄] 문 윗부분(상단) 또는 기본 타일
            Vector3Int middlePos = basePos + new Vector3Int(0, 1, 0);
            if (isSecondFromRight) wallTilemap.SetTile(middlePos, wallpaperItem.wallTiles[1]);
            else wallTilemap.SetTile(middlePos, wallpaperItem.wallTiles[0]);

            // [3층 / 가장 윗 줄] 하단 타일
            Vector3Int topPos = basePos + new Vector3Int(0, 2, 0);
            wallTilemap.SetTile(topPos, wallpaperItem.wallTiles[2]);
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
