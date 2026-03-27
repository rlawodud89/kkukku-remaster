using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq; // LINQ 사용을 위해 추가

public class RoomInventoryManager : MonoBehaviour
{
    [Header("슬롯 8개 연결")]
    public ItemSlot[] slots = new ItemSlot[8];

    [Header("아이템 리스트 (데이터)")]
    public List<FurnitureItem> furnitureList = new List<FurnitureItem>();
    public List<FloorItem> floorList = new List<FloorItem>();
    public List<WallpaperItem> wallpaperList = new List<WallpaperItem>();

    public Tilemap floorTilemap; 
    public Tilemap wallTilemap;  

    [Header("현재 장착 중인 아이템 추적")]
    public string currentFloorName = "";       
    public string currentWallpaperName = "";   

    // 현재 씬에 배치된 가구들의 수량을 저장할 딕셔너리
    private Dictionary<string, int> placedFurnitureCount = new Dictionary<string, int>();

    private int currentPage = 0;
    private int itemsPerPage = 8;
    private int currentCategory = 0;

    public GameObject InteriorCanvas;
    public GameObject MainUICanvas;

    public void OnClickInteriorButton()
    {
        UIEventManager.HideMainUI();
        Debug.Log("<color=yellow>=========================================</color>");
        Debug.Log("<color=yellow>[인벤토리 디버그 시작]</color> 인벤토리 버튼 클릭됨!");

        var gameData = ServiceLocator.Get<GameData>();

        // 1. 현재 장착 중인 바닥/벽지 이름 가져오기
        FloorItem currentFloor = gameData.Interior.GetCurrentFloorTile(TilePositionType.ROOM_FLOOR);
        if (currentFloor != null) currentFloorName = currentFloor.itemName;

        WallpaperItem currentWallpaper = gameData.Interior.GetCurrentWallTile(TilePositionType.ROOM_WALL);
        if (currentWallpaper != null) currentWallpaperName = currentWallpaper.itemName;

        Debug.Log($"<color=white>1. 장착 정보:</color> 바닥=[{currentFloorName}], 벽지=[{currentWallpaperName}]");


        placedFurnitureCount.Clear();
        
        var placedItems = RoomInteriorManager.Instance.currentPlacedList; 
        
        if (placedItems != null)
        {
            foreach (var item in placedItems)
            {
                if (placedFurnitureCount.ContainsKey(item.itemName))
                    placedFurnitureCount[item.itemName]++;
                else
                    placedFurnitureCount[item.itemName] = 1;
            }
        }

        // 3. 인벤토리 리스트 갱신 (DB에서 최신 데이터 로드)
        furnitureList = gameData.Inventory.GetRoomInteriorItemInventory();
        floorList = gameData.Inventory.GetFloorTileItemInventory();
        wallpaperList = gameData.Inventory.GetWallTileItemInventory();

        Debug.Log($"<color=white>3. DB 인벤토리 로드 결과:</color> 가구:{furnitureList?.Count}, 바닥:{floorList?.Count}, 벽지:{wallpaperList?.Count}");
        Debug.Log("<color=yellow>=========================================</color>");

        OnClickFurnitureTab(); // 기본 탭 열기

        InteriorCanvas.SetActive(true);
        MainUICanvas.SetActive(false);
    }

    // --- 카테고리(탭) 버튼 ---
    public void OnClickFurnitureTab() { currentCategory = 0; currentPage = 0; RefreshUI(); }
    public void OnClickTileTab() { currentCategory = 1; currentPage = 0; RefreshUI(); }
    public void OnClickWallpaperTab() { currentCategory = 2; currentPage = 0; RefreshUI(); }

    // --- 화면 그리기 로직 ---
    public void RefreshUI()
    {
        placedFurnitureCount.Clear(); // 일단 싹 비웁니다.
        
        var placedList = RoomInteriorManager.Instance.currentPlacedList;
        foreach (var placed in placedList)
        {
            if (placedFurnitureCount.ContainsKey(placed.itemName))
                placedFurnitureCount[placed.itemName]++;
            else
                placedFurnitureCount[placed.itemName] = 1;
        }

        int startIndex = currentPage * itemsPerPage;

        for (int i = 0; i < slots.Length; i++)
        {
            int itemIndex = startIndex + i;

            if (currentCategory == 0) // 가구 탭
            {
                if (furnitureList != null && itemIndex < furnitureList.Count)
                {
                    var item = furnitureList[itemIndex];

                    // 배치된 개수 확인 및 남은 수량 계산
                    int placedCount = placedFurnitureCount.ContainsKey(item.itemName) ? placedFurnitureCount[item.itemName] : 0;
                    int availableCount = Mathf.Max(0, item.quantity - placedCount);

                    slots[i].UpdateSlot(item.itemImage, item.itemName, 0, availableCount, true, false);
                }
                else { slots[i].UpdateSlot(null, "", 0, 0, false, false); }
            }
            else if (currentCategory == 1) // 바닥 탭
            {
                if (floorList != null && itemIndex < floorList.Count)
                {
                    var item = floorList[itemIndex];
                    bool isEquipped = (item.itemName == currentFloorName);
                    slots[i].UpdateSlot(item.itemImage, item.itemName, 1, 0, false, isEquipped);
                }
                else { slots[i].UpdateSlot(null, "", 1, 0, false, false); }
            }
            else if (currentCategory == 2) // 벽지 탭
            {
                if (wallpaperList != null && itemIndex < wallpaperList.Count)
                {
                    var item = wallpaperList[itemIndex];
                    bool isEquipped = (item.itemName == currentWallpaperName);
                    slots[i].UpdateSlot(item.itemImage, item.itemName, 2, 0, false, isEquipped);
                }
                else { slots[i].UpdateSlot(null, "", 2, 0, false, false); }
            }
        }
    }

    // --- 타일 배치 및 DB 저장 (InteriorManager와 동기화) ---
    public void PlaceTileOnMap(string targetName, int category, Vector3 mousePosition)
    {
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(mousePosition);
        worldPoint.z = 0;

        Vector3Int dropFloorPos = floorTilemap.WorldToCell(worldPoint);
        Vector3Int dropWallPos = wallTilemap.WorldToCell(worldPoint);

        if (category == 1) // 바닥 적용
        {
            FloorItem itemToPlace = floorList.Find(x => x.itemName == targetName);
            if (itemToPlace != null && floorTilemap.HasTile(dropFloorPos))
            {
                FillTilemap(floorTilemap, itemToPlace.tileBase);
                currentFloorName = targetName;
                RefreshUI();
                ServiceLocator.Get<GameData>().Interior.SetTileInterior(TilePositionType.ROOM_FLOOR, targetName);
            }
        }
        else if (category == 2) // 벽지 적용
        {
            WallpaperItem itemToPlace = wallpaperList.Find(x => x.itemName == targetName);
            if (itemToPlace != null && itemToPlace.wallTiles.Length >= 3 && wallTilemap.HasTile(dropWallPos))
            {
                ApplyWallpaper(itemToPlace);
                currentWallpaperName = targetName;
                RefreshUI();
                ServiceLocator.Get<GameData>().Interior.SetTileInterior(TilePositionType.ROOM_WALL, targetName);
            }
        }
    }

    // 타일맵 전체를 특정 타일로 채우는 헬퍼 함수
    private void FillTilemap(Tilemap map, TileBase tile)
    {
        BoundsInt bounds = map.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (map.HasTile(pos)) map.SetTile(pos, tile);
        }
    }

    // 벽지 패턴 적용 헬퍼 함수
    private void ApplyWallpaper(WallpaperItem item)
    {
        BoundsInt bounds = wallTilemap.cellBounds;
        int minY = int.MaxValue;
        int maxX = int.MinValue;
        HashSet<int> xCoords = new HashSet<int>();

        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (wallTilemap.HasTile(pos))
            {
                xCoords.Add(pos.x);
                if (pos.x > maxX) maxX = pos.x;
                if (pos.y < minY) minY = pos.y;
            }
        }

        foreach (int x in xCoords)
        {
            for (int y = bounds.yMin; y <= bounds.yMax; y++) 
                wallTilemap.SetTile(new Vector3Int(x, y, 0), null);

            bool isSecondFromRight = (x == maxX - 1);
            wallTilemap.SetTile(new Vector3Int(x, minY, 0), isSecondFromRight ? null : item.wallTiles[0]);
            wallTilemap.SetTile(new Vector3Int(x, minY + 1, 0), isSecondFromRight ? item.wallTiles[1] : item.wallTiles[0]);
            wallTilemap.SetTile(new Vector3Int(x, minY + 2, 0), item.wallTiles[2]);
        }
    }

public void OnClickNextPage()
    {
        // 현재 카테고리에 맞춰 최대 페이지 계산
        int maxCount = currentCategory == 0 ? furnitureList.Count : (currentCategory == 1 ? floorList.Count : wallpaperList.Count);
        int maxPage = Mathf.Max(0, (maxCount - 1) / itemsPerPage);

        if (currentPage < maxPage)
        {
            currentPage++;
            RefreshUI();
        }
    }

    public void OnClickPrevPage()
    {
        if (currentPage > 0)
        {
            currentPage--;
            RefreshUI();
        }
    }
}