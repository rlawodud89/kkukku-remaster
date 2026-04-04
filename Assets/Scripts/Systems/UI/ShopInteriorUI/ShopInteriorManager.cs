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

    [Header("가게 그리드 설정 (Top-Left 기준)")]
    public Vector3Int gridStartPos; // 왼쪽 위 시작 좌표
    public int gridWidth;
    public int gridHeight;

    [Header("문(Door) 설정")]
    public Transform doorObject;        // 인스펙터에서 문 오브젝트 연결!
    public float doorOffsetY = 0.5f;    // "조금 위"를 조절할 수 있는 수치 (인스펙터에서 조절 가능)

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

        // 크기가 달라질 수 있으므로, 기존에 깔린 바닥을 깨끗하게 지웁니다.
        floorTilemap.ClearAllTiles();

        // Top-Left 기준이므로 x는 오른쪽(+), y는 아래(-) 방향으로 그림
        for (int x = 0; x < gridWidth; x++)
        {
            for (int y = 0; y < gridHeight; y++)
            {
                Vector3Int pos = new Vector3Int(gridStartPos.x + x, gridStartPos.y - y, 0);
                floorTilemap.SetTile(pos, floorItem.tileBase);
            }
        }
    }

    public void PlaceWallpaperEntirely(WallpaperItem wallpaperItem)
    {
        if (wallpaperItem == null || wallpaperItem.wallTiles.Length < 3) return;

        // 기존 벽지 싹 지우기
        wallTilemap.ClearAllTiles();

        int startX = gridStartPos.x;
        int maxX = startX + gridWidth - 1;

        // 바닥의 시작점(Top-Left) 바로 윗칸부터 벽이 세워진다고 가정 (y + 1)
        int wallBaseY = gridStartPos.y + 1;

        for (int x = startX; x <= maxX; x++)
        {
            Vector3Int basePos = new Vector3Int(x, wallBaseY, 0);

            // 기존 코드에 있던 '오른쪽에서 두 번째 칸' 특수 처리 로직 유지
            bool isSecondFromRight = (x == maxX - 1);

            // 맨 아래칸 벽지
            if (!isSecondFromRight) wallTilemap.SetTile(basePos, wallpaperItem.wallTiles[0]);

            // 중간칸 벽지
            Vector3Int middlePos = basePos + new Vector3Int(0, 1, 0);
            wallTilemap.SetTile(middlePos, isSecondFromRight ? wallpaperItem.wallTiles[1] : wallpaperItem.wallTiles[0]);

            // 맨 위칸 벽지
            Vector3Int topPos = basePos + new Vector3Int(0, 2, 0);
            wallTilemap.SetTile(topPos, wallpaperItem.wallTiles[2]);
        }

        // ==========================================
        // ★ 문(Door) 오브젝트 자동 배치 로직
        // ==========================================
        if (doorObject != null)
        {
            // 1. 문의 그리드(Cell) 좌표 구하기 
            // X: 오른쪽에서 두번째 (maxX - 1)
            // Y: 바닥의 맨 윗줄 (gridStartPos.y)
            Vector3Int doorCellPos = new Vector3Int(maxX - 1, gridStartPos.y, 0);

            // 2. 해당 타일 칸의 '월드 좌표 정중앙' 위치 가져오기
            Vector3 cellCenterPos = floorTilemap.GetCellCenterWorld(doorCellPos);

            // 3. 정중앙에서 Y축으로 '조금 위(doorOffsetY)'만큼 올려서 문의 실제 위치 지정
            doorObject.position = cellCenterPos + new Vector3(0, doorOffsetY, 0);
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

   public void UpdateGridHighlight(Vector3 targetPos, int furnitureID, string itemName)
    {
        if (gridHighlightObj == null) return;

        var so = ServiceLocator.Get<GameData>().Inventory.GetShopInteriorItemSO(itemName);
        int itemWidth = so != null ? so.itemWidth : 1; 
        int itemHeight = so != null ? so.itemHeight : 1;


        Vector3Int bottomLeftCell = floorTilemap.WorldToCell(targetPos);
        Vector3Int topLeftCell = new Vector3Int(bottomLeftCell.x, bottomLeftCell.y + itemHeight - 1, 0);
        int startGridIndex = ShopStorageDataManager.Instance.pathfinding.PosToIndex(topLeftCell);

        if (startGridIndex == -1)
        {
            gridHighlightObj.gameObject.SetActive(false);
            return;
        }

        Vector3 cellSize = floorTilemap.layoutGrid.cellSize;
        Vector3 tilemapScale = floorTilemap.transform.lossyScale; 
        
        // 하이라이트 오브젝트 자체의 크기도 타일맵 스케일에 맞춰 줄여줍니다.
        gridHighlightObj.localScale = new Vector3(itemWidth * cellSize.x * tilemapScale.x, itemHeight * cellSize.y * tilemapScale.y, 1f);

        Vector3 cellCenterPos = floorTilemap.GetCellCenterWorld(bottomLeftCell);

        // 위치 오프셋에도 타일맵 스케일 곱하기
        float offsetX = (itemWidth - 1) * (cellSize.x * tilemapScale.x) * 0.5f;
        float offsetY = (itemHeight - 1) * (cellSize.y * tilemapScale.y) * 0.5f;

        gridHighlightObj.position = cellCenterPos + new Vector3(offsetX, offsetY, 0f);
        
        gridHighlightObj.gameObject.SetActive(true);

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
            
            var so = ServiceLocator.Get<GameData>().Inventory.GetShopInteriorItemSO(itemName);
            int itemWidth = so != null ? so.itemWidth : 1;
            int itemHeight = so != null ? so.itemHeight : 1;

            Vector3Int bottomLeftCell = floorTilemap.WorldToCell(currentSelectedFurniture.transform.position);
            Vector3Int topLeftCell = new Vector3Int(bottomLeftCell.x, bottomLeftCell.y + itemHeight - 1, 0);
            
            int newGridIndex = ShopStorageDataManager.Instance.pathfinding.PosToIndex(topLeftCell);
            var data = ShopStorageDataManager.Instance.interiorData;

            // 🌟 [추가] 튕겨내기를 위해 가구의 '원래 위치' 정보를 미리 찾아둡니다.
            Interiorinfo oldInfo = null;
            if (data.Interior != null) oldInfo = data.Interior.Find(x => x.ID == targetID);
            if (oldInfo == null && data.Table != null) oldInfo = data.Table.Find(x => x.ID == targetID);
            if (oldInfo == null && data.Casher != null && data.Casher.ID == targetID) oldInfo = data.Casher;

            // 유효성 검사
            if (newGridIndex == -1 || CheckIfPlacementInvalid(newGridIndex, itemWidth, itemHeight, targetID))
            {
                Debug.LogWarning("🚨 여기는 겹치거나 벽 때문에 놓을 수 없습니다!");
                
                // 🌟 [핵심 수정] 실패 시 가구를 원래 자리(oldInfo)로 즉시 돌려보냅니다!
                if (oldInfo != null)
                {
                    Vector3Int oldTopLeft = ShopStorageDataManager.Instance.pathfinding.IndexToPos(oldInfo.placement);
                    Vector3Int oldBottomLeft = new Vector3Int(oldTopLeft.x, oldTopLeft.y - oldInfo.Height + 1, 0);
                    currentSelectedFurniture.transform.position = floorTilemap.CellToWorld(oldBottomLeft);
                }
                return;
            }

            // 이동 성공 시 로컬 데이터 갱신
            if (oldInfo != null) oldInfo.placement = newGridIndex;

            if (data.Casher != null && data.Casher.ID == targetID)
            {
                NPCAI[] allNPCs = FindObjectsOfType<NPCAI>();
                foreach (NPCAI npc in allNPCs) npc.RedirectToNewCashier();
            }

            // DB 업데이트 및 길찾기 갱신
            ServiceLocator.Get<GameData>().Interior.TransferShopInterior(targetID, newGridIndex);
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
        if (data.Interior != null) allItems.AddRange(data.Interior);
        if (data.Table != null) allItems.AddRange(data.Table);

        foreach (var item in allItems)
        {
            if (item.ID == excludeID) continue; // 나 자신은 무시

            Vector3Int itemStartCell = pf.IndexToPos(item.placement);
            
            bool overlapX = targetCell.x >= itemStartCell.x && targetCell.x < itemStartCell.x + item.Width;
            bool overlapY = targetCell.y <= itemStartCell.y && targetCell.y > itemStartCell.y - item.Height;

            if (overlapX && overlapY)
            {
                // 🌟 [추가됨] 범인이 누구인지 정확하게 알려줍니다!
                string ghostName = item.prefab != null ? item.prefab.name : "알수없음";
                Debug.LogWarning($"🚨 [설치 차단] {targetCell} 위치는 이미 [ID:{item.ID} / {ghostName}] 가구가 차지하고 있습니다!");
                return true; 
            }
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

            // =================================================================
            // ✨ [수거 방어 로직] 유저님이 제안하신 코드 적용!
            // =================================================================
            bool hasBlanket = ServiceLocator.Get<GameData>().ShopState.IsBlanketOnShopTable(targetID);

            if (hasBlanket)
            {
                Debug.LogWarning("🚨 [수거 불가] 진열대에 이불이 남아있습니다! 먼저 비워주세요.");
                InteractionUI.Instance.HideMenu();
                return; // ❌ 여기서 함수를 종료하여 철거를 막습니다.
            }
            // =================================================================

            string itemName = currentSelectedFurniture.gameObject.name.Replace("(Clone)", "").Trim(); 
            var gameData = ServiceLocator.Get<GameData>();

            // 1. DB 처리 (가방에 아이템 추가 및 맵 데이터 삭제)
            gameData.Interior.RemoveShopInterior(targetID); 
            gameData.Inventory.AddShopInteriorItem(itemName, 1); 

            var data = ShopStorageDataManager.Instance.interiorData;

            // 일반 인테리어 리스트에서 찾아서 삭제
            if (data.Interior != null)
            {
                var targetInterior = data.Interior.Find(x => x.ID == targetID);
                if (targetInterior != null) data.Interior.Remove(targetInterior);
            }
            
            // 테이블 리스트에서 찾아서 삭제
            if (data.Table != null)
            {
                var targetTable = data.Table.Find(x => x.ID == targetID);
                if (targetTable != null) data.Table.Remove(targetTable);
            }

            // 계산대인 경우 처리
            if (data.Casher != null && data.Casher.ID == targetID)
            {
                data.Casher = null;
            }

            // 3. 실제 화면에서 오브젝트 파괴 및 길찾기 맵 갱신
            Destroy(currentSelectedFurniture.gameObject);
            ShopStorageDataManager.Instance.pathfinding.BuildObstacleMap(data);

            Debug.Log($"✅ [수거 완료] 빈 '{itemName}' 가구를 보관함으로 넣었습니다!");
            
            // 4. 선택 해제
            DeselectCurrent();
            
            // 5. UI 새로고침
            InventoryManager uiManager = FindObjectOfType<InventoryManager>();
            if (uiManager != null) 
            {
                uiManager.RefreshUI();
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
