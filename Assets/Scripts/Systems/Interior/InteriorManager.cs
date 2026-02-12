using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; 
using System.Linq;

public class InteriorManager : MonoBehaviour
{
    public static InteriorManager Instance;
    public bool IsEditMode { get; private set; } = false;

    [Header("UI Reference")]
    [SerializeField] private Button editModeButton;
    [SerializeField] private Button interiorStorageButton;
    [SerializeField] public Grid mainGrid; // ★ 인스펙터 연결 필수
    [SerializeField] private Sprite editModeOnSprite;
    [SerializeField] private Sprite editModeOffSprite;
    [SerializeField] private TextMeshProUGUI buttonText;

    [Header("Prefabs & Parents")]
    [SerializeField] private List<GameObject> allFurniturePrefabs; 
    [SerializeField] private Transform furnitureParent; 

   [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 8;  
    [SerializeField] private int gridHeight = 6; 
    [SerializeField] private float cellSize = 0.7f; 

    [Header("Visual Adjustment")]
    [SerializeField] private float yVisualOffset = 0.35f; // ★ 이 값을 조절해서 위치를 땡기세요!


    [Header("선택된 가구 관리")]
    public FurnitureMobileDrag currentSelectedFurniture; // 현재 선택된 녀석

    public List<RoomInteriorPlaced> currentPlacedList = new List<RoomInteriorPlaced>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    private void Start()
    {

        // 1. [테스트용] 강제로 데이터 집어넣기
        InjectTestData(); 

        // 2. [로드] 저장된 데이터 화면에 뿌리기
        SpawnFurniture(); 
    }

    // ★ 테스트가 끝나면 나중에 지우면 되는 함수입니다.
    
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
    }

    public void ToggleEditMode()
    {
        IsEditMode = !IsEditMode;
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (IsEditMode)
        {
            if (buttonText) buttonText.text = "가구 배치 모드";
            if (editModeButton && editModeOnSprite) editModeButton.image.sprite = editModeOnSprite;
            if (interiorStorageButton) interiorStorageButton.gameObject.SetActive(true);
        }
        else
        {
            if (buttonText) buttonText.text = "가구 배치 모드 종료";
            if (editModeButton && editModeOffSprite) editModeButton.image.sprite = editModeOffSprite;
            if (interiorStorageButton) interiorStorageButton.gameObject.SetActive(false);
            // ★ 편집 모드 끝날 때 전체 상태 저장 (선택 사항)
            SaveAllFurniture(); 
        }
    }

    // =================================================================
    // 1. [로드] DB에서 불러와서 배치하기
    // =================================================================
// InteriorManager.cs

// InteriorManager.cs

public void SpawnFurniture()
{
    var gameData = ServiceLocator.Get<GameData>();
    if (gameData == null) return;

    var interiorList = gameData.Interior.GetCurrentRoomInterior();
    
    // ★ 1. 매니저의 리스트를 DB 데이터와 동기화 (이게 핵심!)
    // 리스트를 새로 만들어서 DB 내용을 그대로 복사해 넣습니다.
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

        if (obj.TryGetComponent<WR_StorageClick>(out var script))
        {
            script.myStorageID = interior.ID; 
        }
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

    if (obj.TryGetComponent<WR_StorageClick>(out var script))
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
            if (child.TryGetComponent<WR_StorageClick>(out var script))
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
    
    // 가구를 선택했을 때 호출
    public void SelectFurniture(FurnitureMobileDrag furniture)
    {
        // 1. 이미 다른 게 선택되어 있었다면? -> 걔는 선택 해제(UI 끄기)
        if (currentSelectedFurniture != null && currentSelectedFurniture != furniture)
        {
            currentSelectedFurniture.SetHighlight(false);
        }

        // 2. 새로운 녀석 선택
        currentSelectedFurniture = furniture;
        
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
    }

    // 보관함 이동
    public void RemoveFurnitureData(FurnitureMobileDrag furniture)
    {
        // 1. 리스트에서 제거 (스크립트에 ID가 있다고 가정)
        // 만약 FurnitureMobileDrag에 ID가 없다면 GetComponent로 가져와야 함
        if(furniture.TryGetComponent<WR_StorageClick>(out var dataScript))
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
}