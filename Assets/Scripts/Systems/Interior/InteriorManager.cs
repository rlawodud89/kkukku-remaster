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
    [SerializeField] public Grid mainGrid; // ★ 인스펙터 연결 필수
    [SerializeField] private Sprite editModeOnSprite;
    [SerializeField] private Sprite editModeOffSprite;
    [SerializeField] private TextMeshProUGUI buttonText;

    [Header("Prefabs & Parents")]
    [SerializeField] private List<GameObject> allFurniturePrefabs; 
    [SerializeField] private Transform furnitureParent; 

    [Header("Grid Settings")]
    [SerializeField] private int gridWidth = 10; // 맵의 가로 칸 수 (중요!)
    [SerializeField] private float cellSize = 0.7f;
    [SerializeField] private int gridOffset = 4;
    // 현재 배치된 가구 리스트 (메모리 상 관리)
    public List<RoomInteriorPlaced> currentPlacedList = new List<RoomInteriorPlaced>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    private void Start()
    {

        // 1. [테스트용] 강제로 데이터 집어넣기
        //InjectTestData(); 

        // 2. [로드] 저장된 데이터 화면에 뿌리기
        SpawnFurniture(); 
    }

    // ★ 테스트가 끝나면 나중에 지우면 되는 함수입니다.
    
    private void InjectTestData()
    {
        var interiorDB = ServiceLocator.Get<GameData>().Interior;

        // 이미 데이터가 있으면 또 넣지 않기 (중복 방지)
        if (interiorDB.GetCurrentRoomInterior().Count > 0) 
        {
            Debug.Log("이미 데이터가 있어서 테스트 주입을 건너뜁니다.");
            return;
        }

        Debug.Log("★ 테스트 데이터 강제 주입 시작!");

        // ---------------------------------------------------------
        // 여기서 "이름"은 Inspector의 All Furniture Prefabs에 등록된
        // 프리팹 이름과 ★토씨 하나 틀리지 않고 똑같아야★ 합니다.
        // ---------------------------------------------------------

        // 1. (0,0) 위치인 0번 그리드에 가구 추가
        interiorDB.AddRoomInterior(0, "BlanketStorage"); 
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
        }
        else
        {
            if (buttonText) buttonText.text = "가구 배치 모드 종료";
            if (editModeButton && editModeOffSprite) editModeButton.image.sprite = editModeOffSprite;
            
            // ★ 편집 모드 끝날 때 전체 상태 저장 (선택 사항)
            SaveAllFurniture(); 
        }
    }

    // =================================================================
    // 1. [로드] DB에서 불러와서 배치하기
    // =================================================================
    public void SpawnFurniture()
    {
        var gameData = ServiceLocator.Get<GameData>();
        if (gameData == null) return;

        var interiorList = gameData.Interior.GetCurrentRoomInterior();
        currentPlacedList = interiorList;

        // 기존 가구 싹 지우기 (초기화)
        if (furnitureParent != null)
        {
            foreach (Transform child in furnitureParent) Destroy(child.gameObject);
        }

        foreach (var interior in interiorList)
        {
            GameObject targetPrefab = allFurniturePrefabs.Find(x => x.name == interior.itemName);

            if (targetPrefab == null)
            {
                Debug.LogError($"프리팹 없음: {interior.itemName}");
                continue;
            }

            // ★ 헬퍼 함수 사용 (좌표 변환)
            Vector3 spawnPos = GridToWorld(interior.gridNumber);

            GameObject obj = Instantiate(targetPrefab, spawnPos, Quaternion.identity, furnitureParent);
            obj.name = interior.itemName; // 이름 깔끔하게

            // ID 주입
            if (obj.TryGetComponent<WR_StorageClick>(out var script))
            {
                script.myStorageID = interior.ID; 
            }
        }
    }

    // =================================================================
    // 2. [설치] 새로운 가구를 하나 "딱!" 내려놓을 때 (즉시 DB 저장)
    // =================================================================
    public void InstallFurniture(int gridIndex, string itemName)
    {
        // A. DB에 추가하고 ID 발급받기
        int newID = ServiceLocator.Get<GameData>().Interior.AddRoomInterior(gridIndex, itemName);

        // B. 프리팹 찾기
        GameObject prefab = allFurniturePrefabs.Find(x => x.name == itemName);
        if (prefab == null) return;

        // C. 생성 (헬퍼 함수 사용)
        GameObject obj = Instantiate(prefab, GridToWorld(gridIndex), Quaternion.identity, furnitureParent);
        obj.name = itemName;

        // D. ID 주입
        if (obj.TryGetComponent<WR_StorageClick>(out var script))
        {
            script.myStorageID = newID;
        }
        
        // E. 현재 리스트에도 추가 (메모리 동기화)
        // (이걸 안 하면 재시작 전까지 currentPlacedList랑 화면이랑 달라짐)
        currentPlacedList.Add(new RoomInteriorPlaced 
        { 
            gridNumber = gridIndex, 
            itemName = itemName, 
            ID = newID 
        });

        Debug.Log($"배치 완료! ID: {newID}");
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
    // ★ [헬퍼 함수] 좌표 변환 로직 (Grid <-> World)
    // =================================================================
    
    // 그리드 번호(int) -> 월드 좌표(Vector3)
    public Vector3 GridToWorld(int gridIndex)
    {
        // 1. 행/열 계산 (정수)
        int xIndex = gridIndex % gridWidth;
        int zIndex = gridIndex / gridWidth;

        int x = xIndex - gridOffset;
        int z = zIndex - gridOffset;

        // 2. 스케일 적용 (0.7 곱하기)

        float worldX = x * cellSize;
        float worldZ = z * cellSize; // 2D게임이면 worldY로 변경

        // (옵션) 가구의 중심이 칸의 가운데 오게 하려면 반 칸(cellSize * 0.5) 더하기
        //float halfSize = cellSize * 0.5f; 
        
        return new Vector3(worldX, 0, worldZ);
    }

    // 월드 좌표(Vector3) -> 그리드 번호(int)
    public int WorldToGrid(Vector3 worldPos)
    {
        // 0.7로 나누고 내림(Floor) 하여 인덱스 구하기
        int x = Mathf.FloorToInt(worldPos.x / cellSize);
        int z = Mathf.FloorToInt(worldPos.z / cellSize); // 2D면 worldPos.y

        int xIndex = x + gridOffset;
        int zIndex = z + gridOffset;

        // 맵 범위 밖 클릭 방지 (안전장치)
        if (xIndex < 0 || xIndex > gridWidth || zIndex < 0) return -1;

        // 인덱스로 합치기
        return xIndex + (zIndex * gridWidth);
    }

}