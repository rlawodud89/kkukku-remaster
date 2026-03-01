using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using UnityEngine.UIElements;

public class InventoryManager : MonoBehaviour
{
    [Header("슬롯 8개 연결")]
    public ItemSlot[] slots = new ItemSlot[8];

    [Header("아이템 리스트 (데이터)")]
    public List<FurnitureItem> furnitureList = new List<FurnitureItem>();
    public List<FloorItem> floorList = new List<FloorItem>();
    public List<WallpaperItem> wallpaperList = new List<WallpaperItem>();

    public Tilemap floorTilemap; // 바닥을 깔 타일맵
    public Tilemap wallTilemap;  // 벽지를 깔 타일맵

    [Header("현재 장착 중인 아이템 추적")]
    public string currentFloorName = "";       // 현재 깔려있는 바닥 이름
    public string currentWallpaperName = "";   // 현재 발려있는 벽지 이름

    private Dictionary<string, int> placedFurnitureCount = new Dictionary<string, int>();

    private int currentPage = 0;
    private int itemsPerPage = 8;
    private int currentCategory = 0;

    public GameObject InteriorCanvas;
    public GameObject MainUICanvas;

    public void OnClickInteriorButton()
    {
        Debug.Log("<color=yellow>=========================================</color>");
        Debug.Log("<color=yellow>[인벤토리 디버그 시작]</color> 인벤토리 버튼 클릭됨!");

        // 1. 현재 장착 중인 바닥 타일 이름 가져오기
        FloorItem currentFloor = ServiceLocator.Get<GameData>().Interior.GetCurrentFloorTile(TilePositionType.SHOP_FLOOR);
        if (currentFloor != null) currentFloorName = currentFloor.itemName;

        // 2. 현재 장착 중인 벽지 타일 이름 가져오기
        WallpaperItem currentWallpaper = ServiceLocator.Get<GameData>().Interior.GetCurrentWallTile(TilePositionType.SHOP_WALL);
        if (currentWallpaper != null) currentWallpaperName = currentWallpaper.itemName;

        Debug.Log($"<color=white>1. 장착 정보:</color> 바닥=[{currentFloorName}], 벽지=[{currentWallpaperName}]");

        // ✨ 3. 현재 맵에 설치된 가구들 정보 싹 가져와서 개수 세기!
        placedFurnitureCount.Clear();
        var placedItems = ServiceLocator.Get<GameData>().Interior.GetCurrentShopInterior();

        if (placedItems != null)
        {
            Debug.Log($"<color=white>2. 맵에 설치된 가구 데이터 확인:</color> 총 {placedItems.Count}개 발견됨.");
            foreach (var placed in placedItems)
            {
                if (placedFurnitureCount.ContainsKey(placed.itemName))
                    placedFurnitureCount[placed.itemName]++;
                else
                    placedFurnitureCount[placed.itemName] = 1;
            }

            foreach (var kvp in placedFurnitureCount)
            {
                Debug.Log($"   -> 설치된 가구: [{kvp.Key}] {kvp.Value}개");
            }
        }
        else
        {
            Debug.LogError("<color=red>경고:</color> GetCurrentShopInterior()가 null을 반환했습니다!");
        }

        // 4. 인벤토리 리스트 갱신
        furnitureList = ServiceLocator.Get<GameData>().Inventory.GetShopInteriorItemInventory();
        floorList = ServiceLocator.Get<GameData>().Inventory.GetFloorTileItemInventory();
        wallpaperList = ServiceLocator.Get<GameData>().Inventory.GetWallTileItemInventory();

        Debug.Log($"<color=white>3. DB 인벤토리 리스트 로드 결과:</color>");
        Debug.Log($"   -> 가구 리스트 개수: {(furnitureList != null ? furnitureList.Count : "NULL!!")}");
        Debug.Log($"   -> 바닥 리스트 개수: {(floorList != null ? floorList.Count : "NULL!!")}");
        Debug.Log($"   -> 벽지 리스트 개수: {(wallpaperList != null ? wallpaperList.Count : "NULL!!")}");

        Debug.Log("<color=yellow>=========================================</color>");

        OnClickFurnitureTab();

        InteriorCanvas.SetActive(true);
        MainUICanvas.SetActive(false);
    }


    // --- 카테고리(탭) 버튼을 눌렀을 때 실행될 함수들 ---
    public void OnClickFurnitureTab()
    {
        currentCategory = 0; currentPage = 0; RefreshUI();
    }
    public void OnClickTileTab()
    {
        currentCategory = 1; currentPage = 0; RefreshUI();
    }
    public void OnClickWallpaperTab()
    {
        currentCategory = 2; currentPage = 0; RefreshUI();
    }

    // --- 화면 그리기 로직 ---
    public void RefreshUI()
    {

        placedFurnitureCount.Clear(); 
    
    var data = ShopStorageDataManager.Instance.interiorData;
    
    // 일반 인테리어, 테이블, 계산대 모두 합쳐서 개수 세기
    var allPlaced = new List<Interiorinfo>();
    if (data.Casher != null) allPlaced.Add(data.Casher);
    allPlaced.AddRange(data.Interior);
    allPlaced.AddRange(data.Table);

    foreach (var item in allPlaced)
    {
        string name = item.prefab.name; // 혹은 itemName
        if (placedFurnitureCount.ContainsKey(name)) placedFurnitureCount[name]++;
        else placedFurnitureCount[name] = 1;
    }

    // 2. DB에서 최신 가방 데이터 가져오기
    this.furnitureList = ServiceLocator.Get<GameData>().Inventory.GetShopInteriorItemInventory();
        int startIndex = currentPage * itemsPerPage;
        Debug.Log($"<color=cyan>[UI 새로고침]</color> 카테고리: {currentCategory}, 페이지: {currentPage}, 시작 인덱스: {startIndex}");



        for (int i = 0; i < slots.Length; i++)
        {
            int itemIndex = startIndex + i;

            if (currentCategory == 0) // ================= 가구 탭 =================
            {
                if (furnitureList != null && itemIndex < furnitureList.Count)
                {
                    var item = furnitureList[itemIndex];

                    int placedCount = placedFurnitureCount.ContainsKey(item.itemName) ? placedFurnitureCount[item.itemName] : 0;
                    int availableCount = item.quantity - placedCount;

                    // 첫 번째 슬롯(i=0)일 때만 샘플로 자세한 정보를 찍어봅니다. 너무 많이 찍히면 보기 힘드니까요!
                    if (i == 0)
                    {
                        Debug.Log($"   -> [슬롯 0번 가구 샘플] 이름: {item.itemName}, 보유량: {item.quantity}, 설치됨: {placedCount}, 남은개수: {availableCount}");
                    }

                    slots[i].UpdateSlot(item.itemImage, item.itemName, 0, availableCount, true, false);
                }
                else { slots[i].UpdateSlot(null, "", 0, 0, false, false); }
            }
            else if (currentCategory == 1) // ================= 타일 탭 =================
            {
                if (floorList != null && itemIndex < floorList.Count)
                {
                    var item = floorList[itemIndex];
                    bool isEquipped = (item.itemName == currentFloorName);
                    slots[i].UpdateSlot(item.itemImage, item.itemName, 1, 0, false, isEquipped);
                }
                else { slots[i].UpdateSlot(null, "", 1, 0, false, false); }
            }
            else if (currentCategory == 2) // ================= 벽지 탭 =================
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


    public void PlaceTileOnMap(string targetName, int category, Vector3 mousePosition)
    {
        Vector3 worldPoint = Camera.main.ScreenToWorldPoint(mousePosition);
        worldPoint.z = 0;

        // 마우스를 놓은 위치를 타일맵 좌표로 변환
        Vector3Int dropFloorPos = floorTilemap.WorldToCell(worldPoint);
        Vector3Int dropWallPos = wallTilemap.WorldToCell(worldPoint);

        if (category == 1) // ================= 바닥 타일 탭일 때 =================
        {
            FloorItem itemToPlace = floorList.Find(x => x.itemName == targetName);
            if (itemToPlace != null)
            {
                if (floorTilemap.HasTile(dropFloorPos))
                {
                    BoundsInt bounds = floorTilemap.cellBounds;
                    foreach (Vector3Int pos in bounds.allPositionsWithin)
                    {
                        if (floorTilemap.HasTile(pos))
                        {
                            floorTilemap.SetTile(pos, itemToPlace.tileBase);
                        }
                    }

                    // 1. UI 및 현재 상태 업데이트
                    currentFloorName = targetName;
                    RefreshUI();

                    // ✨ 2. 팀원 API 연동: 바닥 타일 변경 데이터 DB 저장
                    ServiceLocator.Get<GameData>().Interior.SetTileInterior(TilePositionType.SHOP_FLOOR, targetName);

                    Debug.Log($"<color=green>[인테리어]</color> [{targetName}] 바닥 적용 및 DB 저장 완료!");
                }
            }
        }
        else if (category == 2) // ================= 벽지 탭일 때 =================
        {
            WallpaperItem itemToPlace = wallpaperList.Find(x => x.itemName == targetName);
            if (itemToPlace != null && itemToPlace.wallTiles.Length >= 3)
            {
                if (wallTilemap.HasTile(dropWallPos))
                {
                    BoundsInt bounds = wallTilemap.cellBounds;
                    int minY = int.MaxValue;
                    int maxX = int.MinValue;
                    HashSet<int> wallXCoords = new HashSet<int>();

                    foreach (Vector3Int pos in bounds.allPositionsWithin)
                    {
                        if (wallTilemap.HasTile(pos))
                        {
                            wallXCoords.Add(pos.x);
                            if (pos.x > maxX) maxX = pos.x;
                            if (pos.y < minY) minY = pos.y;
                        }
                    }

                    foreach (int x in wallXCoords)
                    {
                        // 기존 벽지 싹 청소
                        for (int y = bounds.yMin; y <= bounds.yMax; y++)
                        {
                            wallTilemap.SetTile(new Vector3Int(x, y, 0), null);
                        }

                        Vector3Int basePos = new Vector3Int(x, minY, 0);
                        bool isSecondFromRight = (x == maxX - 1);

                        // [1층]
                        if (!isSecondFromRight) wallTilemap.SetTile(basePos, itemToPlace.wallTiles[0]);

                        // [2층]
                        Vector3Int middlePos = basePos + new Vector3Int(0, 1, 0);
                        if (isSecondFromRight) wallTilemap.SetTile(middlePos, itemToPlace.wallTiles[1]);
                        else wallTilemap.SetTile(middlePos, itemToPlace.wallTiles[0]);

                        // [3층]
                        Vector3Int topPos = basePos + new Vector3Int(0, 2, 0);
                        wallTilemap.SetTile(topPos, itemToPlace.wallTiles[2]);
                    }

                    currentWallpaperName = targetName;
                    RefreshUI();
                    ServiceLocator.Get<GameData>().Interior.SetTileInterior(TilePositionType.SHOP_WALL, targetName);

                    Debug.Log($"<color=green>[인테리어]</color> [{targetName}] 벽지 적용 및 DB 저장 완료!");
                }
                else
                {
                    Debug.Log("벽지 영역이 아닌 곳에 놓아서 취소되었습니다.");
                }
            }
        }
    }


    // --- 페이지 넘기기 함수 ---
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
