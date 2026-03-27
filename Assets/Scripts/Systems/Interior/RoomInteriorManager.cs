using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; 
using System.Linq;
using UnityEngine.Tilemaps;

public class RoomInteriorManager : MonoBehaviour
{
    public static RoomInteriorManager Instance;
    public bool IsEditMode { get; private set; } = false;

    [Header("UI Reference")]
    [SerializeField] private Button interiorStorageButton;
    [SerializeField] public Grid mainGrid; // ★ 인스펙터 연결 필수
    [SerializeField] private TextMeshProUGUI buttonText;

    

    [Header("Prefabs & Parents")]
    [SerializeField] private List<GameObject> allFurniturePrefabs; 
    [SerializeField] private Transform furnitureParent; 

   [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 8;  
    [SerializeField] private int gridHeight = 6; 
    [SerializeField] private float cellSize = 0.7f; 

    [Header("타일맵 연결 (바닥/벽지)")]
    public Tilemap floorTilemap; 
    public Tilemap wallTilemap;

    [Header("Grid Highlight")]
    public Transform gridHighlightObj;      // 방금 만든 GridHighlight 객체 연결
    public SpriteRenderer highlightSprite;  // 색상을 바꾸기 위해 연결
    public Color colorValid = new Color(0, 1, 0, 0.5f);   // 놓을 수 있음 (반투명 초록)
    public Color colorInvalid = new Color(1, 0, 0, 0.5f); // 겹침/불가 (반투명 빨강)

    [Header("Visual Adjustment")]
    [SerializeField] private float yVisualOffset = 0.35f; // ★ 이 값을 조절해서 위치를 땡기세요!


    [Header("선택된 가구 관리")]
    public FurnitureMobileDrag currentSelectedFurniture; // 현재 선택된 녀석

    public List<RoomInteriorPlaced> currentPlacedList = new List<RoomInteriorPlaced>();

    RoomInventoryManager uiManager;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }

    // 아무도 없으면 -1, 가구가 있으면 그 가구의 ID를 저장할 배열
    public int[] gridOccupancyMap;

    private void Start()
    {
        gridOccupancyMap = new int[gridWidth * gridHeight];
        for(int i = 0; i < gridOccupancyMap.Length; i++) gridOccupancyMap[i] = -1;

        SpawnFurniture(); 
        //InjectTestData(); // ★ 테스트용 데이터 주입 함수 (필요할 때만 켜세요!)
        InitializeRoomTiles(); 
        uiManager = FindObjectOfType<RoomInventoryManager>();
    }

    public void MarkGridOccupancy(int startIndex, int width, int height, int furnitureID)
    {
        int startX = startIndex % gridWidth;
        int startY = startIndex / gridWidth;

        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int index = (startY + y) * gridWidth + (startX + x);
                gridOccupancyMap[index] = furnitureID; 
            }
        }
    }


    public void InitializeRoomTiles()
    {
        // ※ TilePositionType.ROOM_FLOOR / ROOM_WALL 부분은 유저님의 실제 Enum 이름에 맞게 수정해주세요!
        FloorItem currentFloor = ServiceLocator.Get<GameData>().Interior.GetCurrentFloorTile(TilePositionType.ROOM_FLOOR);
        if (currentFloor != null) PlaceFloorEntirely(currentFloor);

        WallpaperItem currentWallpaper = ServiceLocator.Get<GameData>().Interior.GetCurrentWallTile(TilePositionType.ROOM_WALL);
        if (currentWallpaper != null) PlaceWallpaperEntirely(currentWallpaper);
    }

    public void PlaceFloorEntirely(FloorItem floorItem)
    {
        if (floorItem == null || floorItem.tileBase == null || floorTilemap == null) return;
        BoundsInt bounds = floorTilemap.cellBounds;
        foreach (Vector3Int pos in bounds.allPositionsWithin)
        {
            if (floorTilemap.HasTile(pos)) floorTilemap.SetTile(pos, floorItem.tileBase);
        }
    }

    public void PlaceWallpaperEntirely(WallpaperItem wallpaperItem)
    {
        if (wallpaperItem == null || wallpaperItem.wallTiles.Length < 3 || wallTilemap == null) return;
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
            Vector3Int middlePos = basePos + new Vector3Int(0, 1, 0);
            //Vector3Int topPos = basePos + new Vector3Int(0, 2, 0);

            
           wallTilemap.SetTile(basePos, wallpaperItem.wallTiles[0]);
           wallTilemap.SetTile(middlePos, wallpaperItem.wallTiles[0]);
           // wallTilemap.SetTile(topPos, wallpaperItem.wallTiles[2]);
        }
    }
    

    // =================================================================
    // ★ 바닥/벽지 교체 및 드래그 앤 드롭 적용 로직
    // =================================================================

    public void ChangeRoomFloor(string itemName)
    {
        var gameData = ServiceLocator.Get<GameData>();
        
        // 1. DB 업데이트 (TilePositionType 확인 필수!)
        gameData.Interior.SetTileInterior(TilePositionType.ROOM_FLOOR, itemName);
        
        // 2. 화면에 즉시 적용
        FloorItem newFloor = gameData.Interior.GetCurrentFloorTile(TilePositionType.ROOM_FLOOR);
        if (newFloor != null) PlaceFloorEntirely(newFloor);
    }

    public void ChangeRoomWallpaper(string itemName)
    {
        var gameData = ServiceLocator.Get<GameData>();
        
        // 1. DB 업데이트 
        gameData.Interior.SetTileInterior(TilePositionType.ROOM_WALL, itemName);
        
        // 2. 화면에 즉시 적용
        WallpaperItem newWallpaper = gameData.Interior.GetCurrentWallTile(TilePositionType.ROOM_WALL);
        if (newWallpaper != null) PlaceWallpaperEntirely(newWallpaper);
    }

    public void PlaceTileOnMap(string itemName, int category, Vector3 mousePos)
    {
        if (category == 1) // 타일(바닥)
        {
            // 같은 스크립트 안에 있으므로 Instance 없이 바로 호출
            ChangeRoomFloor(itemName);
            Debug.Log($"[작업실] {itemName} 바닥을 깔았습니다!");
        }
        else if (category == 2) // 벽지
        {
            ChangeRoomWallpaper(itemName);
            Debug.Log($"[작업실] {itemName} 벽지를 발랐습니다!");
        }

        // ✨ UI 매니저를 통해 UI 갱신! (현재 장착된 타일을 회색으로 만들기 위함)
        if (uiManager != null) 
        {
            uiManager.RefreshUI(); 
        }
    }
    private void InjectTestData()
    {
        var interiorDB = ServiceLocator.Get<GameData>().Interior;

        Debug.Log("★ 테스트 데이터 강제 주입 시작!");

        // ---------------------------------------------------------
        // 여기서 "이름"은 Inspector의 All Furniture Prefabs에 등록된
        // 프리팹 이름과 ★토씨 하나 틀리지 않고 똑같아야★ 합니다.
        // ---------------------------------------------------------

        // 1. (0,0) 위치인 0번 그리드에 가구 추가
        //interiorDB.AddRoomInterior(0, "BlanketStorage"); 
        //interiorDB.AddRoomInterior(8, "PersonalCraftBox"); 
        //interiorDB.AddRoomInterior(15, "MaterialStorage"); 
        //interiorDB.AddRoomInterior(14, "SnackBox");
        //interiorDB.AddRoomInterior(18, "여우");
        ServiceLocator.Get<GameData>().Interior.SetTileInterior(
            TilePositionType.ROOM_WALL, "빈티지꽃타일");
    }

    public void TurnOnEditMode()
    {
        IsEditMode = true;
    }

    public void TurnOffEditMode()
    {
        IsEditMode = false;
        DeselectCurrent();
        HideGridHighlight();
        currentSelectedFurniture = null;
    }

    // =================================================================
    // 1. [로드] DB에서 불러와서 배치하기
    // =================================================================


public void SpawnFurniture()
{
    var gameData = ServiceLocator.Get<GameData>();
    if (gameData == null) return;

    var interiorList = gameData.Interior.GetCurrentRoomInterior();
    
    currentPlacedList = new List<RoomInteriorPlaced>(interiorList);

    // 2. 화면 초기화 (기존 가구 삭제)
    if (furnitureParent != null)
    {
        foreach (Transform child in furnitureParent) Destroy(child.gameObject);
    }

    // 3. 화면에 생성 (비주얼 작업)
    foreach (var interior in interiorList)
    {
        GameObject targetPrefab = allFurniturePrefabs.Find(x => x.name == interior.itemName);

        if (targetPrefab == null) continue;

        Vector3 spawnPos = GridToWorld(interior.gridNumber);
        GameObject obj = Instantiate(targetPrefab, spawnPos, Quaternion.identity, furnitureParent);
        obj.name = interior.itemName;

        if (obj.TryGetComponent<WR_StorageController>(out var script))
        {
            script.myStorageID = interior.ID; 
        }

        if (obj.TryGetComponent<EmployeeController>(out var empScript))
        {
            empScript.myWorkerID = interior.ID; 
        }

        var so = gameData.Inventory.GetRoomInteriorItemSO(interior.itemName);
        int itemWidth = so != null ? so.itemWidth : 1; 
        int itemHeight = so != null ? so.itemHeight : 1;

        MarkGridOccupancy(interior.gridNumber, itemWidth, itemHeight, interior.ID);
    }
    
    
    Debug.Log($"[Load] 가구 {currentPlacedList.Count}개 로드 및 리스트 등록 완료!");
}
    // =================================================================
    // 2. [설치] 새로운 가구를 하나 "딱!" 내려놓을 때 (즉시 DB 저장)
    // =================================================================
    // InteriorManager.cs

    public void InstallFurniture(int gridIndex, string itemName)
    {
        Debug.Log($"[설치 시도] 위치: {gridIndex}, 이름: {itemName}");

        // 1. 프리팹 리스트가 비어있는지 체크
        if (allFurniturePrefabs == null || allFurniturePrefabs.Count == 0)
        {
            Debug.LogError("❌ [오류] InteriorManager에 'All Furniture Prefabs' 리스트가 비어있습니다! 인스펙터를 확인하세요.");
            return;
        }

        // 2. 이름으로 프리팹 찾기
        GameObject prefab = allFurniturePrefabs.Find(x => x.name == itemName);

        if (prefab == null)
        {
            Debug.LogError($"❌ [오류] 프리팹을 찾을 수 없습니다! 찾는 이름: '{itemName}'");
            Debug.LogError("팁: 리스트에 등록된 프리팹 이름과 데이터의 철자/띄어쓰기가 정확히 일치하는지 확인하세요.");
            return;
        }

        // 3. 생성 및 배치
        int newID = ServiceLocator.Get<GameData>().Interior.AddRoomInterior(gridIndex, itemName);
        
        GameObject obj = Instantiate(prefab, GridToWorld(gridIndex), Quaternion.identity, furnitureParent);
        obj.name = itemName;

        if (obj.TryGetComponent<WR_StorageController>(out var script))
        {
            script.myStorageID = newID;
        }

        // 리스트 추가
        currentPlacedList.Add(new RoomInteriorPlaced 
        { 
            gridNumber = gridIndex, 
            itemName = itemName, 
            ID = newID 
        });

        Debug.Log($"✅ [성공] 가구 설치 완료! (ID: {newID})");
    }

    // =================================================================
    // 3. [전체 저장] 가구를 드래그해서 위치를 바꿨을 때 사용 (Snapshot)
    // =================================================================
    public void SaveAllFurniture()
    {
        List<RoomInteriorPlaced> saveList = new List<RoomInteriorPlaced>();

        foreach (Transform child in furnitureParent)
        {
            if (child.TryGetComponent<WR_StorageController>(out var script))
            {
                RoomInteriorPlaced data = new RoomInteriorPlaced();
                
                // 현재 월드 좌표 -> 그리드 번호로 변환
                data.gridNumber = WorldToGrid(child.position);
                data.itemName = child.name;
                data.ID = script.myStorageID; // ID 유지
                data.interiorType = script.myType; // 필요 시 수정

                saveList.Add(data);
            }
        }

        // DB 덮어쓰기
         // 1. DB에 있는 리스트 원본을 가져옵니다.
        var dbList = ServiceLocator.Get<GameData>().Interior.GetCurrentRoomInterior();
        
        // 2. 내용을 싹 비웁니다.
        dbList.Clear();
        
        // 3. 현재 화면에 있는 배치 정보로 채워넣습니다.
        dbList.AddRange(saveList);
        Debug.Log("전체 위치 저장 완료!");
    }

    // =================================================================
    // 1. Grid Number -> World Position (Pivot: Bottom Left 기준)
    // =================================================================
    public Vector3 GridToWorld(int gridIndex, int width = 1, int height = 1)
    {

        // 1. 인덱스 구하기
        int xIndex = gridIndex % gridWidth;
        int yIndex = gridIndex / gridWidth;

        // 8칸 * 0.7 = 5.6 -> 절반 2.8 -> 시작점은 -2.8
        float mapLeftEdge = -(gridWidth * cellSize) * 0.5f;
        float mapBottomEdge = -(gridHeight * cellSize) * 0.5f;
        
        float finalX = mapLeftEdge + (xIndex * cellSize);
        float finalY = mapBottomEdge + (yIndex * cellSize);

        // 결과:
        // Index 0 -> -2.8, -2.1 (정확히 칸의 왼쪽 아래 모서리)
        // 가구 Pivot이 Bottom Left라면, 여기서부터 그려지므로 칸 안에 쏙 들어감.

        return new Vector3(finalX, finalY, 0);
    }

    // =================================================================
    // 2. World Position -> Grid Number
    // =================================================================
    public int WorldToGrid(Vector3 worldPos)
    {
        // 1. 맵 시작점 보정 (좌표계를 0부터 시작하도록 임시 변환)
        float mapHalfWidth = (gridWidth * cellSize) * 0.5f;   // 2.8
        float mapHalfHeight = (gridHeight * cellSize) * 0.5f; // 2.1

        // -2.8 좌표가 들어오면 -> 0이 됨
        float adjustedX = worldPos.x + mapHalfWidth;
        float adjustedY = worldPos.y + mapHalfHeight;

        // 2. 인덱스 계산
        int xIndex = Mathf.FloorToInt(adjustedX / cellSize);
        int yIndex = Mathf.FloorToInt(adjustedY / cellSize);

        // 3. 범위 체크
        if (xIndex < 0 || xIndex >= gridWidth || yIndex < 0 || yIndex >= gridHeight) 
        {
            return -1;
        }

        return xIndex + (yIndex * gridWidth);
    }

    // ============인테리어 edit Mode 관련 함수들 ============
    private int myID = -1;
    // 가구를 선택했을 때 호출
    public void SelectFurniture(FurnitureMobileDrag furniture)
    {
        if (furniture.TryGetComponent<WR_StorageController>(out var script))
        {
            myID = script.myStorageID;
        }
        // 1. 이미 다른 게 선택되어 있었다면? -> 걔는 선택 해제(UI 끄기)
        if (currentSelectedFurniture != null && currentSelectedFurniture != furniture)
        {
            currentSelectedFurniture.SetHighlight(false);
        }

        // 2. 새로운 녀석 선택
        currentSelectedFurniture = furniture;
        UpdateGridHighlight(furniture.transform.position, myID, furniture.gameObject.name);
        // 3. 켜기
        if (currentSelectedFurniture != null)
        {
            currentSelectedFurniture.SetHighlight(true);
        }
    }

    // 빈 땅을 찍거나 확인 버튼을 눌렀을 때
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

        int startGridIndex = WorldToGrid(targetPos);

        if (startGridIndex == -1)
        {
            gridHighlightObj.gameObject.SetActive(false);
            return;
        }

        // 1. SO에서 이 가구가 차지하는 가로/세로 칸 수 가져오기
        var so = ServiceLocator.Get<GameData>().Inventory.GetRoomInteriorItemSO(itemName);
        int itemWidth = so != null ? so.itemWidth : 1; 
        int itemHeight = so != null ? so.itemHeight : 1;

        // 2. 하이라이트 네모의 크기(Scale)를 가구 크기에 맞게 쫙 늘려주기!
        gridHighlightObj.localScale = new Vector3(itemWidth * cellSize, itemHeight * cellSize, 1f);

        // 3. 위치 잡기: 왼쪽 아래(Bottom-Left) 기준점에서 폭과 높이의 '절반'만큼 우측 상단으로 이동
        Vector3 cellBottomLeft = GridToWorld(startGridIndex);
        gridHighlightObj.position = cellBottomLeft + new Vector3(itemWidth * cellSize * 0.5f, itemHeight * cellSize * 0.5f, 0f);

        gridHighlightObj.gameObject.SetActive(true);

        // 4. 이 가구가 차지할 '모든 칸'이 맵을 안 벗어났고 & 비어있는지 검사
        bool isInvalid = CheckIfPlacementInvalid(startGridIndex, itemWidth, itemHeight, furnitureID);

        // 5. 결과에 따라 초록색 / 빨간색 지정
        if (highlightSprite) 
        {
            highlightSprite.color = isInvalid ? colorInvalid : colorValid;
        }
    }

    // 하이라이트를 아예 끄는 함수 (드래그가 끝났을 때 사용)
    public void HideGridHighlight()
    {
        if (gridHighlightObj != null) gridHighlightObj.gameObject.SetActive(false);
    }


    // 보관함 이동
    public void RemoveFurnitureData(FurnitureMobileDrag furniture)
    {
        // 1. 리스트에서 제거 (스크립트에 ID가 있다고 가정)
        // 만약 FurnitureMobileDrag에 ID가 없다면 GetComponent로 가져와야 함
        if(furniture.TryGetComponent<WR_StorageController>(out var dataScript))
        {
             var target = currentPlacedList.Find(x => x.ID == dataScript.myStorageID);
             if (target != null) currentPlacedList.Remove(target);
        }
        
        // 2. 선택된 상태였다면 해제
        if (currentSelectedFurniture == furniture)
        {
            currentSelectedFurniture = null;
            InteractionUI.Instance.HideMenu();
        }
    }

   public void StoreSelectedFurniture()
    {
        if (currentSelectedFurniture == null) return;

        if (currentSelectedFurniture.TryGetComponent<WR_StorageController>(out var script))
        {
            int targetID = script.myStorageID;
            
            // 핵심 방어 로직: 수납장 안에 물건이 1개라도 있는지 검사!
            if (script.myStorageType == StorageUIController.StorageType.Material ||
                script.myStorageType == StorageUIController.StorageType.Blanket ||
                script.myStorageType == StorageUIController.StorageType.Snack ||
                script.myStorageType == StorageUIController.StorageType.CraftBox)
            {
                int itemCount = GetItemCountInSpecificBox(script.myStorageType, targetID);
                
                if (itemCount > 0)
                {
                    Debug.LogWarning($"🚨 [수거 불가] 상자 안에 아이템이 {itemCount}개 들어있습니다! 먼저 비워주세요.");
                    InteractionUI.Instance.HideMenu();

                    return; 
                }
            }

            string itemName = currentSelectedFurniture.gameObject.name.Replace("(Clone)", "").Trim(); 
            var gameData = ServiceLocator.Get<GameData>();

            // DB 처리 
            gameData.Interior.RemoveRoomInterior(targetID); 
            gameData.Inventory.AddRoomInteriorItem(itemName, 1); 

            var targetData = currentPlacedList.Find(x => x.ID == targetID);
            if (targetData != null) currentPlacedList.Remove(targetData);

            Destroy(currentSelectedFurniture.gameObject);
            uiManager.RefreshUI();

            Debug.Log($"✅ [수거 완료] 빈 '{itemName}' 가구를 보관함으로 넣었습니다!");

            DeselectCurrent();
        }
    }
    public void ConfirmFurnitureMove()
    {
        if (currentSelectedFurniture == null) return;

        if (currentSelectedFurniture.TryGetComponent<WR_StorageController>(out var script))
        {
            int targetID = script.myStorageID;
            
            // 1. 현재 화면 상의 위치를 그리드 번호로 변환
            int newGridIndex = WorldToGrid(currentSelectedFurniture.transform.position);

            // 2. 맵의 왼쪽/아래쪽 완전 이탈 검사 (안전장치)
            if (newGridIndex == -1)
            {
                Debug.LogWarning("맵 바깥으로는 가구를 배치할 수 없습니다!");
                // TODO: 원래 자리로 되돌리는 로직 추가 가능
                return;
            }


            string itemName = currentSelectedFurniture.gameObject.name; // 프리팹 이름
            var so = ServiceLocator.Get<GameData>().Inventory.GetRoomInteriorItemSO(itemName);
            
            int itemWidth = so != null ? so.itemWidth : 1;
            int itemHeight = so != null ? so.itemHeight : 1;

            if (CheckIfPlacementInvalid(newGridIndex, itemWidth, itemHeight, targetID))
            {
                Debug.LogWarning("여기는 가구를 놓을 수 없는 자리(맵 이탈 또는 겹침)입니다!");
                // TODO: 유저에게 겹쳤다고 UI 알림 띄우기 (또는 제자리로 튕겨내기)
                return;
            }

            var placedData = currentPlacedList.Find(x => x.ID == targetID);
            if (placedData != null)
            {
                placedData.gridNumber = newGridIndex;
            }

            // 5. DB 업데이트
            ServiceLocator.Get<GameData>().Interior.TransferRoomInterior(targetID, newGridIndex);

            Debug.Log($"✅ [이동 완료] ID: {targetID} 가구가 {newGridIndex}번 칸으로 이동 저장되었습니다!");


            uiManager.RefreshUI();

            // 6. 저장 완료되었으니 선택 해제 (테두리 끄기 등)
            DeselectCurrent();
        }
    }
    private bool CheckIfPlacementInvalid(int startIndex, int width, int height, int excludeID)
    {
        int startX = startIndex % gridWidth;
        int startY = startIndex / gridWidth;

        // 1. 맵 밖으로 삐져나가는지 검사 (벽 뚫기 방지!)
        if (startX + width > gridWidth || startY + height > gridHeight)
        {
            return true; // 빨간불!
        }

        // 2. 내가 차지할 (X, Y) 칸들을 하나씩 돌면서 다른 가구가 있는지 검사
        for (int x = 0; x < width; x++)
        {
            for (int y = 0; y < height; y++)
            {
                int checkX = startX + x;
                int checkY = startY + y;
                int checkIndex = checkY * gridWidth + checkX;

                if (IsGridOccupied(checkIndex, excludeID)) 
                {
                    return true; // 다른 가구랑 겹쳤으니 빨간불!
                }
            }
        }
        return false; // 완벽하게 깔끔한 자리!
    }

    private bool IsGridOccupied(int targetGridIndex, int excludeID = -1)
    {
        if(targetGridIndex < 0 || targetGridIndex >= gridOccupancyMap.Length) return true;

        // 해당 칸의 값 확인
        int occupyingID = gridOccupancyMap[targetGridIndex];

        return occupyingID != -1 && occupyingID != excludeID;
    }

    
    // ===============Storage 관리==================

    public WR_StorageController GetStorageBoxByID(int storageID)
    {
        // furnitureParent 자식들을 뒤져서 ID가 일치하는 녀석을 찾습니다.
        foreach (Transform child in furnitureParent)
        {
            if (child.TryGetComponent<WR_StorageController>(out var controller))
            {
                if (controller.myStorageID == storageID)
                {
                    return controller;
                }
            }
        }
        return null; // 못 찾았을 경우
    }

    public EmployeeController GetEmployeeControllerByID(int storageID)
    {
        // furnitureParent 자식들을 뒤져서 ID가 일치하는 직원 컨트롤러를 찾습니다.
        foreach (Transform child in furnitureParent)
        {
            if (child.TryGetComponent<EmployeeController>(out var empController))
            {
                if (empController.TryGetComponent<WR_StorageController>(out var storageController))
                {
                    if (storageController.myStorageID == storageID)
                    {
                        return empController;
                    }
                }
            }
        }
        return null; // 못 찾았을 경우
    }
    /// <summary>
    /// 맵에 배치된 상자 중, 원하는 타입(예: 이불장)이면서 빈자리가 있는 상자를 찾아 아이템을 넣습니다.
    /// </summary>
    /// <returns>수납 성공 여부 (모든 상자가 꽉 찼으면 false)</returns>
    public bool TryAddToAnyStorage(StorageUIController.StorageType targetType, string itemName, int amountToAdd)
    {
        // 1. 방에 배치된 모든 가구 검사
        foreach (Transform child in furnitureParent)
        {
            if (child.TryGetComponent<WR_StorageController>(out var controller))
            {
                // 2. 타입이 일치하는 수납장인지 확인
                if (controller.myStorageType == targetType)
                {
                    // 3. 해당 수납장에 수납 시도
                    if (controller.TryAddItem(itemName, amountToAdd))
                    {
                        Debug.Log($"[InteriorManager] {controller.myStorageID}번 {targetType}에 '{itemName}' 자동 수납 완료!");
                        return true; // 성공했으니 탐색 종료
                    }
                }
            }
        }

        // 4. 모든 상자를 다 뒤졌는데도 자리가 없다면
        Debug.LogWarning($"[InteriorManager] 배치된 모든 {targetType}이(가) 꽉 찼습니다!");
        return false;
    }

    // =================================================================
    // ★ 씬이 다를 때(미니게임 등) 화면의 오브젝트 없이 DB만 보고 수납하는 함수
    // =================================================================
    public bool TryAddToAnyStorage_CrossScene(StorageUIController.StorageType targetType, string itemName, int amountToAdd)
    {
        var gameData = ServiceLocator.Get<GameData>();
        var roomInteriors = gameData.Interior.GetCurrentRoomInterior(); // 현재 방에 배치된 가구 DB 리스트

        if (roomInteriors == null) return false;

        foreach (var interior in roomInteriors)
        {
            if (interior.ID != -1) // 가구(상자)라면
            {
                // 1. 프리팹 리스트에서 해당 가구의 원본을 찾아 타입을 검사합니다.
                GameObject prefab = allFurniturePrefabs.Find(x => x.name == interior.itemName);
                if (prefab != null && prefab.TryGetComponent<WR_StorageController>(out var controllerPrefab))
                {
                    // 2. 내가 찾는 타입(예: 재료함)이 맞는지 확인
                    if (controllerPrefab.myStorageType == targetType)
                    {
                        // 3. SO를 통해 이 상자의 최대 용량(칸 수) 확인
                        var boxSO = gameData.Inventory.GetRoomInteriorItemSO(interior.itemName);
                        int maxCapacity = boxSO != null ? boxSO.slotCount : 0;

                        // 4. 이 상자 안에 이미 들어있는 아이템 총합 확인 (만들어둔 헬퍼 함수 재활용)
                        int currentCount = GetItemCountInSpecificBox(targetType, interior.ID);

                        // 5. 빈자리가 있다면 DB에 직접 꽂아넣기!
                        if (currentCount < maxCapacity)
                        {

                            gameData.Inventory.AdjustMaterialCount(interior.ID, itemName, amountToAdd);
                            
                            Debug.Log($"[CrossScene] 낚시 성공! 보이지 않는 씬이지만 DB의 {interior.ID}번 {targetType}에 '{itemName}' 저장 완료!");
                            return true; // 수납 성공
                        }
                    }
                }
            }
        }

        Debug.LogWarning($"[CrossScene] 배치된 모든 {targetType}이(가) 꽉 찼거나 없습니다!");
        return false;
    }
    
    public bool ConsumeMaterialFromAnyStorage(string itemName, int countToRemove)
    {
        int remaining = countToRemove;
        var inventory = ServiceLocator.Get<GameData>().Inventory;

        // 1. 방에 배치된 모든 가구 순회 (이제 DB를 뒤질 필요 없이 화면에 있는 가구 리스트 사용)
        foreach (Transform child in furnitureParent)
        {
            if (child.TryGetComponent<WR_StorageController>(out var controller))
            {
                // 2. 재료함이거나 개인제작함인 경우만 체크
                if (controller.myStorageType == StorageUIController.StorageType.Material || 
                    controller.myStorageType == StorageUIController.StorageType.CraftBox)
                {
                    // 3. 해당 상자 안의 아이템 가져오기
                    var itemsInBox = inventory.GetMaterialItems(controller.myStorageID);
                    if (itemsInBox == null) continue;

                    // 4. 리스트 역순 순회하며 차감
                    for (int i = itemsInBox.Count - 1; i >= 0; i--)
                    {
                        if (itemsInBox[i].itemName == itemName)
                        {
                            int deductAmount = Mathf.Min(itemsInBox[i].count, remaining);
                            
                            // DB에서 수량 차감
                            inventory.AdjustMaterialCount(controller.myStorageID, itemName, -deductAmount);
                            remaining -= deductAmount;

                            // ★ 가구의 총 용량 갱신 (빈자리가 생겼으니 다시 계산)
                            controller.UpdateTotalItemCount();

                            // 다 뺐으면 성공!
                            if (remaining <= 0) return true;
                        }
                    }
                }
                
            }
        }
        // 제작 전에 검사하므로 여기까지 올 일은 없겠지만, 혹시 모를 예외 처리
        Debug.LogWarning($"[InteriorManager] {itemName} 재료가 {remaining}개 부족하여 다 소모하지 못했습니다!");
        return false;
    }

    public bool HasAnyEmptySpace(StorageUIController.StorageType type)
    {
        var gameData = ServiceLocator.Get<GameData>();

        // 1. 방에 배치된 실제 가구(프리팹)들을 순회합니다. (타입 검사를 위해)
        foreach (Transform child in furnitureParent)
        {
            if (child.TryGetComponent<WR_StorageController>(out var controller))
            {
                // 2. 내가 찾고 있는 타입(예: 이불장)이 맞는지 필터링!
                if (controller.myStorageType == type)
                {
                    // 이름 뒤에 (Clone)이 붙어있을 수 있으니 안전하게 떼고 SO 검색
                    string itemName = child.name.Replace("(Clone)", "").Trim();
                    var boxSO = gameData.Inventory.GetRoomInteriorItemSO(itemName);
                    
                    if (boxSO != null)
                    {
                        // 3. '이 상자(ID)' 안에 들어있는 아이템들의 총 개수를 구합니다.
                        int currentCount = GetItemCountInSpecificBox(type, controller.myStorageID);

                        // 4. 최대 용량보다 적게 들어있다면 빈자리가 있는 것!
                        if (currentCount < boxSO.slotCount)
                        {
                            return true; // 하나라도 여유 있는 상자를 발견하면 즉시 true 반환!
                        }
                    }
                }
            }
        }
        
        // 다 뒤졌는데도 자리가 없다면
        return false;
    }

    private int GetItemCountInSpecificBox(StorageUIController.StorageType type, int boxID)
    {
        int count = 0;
        var inventory = ServiceLocator.Get<GameData>().Inventory;

        if (type == StorageUIController.StorageType.Material || type == StorageUIController.StorageType.CraftBox)
        {
            var items = inventory.GetMaterialItems(boxID);
            // ※ 만약 용량 기준이 '칸 수'라면 count++ 로, '누적 개수'라면 count += item.count 로 쓰시면 됩니다!
            if (items != null) foreach (var item in items) count += item.count; 
        }
        else if (type == StorageUIController.StorageType.Blanket)
        {
            var items = inventory.GetBlanketsInBox(boxID);
            if (items != null) foreach (var item in items) count += item.count;
        }
        else if (type == StorageUIController.StorageType.Snack)
        {
            var items = inventory.GetSnackItems(boxID);
            if (items != null) foreach (var item in items) count += item.count;
        }
        
        return count;
    }

    /// <summary>
    /// 방 전체를 뒤져서 특정 타입(StorageType)의 특정 아이템(targetItemName) 총 개수를 반환합니다.
    /// </summary>
    public int GetTotalItemCountInRoom(StorageUIController.StorageType type, string targetItemName)
    {
        int totalCount = 0;
        var gameData = ServiceLocator.Get<GameData>();

        var roomInteriors = gameData.Interior.GetCurrentRoomInterior();

        if (roomInteriors == null || roomInteriors.Count == 0) return 0;

        foreach (var interior in roomInteriors)
        {
            if (interior.ID != -1) 
            {
                // ★ 타입에 따라 DB에서 꺼내오는 서랍(List)을 다르게 지정합니다!
                if (type == StorageUIController.StorageType.Material || type == StorageUIController.StorageType.CraftBox)
                {
                    var items = gameData.Inventory.GetMaterialItems(interior.ID);
                    if (items != null)
                    {
                        foreach (var item in items)
                            if (item.itemName == targetItemName) totalCount += item.count;
                    }
                }
                else if (type == StorageUIController.StorageType.Blanket)
                {
                    var items = gameData.Inventory.GetBlanketsInBox(interior.ID);
                    if (items != null)
                    {
                        foreach (var item in items)
                            if (item.itemName == targetItemName) totalCount += item.count;
                    }
                }
                else if (type == StorageUIController.StorageType.Snack)
                {
                    var items = gameData.Inventory.GetSnackItems(interior.ID);
                    if (items != null)
                    {
                        foreach (var item in items)
                            if (item.itemName == targetItemName) totalCount += item.count;
                    }
                }
            }
        }

        return totalCount; 
    }


    public void DragDropFurnitureFromInventory(string itemName, Vector3 dropWorldPos)
    {
        var gameData = ServiceLocator.Get<GameData>();

        int dropIndex = WorldToGrid(dropWorldPos);

        // 2. 맵 밖으로 던졌으면 무시 (-1 반환 시)
        if (dropIndex == -1)
        {
            Debug.LogWarning("🚨 [작업실] 맵 바깥으로는 가구를 놓을 수 없습니다!");
            return;
        }

        // 3. 인벤토리 SO 데이터에서 가구 크기(가로/세로 칸 수) 가져오기
        var so = gameData.Inventory.GetRoomInteriorItemSO(itemName);
        if (so == null)
        {
            Debug.LogError($"🚨 [작업실] '{itemName}'의 SO 데이터를 찾을 수 없어 크기를 알 수 없습니다!");
            return;
        }
        
        int itemWidth = so.itemWidth;
        int itemHeight = so.itemHeight;

        if (CheckIfPlacementInvalid(dropIndex, itemWidth, itemHeight, -1))
        {
            Debug.LogWarning("🚨 [작업실] 그 자리에는 이미 다른 가구가 있거나 맵을 벗어납니다!");
            // TODO: 화면 중앙에 "여긴 놓을 수 없어요!" 같은 토스트 알림 띄우기
            return;
        }


        gameData.Inventory.RemoveRoomInteriorItem(itemName, 1);

        InstallFurniture(dropIndex, itemName);

        uiManager.RefreshUI();


        Debug.Log($"✅ [작업실 설치 완료] '{itemName}' 가구가 {dropIndex}번 칸에 완벽하게 설치되었습니다!");
    }
}