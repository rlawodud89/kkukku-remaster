using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq; 

public class FishingManagerUI : MonoBehaviour
{   
    public static FishingManagerUI Instance;

    private InteriorManager interiorManager; // 수납장에 아이템 넣으려고 매니저 참조해둠

    [Header("Item Data (Drag & Drop Here)")]
    public List<MaterialItemSO> allItems = new List<MaterialItemSO>();

    [Header("Game Status")]
    public int currentFishingLevel = 1; 
    public int bonusCatchCount = 0; 

    [Header("UI Objects")]
    [SerializeField] private GameObject fishPrefab;     
    [SerializeField] private RectTransform spawnPoint;  
    [SerializeField] private RectTransform targetBar;   
    [SerializeField] private Transform fishParent;      
    public Button FishingButton;

    // =========================================================
    // ★ 추가된 획득 결과 UI 관련 변수들
    // =========================================================
    [Header("Result UI")]
    [SerializeField] private Transform caughtItemsContent;     // 스크롤 뷰의 Content
    [SerializeField] private GameObject caughtItemSlotPrefab;  // 방금 만든 CaughtFishSlotUI가 붙은 프리팹

    // "물고기 이름"을 키(Key)로 써서 개수와 UI를 추적하는 딕셔너리
    private Dictionary<string, int> caughtFishCounts = new Dictionary<string, int>();
    private Dictionary<string, CaughtFishSlotUI> caughtFishUIs = new Dictionary<string, CaughtFishSlotUI>();
    // =========================================================

    [Header("Balance")]
    [SerializeField] private float spawnInterval = 1.0f; 
    [SerializeField] private float fallSpeed = 500f;     
    [SerializeField] private float perfectDistance = 50f;
    [SerializeField] private float spawnRangeX = 300f;   

    private float timer = 0;
    private List<FallingFishUI> activeFishes = new List<FallingFishUI>();

    private void Awake()
    {
        if (Instance == null) Instance = this;
        FishingButton.image.sprite = ServiceLocator.Get<GameData>().User.GetCurrentUsedTool(ToolType.FISHING)?.image;
    }

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnFish();
            timer = 0;
        }

        if (Input.GetMouseButtonDown(0))
        {
            CheckTiming();
        }
    }


    private void Start()
    {
        // 씬에 있는 InteriorManager를 찾아와서 저장해둡니다.
        interiorManager = InteriorManager.Instance;

        if (interiorManager == null)
        {
            Debug.LogWarning("🚨 [FishingManagerUI] 씬에 InteriorManager가 없습니다! 수납장에 아이템이 들어가지 않습니다.");
        }
    }
    void SpawnFish()
    {
        if (allItems.Count == 0) return;

        MaterialItemSO selectedItem = GetItemByLevel();
        if (selectedItem == null) return;

        GameObject obj = Instantiate(fishPrefab, fishParent);
        RectTransform fishRect = obj.GetComponent<RectTransform>();
        
        float randomX = Random.Range(-spawnRangeX / 2f, spawnRangeX / 2f);
        fishRect.anchoredPosition = new Vector2(spawnPoint.anchoredPosition.x + randomX, spawnPoint.anchoredPosition.y);

        FallingFishUI fishScript = obj.GetComponent<FallingFishUI>();
        fishScript.Setup(fallSpeed, this, targetBar.anchoredPosition.y, selectedItem.image, selectedItem.itemName);
        
        activeFishes.Add(fishScript);
    }

    MaterialItemSO GetItemByLevel()
    {
        var candidates = allItems.Where(item => item.level <= currentFishingLevel).ToList();
        if (candidates.Count == 0) return null;
        int randomIndex = Random.Range(0, candidates.Count);
        return candidates[randomIndex];
    }

    void CheckTiming()
    {
        if (activeFishes.Count == 0) return;

        FallingFishUI targetFish = activeFishes[0];
        if (targetFish == null) 
        {
            activeFishes.RemoveAt(0);
            return;
        }

        RectTransform fishRect = targetFish.GetComponent<RectTransform>();
        float distance = Mathf.Abs(fishRect.anchoredPosition.y - targetBar.anchoredPosition.y);

        if (distance <= perfectDistance)
        {
            Debug.Log($"<color=cyan>Perfect!</color> ({targetFish.name})");
            CatchFish(targetFish);
        }
        else if (distance <= perfectDistance * 2.5f)
        {
            Debug.Log($"Good ({targetFish.name})");
            CatchFish(targetFish);
        }
    }

    // =========================================================
    // ★ 업그레이드된 물고기 획득 처리 함수
    // =========================================================
    [Header("Storage Box Prefabs (For DB Check)")]
    // 낚시 씬에서 상자 타입을 판별하기 위해, '수납장' 프리팹들만 여기에 끌어다 놔주세요.
    public List<GameObject> storagePrefabs = new List<GameObject>(); 


    // =========================================================
    // ★ 물고기 획득 처리 함수 (CatchFish) 수정
    // =========================================================
    void CatchFish(FallingFishUI fish)
    {
        if (fish == null) return; 

        if (activeFishes.Contains(fish))
        {
            int randomNum = Random.Range(1, bonusCatchCount + 1);
            
            // 1. InteriorManager 대신 내가 직접 DB에 꽂아넣기 시도!
            bool isSaved = TryAddFishToStorageDB(fish.itemName, randomNum);
            
            if (!isSaved)
            {
                Debug.LogWarning("🚨 방에 배치된 재료함이 없거나 꽉 찼습니다! (물고기 증발)");
                // TODO: 유저에게 수납장이 꽉 찼다는 알림 띄우기
            }

            // 2. UI 업데이트
            string fName = fish.itemName;
            if (caughtFishCounts.ContainsKey(fName))
            {
                caughtFishCounts[fName] += randomNum;
                caughtFishUIs[fName].UpdateCount(caughtFishCounts[fName]);
            }
            else
            {
                caughtFishCounts[fName] = randomNum;
                GameObject slotObj = Instantiate(caughtItemSlotPrefab, caughtItemsContent);
                CaughtFishSlotUI slotUI = slotObj.GetComponent<CaughtFishSlotUI>();
                slotUI.Setup(fish.myImage.sprite, caughtFishCounts[fName]);
                caughtFishUIs[fName] = slotUI; 
            }

            activeFishes.Remove(fish);
            Destroy(fish.gameObject);
        }
    }


    private bool TryAddFishToStorageDB(string fishName, int amount)
    {
        var gameData = ServiceLocator.Get<GameData>();
        var roomInteriors = gameData.Interior.GetCurrentRoomInterior(); 
        if (roomInteriors == null || roomInteriors.Count == 0) return false;

        foreach (var interior in roomInteriors)
        {
            if (interior.ID != -1) // 상자(가구)라면
            {
                GameObject prefab = storagePrefabs.Find(x => x.name == interior.itemName);
            

                if (prefab.TryGetComponent<WR_StorageController>(out var controllerPrefab))
                {

                    if (controllerPrefab.myStorageType == StorageUIController.StorageType.Material)
                    {
                        var boxSO = gameData.Inventory.GetRoomInteriorItemSO(interior.itemName);

                        int maxCapacity = boxSO.slotCount;
                        int currentCount = 0;
                        var items = gameData.Inventory.GetMaterialItems(interior.ID);
                        if (items != null) 
                        {
                            foreach (var item in items) currentCount += item.count;
                        }

                        if (currentCount < maxCapacity)
                        {
                            gameData.Inventory.AdjustMaterialCount(interior.ID, fishName, amount);
                            return true;
                        }
                        else
                        {
                            Debug.LogWarning($"<color=red>[디버그 7-막힘]</color> {interior.ID}번 재료함은 {maxCapacity}칸이 꽉 차서 더 넣을 수 없습니다!");
                        }
                    }
                }
            }
        }
        return false; // 넣을 재료함이 없거나 꽉 찼음
    }
    public void OnFishMiss(FallingFishUI fish)
    {
        if (activeFishes.Contains(fish))
        {
            activeFishes.Remove(fish);
        }
    }

    // 미니게임을 다시 시작할 때 UI를 싹 비우는 헬퍼 함수 (필요 시 사용!)
    public void ClearResultUI()
    {
        caughtFishCounts.Clear();
        caughtFishUIs.Clear();
        
        foreach (Transform child in caughtItemsContent)
        {
            Destroy(child.gameObject);
        }
    }
}