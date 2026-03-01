using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;
using System.Linq;

public class ShopInteriorManager : MonoBehaviour
{
    public static ShopInteriorManager Instance;
    public bool IsEditMode { get; private set; } = false;

    [Header("타일맵 연결")]
    public Tilemap floorTilemap; 
    public Tilemap wallTilemap;  

    [Header("가구 부모 객체")]
    public Transform furnitureParent; 
    public Transform objectParent; 

    [Header("선택 및 하이라이트")]
    public FurnitureMobileDrag currentSelectedFurniture;
    public Transform gridHighlightObj;      
    public SpriteRenderer highlightSprite;  
    public Color colorValid = new Color(0, 1, 0, 0.5f);   
    public Color colorInvalid = new Color(1, 0, 0, 0.5f); 

    private int myID = -1;

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    public void TurnOnEditMode() { IsEditMode = true; }

    public void TurnOffEditMode()
    {
        IsEditMode = false;
        DeselectCurrent();
    }

    // ==========================================
    // 초기화 및 로드 함수들
    // ==========================================
    public void InitializeShopInterior()
    {
        PlaceAllFurnitures();

        FloorItem currentFloor = ServiceLocator.Get<GameData>().Interior.GetCurrentFloorTile(TilePositionType.SHOP_FLOOR);
        if (currentFloor != null) PlaceFloorEntirely(currentFloor);

        WallpaperItem currentWallpaper = ServiceLocator.Get<GameData>().Interior.GetCurrentWallTile(TilePositionType.SHOP_WALL);
        if (currentWallpaper != null) PlaceWallpaperEntirely(currentWallpaper);
    }

    private void PlaceAllFurnitures()
    {
        ShopInteriorData data = ShopStorageDataManager.Instance.interiorData;

        if (data.Casher != null && data.Casher.prefab != null) SpawnFurniture(data.Casher);
        if (data.Interior != null) foreach (var info in data.Interior) SpawnFurniture(info);
        if (data.Table != null) foreach (var info in data.Table) SpawnFurniture(info);
    }

    private void SpawnFurniture(Interiorinfo info)
    {
        if (info.prefab == null) return;

        Vector3Int topLeftCell = ShopStorageDataManager.Instance.pathfinding.IndexToPos(info.placement);
        Vector3Int bottomLeftCell = new Vector3Int(topLeftCell.x, topLeftCell.y - info.Height + 1, 0);
        Vector3 spawnPosition = floorTilemap.CellToWorld(bottomLeftCell);

        GameObject spawnedFurniture = Instantiate(info.prefab, spawnPosition, Quaternion.identity, objectParent);
        spawnedFurniture.name = info.prefab.name;

        if (spawnedFurniture.TryGetComponent<ShopStorageClick>(out var storageClick))
        {
            storageClick.storageID = info.ID;
            storageClick.UpdateSpriteState();
        }
        
        // 에디트 모드를 위한 공통 컴포넌트 ID 주입
        if (spawnedFurniture.TryGetComponent<WR_StorageController>(out var wrController))
        {
            wrController.myStorageID = info.ID;
        }
    }

    public void PlaceFloorEntirely(FloorItem floorItem)
    {
        if (floorItem == null || floorItem.tileBase == null) return;
        BoundsInt bounds = floorTilemap.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (floorTilemap.HasTile(pos)) floorTilemap.SetTile(pos, floorItem.tileBase);
        }
    }

    public void PlaceWallpaperEntirely(WallpaperItem wallpaperItem)
    {
        if (wallpaperItem == null || wallpaperItem.wallTiles.Length < 3) return;
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

        if (wallXCoords.Count == 0) return;

        foreach (int x in wallXCoords)
        {
            for (int y = bounds.yMin; y <= bounds.yMax; y++) wallTilemap.SetTile(new Vector3Int(x, y, 0), null);

            Vector3Int basePos = new Vector3Int(x, minY, 0);
            bool isSecondFromRight = (x == maxX - 1);

            if (!isSecondFromRight) wallTilemap.SetTile(basePos, wallpaperItem.wallTiles[0]);
            Vector3Int middlePos = basePos + new Vector3Int(0, 1, 0);
            wallTilemap.SetTile(middlePos, isSecondFromRight ? wallpaperItem.wallTiles[1] : wallpaperItem.wallTiles[0]);
            Vector3Int topPos = basePos + new Vector3Int(0, 2, 0);
            wallTilemap.SetTile(topPos, wallpaperItem.wallTiles[2]);
        }
    }

    // ==========================================
    // ★ 에디트 모드 및 인벤토리 설치 로직 (가게 전용 개조)
    // ==========================================

    public void DragDropFurnitureFromInventory(string itemName, Vector3 dropWorldPos)
    {
        var gameData = ServiceLocator.Get<GameData>();
        Vector3Int cellPos = floorTilemap.WorldToCell(dropWorldPos);
        int dropIndex = ShopStorageDataManager.Instance.pathfinding.PosToIndex(cellPos); 

        if (dropIndex < 0) return;

        var inventoryList = gameData.Inventory.GetShopInteriorItemInventory();
        FurnitureItem itemData = inventoryList.Find(x => x.itemName == itemName);

        if (itemData == null || itemData.prefab == null) return;

        // 겹침 검사
        if (CheckIfPlacementInvalid(dropIndex, itemData.gridSize.x, itemData.gridSize.y, -1))
        {
            Debug.LogWarning("🚨 그 자리에는 가구를 놓을 수 없습니다!");
            return;
        }

        int newID = gameData.Interior.AddShopInterior(dropIndex, itemName);

        Interiorinfo newInfo = new Interiorinfo()
        {
            ID = newID,
            placement = dropIndex,
            prefab = itemData.prefab,
            Width = itemData.gridSize.x,
            Height = itemData.gridSize.y
        };

        // 로컬 리스트에 추가 (이불가게 데이터)
        ShopStorageDataManager.Instance.interiorData.Interior.Add(newInfo);

        SpawnFurniture(newInfo);
        ShopStorageDataManager.Instance.pathfinding.BuildObstacleMap(ShopStorageDataManager.Instance.interiorData);
        gameData.Inventory.RemoveShopInteriorItem(itemName, 1);

        InventoryManager uiManager = FindObjectOfType<InventoryManager>();
        if (uiManager != null) uiManager.RefreshUI();
    }

    public void SelectFurniture(FurnitureMobileDrag furniture)
    {
        if (furniture.TryGetComponent<WR_StorageController>(out var script)) myID = script.myStorageID;
        
        if (currentSelectedFurniture != null && currentSelectedFurniture != furniture)
        {
            currentSelectedFurniture.SetHighlight(false);
        }

        currentSelectedFurniture = furniture;
        UpdateGridHighlight(furniture.transform.position, myID, furniture.gameObject.name.Replace("(Clone)", "").Trim());
        
        if (currentSelectedFurniture != null) currentSelectedFurniture.SetHighlight(true);
    }

    public void DeselectCurrent()
    {
        if (currentSelectedFurniture != null)
        {
            currentSelectedFurniture.SetHighlight(false);
            currentSelectedFurniture = null;
        }
        InteractionUI.Instance.HideMenu();
        HideGridHighlight();
    }

// ==========================================
    // ★ 수정된 하이라이트 로직 (Top-Left 기준점 보정)
    // ==========================================
    public void UpdateGridHighlight(Vector3 targetPos, int furnitureID, string itemName)
    {
        if (gridHighlightObj == null) return;

        // 1. SO 데이터 가져오기 (크기를 알아야 기준점을 보정할 수 있습니다)
        var so = ServiceLocator.Get<GameData>().Inventory.GetRoomInteriorItemSO(itemName);
        int itemWidth = so != null ? so.itemWidth : 1; 
        int itemHeight = so != null ? so.itemHeight : 1;

        // 2. targetPos는 가구의 왼쪽 아래(Bottom-Left)입니다.
        Vector3Int bottomLeftCell = floorTilemap.WorldToCell(targetPos);

        // 3. ✨ 핵심: 가게 길찾기는 'Top-Left' 기준이므로, 높이만큼 Y를 더해줍니다!
        Vector3Int topLeftCell = new Vector3Int(bottomLeftCell.x, bottomLeftCell.y + itemHeight - 1, 0);

        // 4. 보정된 Top-Left 좌표로 인덱스를 구합니다.
        int startGridIndex = ShopStorageDataManager.Instance.pathfinding.PosToIndex(topLeftCell);

        if (startGridIndex == -1)
        {
            gridHighlightObj.gameObject.SetActive(false);
            return;
        }

        // 5. 하이라이트 크기 및 위치 적용
        Vector3 cellSize = floorTilemap.layoutGrid.cellSize;
        gridHighlightObj.localScale = new Vector3(itemWidth * cellSize.x, itemHeight * cellSize.y, 1f);

        Vector3 cellBottomLeft = floorTilemap.CellToWorld(bottomLeftCell);
        gridHighlightObj.position = cellBottomLeft + new Vector3(itemWidth * cellSize.x * 0.5f, itemHeight * cellSize.y * 0.5f, 0f);
        
        gridHighlightObj.gameObject.SetActive(true);

        // 6. 겹침 검사
        bool isInvalid = CheckIfPlacementInvalid(startGridIndex, itemWidth, itemHeight, furnitureID);
        if (highlightSprite) highlightSprite.color = isInvalid ? colorInvalid : colorValid;
    }


    // ==========================================
    // ★ 수정된 이동 확정 로직 (이것도 Top-Left 보정 필수!)
    // ==========================================
    public void ConfirmFurnitureMove()
    {
        if (currentSelectedFurniture == null) return;

        if (currentSelectedFurniture.TryGetComponent<WR_StorageController>(out var script))
        {
            int targetID = script.myStorageID;
            string itemName = currentSelectedFurniture.gameObject.name.Replace("(Clone)", "").Trim();
            
            // 크기 가져오기
            var so = ServiceLocator.Get<GameData>().Inventory.GetRoomInteriorItemSO(itemName);
            int itemWidth = so != null ? so.itemWidth : 1;
            int itemHeight = so != null ? so.itemHeight : 1;

            // ✨ 이동할 때도 Bottom-Left 좌표를 구한 뒤 Top-Left로 올려서 계산해야 합니다.
            Vector3Int bottomLeftCell = floorTilemap.WorldToCell(currentSelectedFurniture.transform.position);
            Vector3Int topLeftCell = new Vector3Int(bottomLeftCell.x, bottomLeftCell.y + itemHeight - 1, 0);
            
            int newGridIndex = ShopStorageDataManager.Instance.pathfinding.PosToIndex(topLeftCell);

            if (newGridIndex == -1) return;

            if (CheckIfPlacementInvalid(newGridIndex, itemWidth, itemHeight, targetID))
            {
                Debug.LogWarning("🚨 여기는 겹치거나 벽 때문에 놓을 수 없습니다!");
                return;
            }

            // 로컬 데이터 갱신
            var data = ShopStorageDataManager.Instance.interiorData;
            var placedData = data.Interior.Find(x => x.ID == targetID);
            if (placedData != null) placedData.placement = newGridIndex;
            
            var placedTable = data.Table.Find(x => x.ID == targetID);
            if (placedTable != null) placedTable.placement = newGridIndex;

            if (data.Casher != null && data.Casher.ID == targetID) data.Casher.placement = newGridIndex;

            // DB 업데이트
            ServiceLocator.Get<GameData>().Interior.TransferShopInterior(targetID, newGridIndex);
            
            // 길찾기 맵 갱신
            ShopStorageDataManager.Instance.pathfinding.BuildObstacleMap(data);

            Debug.Log($"✅ [이동 완료] 가구가 {newGridIndex}번 칸으로 이동 저장되었습니다!");
            DeselectCurrent();
        }
    }
    public void HideGridHighlight() { if (gridHighlightObj != null) gridHighlightObj.gameObject.SetActive(false); }

    // ==========================================
    // ★ 위치 검증 로직 (Pathfinding 연동)
    // ==========================================
    private bool CheckIfPlacementInvalid(int startIndex, int width, int height, int excludeID)
    {
        var pf = ShopStorageDataManager.Instance.pathfinding;
        Vector3Int startCell = pf.IndexToPos(startIndex);

        for (int w = 0; w < width; w++)
        {
            for (int h = 0; h < height; h++)
            {
                // 가게 기준은 왼쪽 위(Top-Left) 시작이므로 x는 +, y는 - 방향으로 순회합니다.
                Vector3Int checkPos = startCell + new Vector3Int(w, -h, 0);
                
                // 1. 바닥이 없거나 벽이 막혀있으면 불가
                if (!floorTilemap.HasTile(checkPos)) return true;
                if (wallTilemap != null && wallTilemap.HasTile(checkPos)) return true;

                // 2. 다른 가구랑 겹치는지 검사 (excludeID는 자기 자신이므로 제외)
                if (IsGridOccupiedByOther(checkPos, excludeID)) return true;
            }
        }
        return false; // 완벽하게 빔!
    }

    private bool IsGridOccupiedByOther(Vector3Int targetCell, int excludeID)
    {
        var data = ShopStorageDataManager.Instance.interiorData;
        var pf = ShopStorageDataManager.Instance.pathfinding;
        
        var allItems = new List<Interiorinfo>();
        if (data.Casher != null) allItems.Add(data.Casher);
        allItems.AddRange(data.Interior);
        allItems.AddRange(data.Table);

        foreach (var item in allItems)
        {
            if (item.ID == excludeID) continue; // 나 자신은 검사 무시!

            Vector3Int itemStartCell = pf.IndexToPos(item.placement);
            
            // 타겟 셀이 이 가구의 영역 안에 들어오는지 판별
            bool overlapX = targetCell.x >= itemStartCell.x && targetCell.x < itemStartCell.x + item.Width;
            bool overlapY = targetCell.y <= itemStartCell.y && targetCell.y > itemStartCell.y - item.Height;

            if (overlapX && overlapY) return true; // 밟음 (겹침)
        }
        return false;
    }

    // ==========================================
    // ★ 가구 수거 및 이동 확정 로직
    // ==========================================

    public void RemoveFurnitureData(FurnitureMobileDrag furniture)
    {
        if(furniture.TryGetComponent<WR_StorageController>(out var dataScript))
        {
            var data = ShopStorageDataManager.Instance.interiorData.Interior;
            var target = data.Find(x => x.ID == dataScript.myStorageID);
            if (target != null) data.Remove(target);
        }
        
        if (currentSelectedFurniture == furniture) DeselectCurrent();
    }

    public void StoreSelectedFurniture()
    {
        if (currentSelectedFurniture == null) return;

        if (currentSelectedFurniture.TryGetComponent<WR_StorageController>(out var script))
        {
            int targetID = script.myStorageID;
            string itemName = currentSelectedFurniture.gameObject.name.Replace("(Clone)", "").Trim(); 
            var gameData = ServiceLocator.Get<GameData>();

            // 1. DB 처리
            gameData.Interior.RemoveShopInterior(targetID); 
            gameData.Inventory.AddShopInteriorItem(itemName, 1); 

            // 2. 로컬 리스트(가게 데이터)에서 삭제
            var data = ShopStorageDataManager.Instance.interiorData;
            var targetData = data.Interior.Find(x => x.ID == targetID);
            if (targetData != null) data.Interior.Remove(targetData);
            
            // 테이블이나 계산대라면?
            var targetTable = data.Table.Find(x => x.ID == targetID);
            if (targetTable != null) data.Table.Remove(targetTable);

            // 3. 실제 화면에서 파괴 및 길찾기 맵 갱신
            Destroy(currentSelectedFurniture.gameObject);
            ShopStorageDataManager.Instance.pathfinding.BuildObstacleMap(data);

            Debug.Log($"✅ [수거 완료] '{itemName}' 가구를 보관함으로 넣었습니다!");
            DeselectCurrent();
            
            InventoryManager uiManager = FindObjectOfType<InventoryManager>();
            if (uiManager != null) uiManager.RefreshUI();
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
