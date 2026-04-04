using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    public List<CustomerData> activeCustomers = new List<CustomerData>();
    private DateTime lastExitTime;

    // ID 관리를 위해 누적 카운트는 유지
    private int totalCustomerCount = 0;

    public bool isStoreOpen = false;
    public float minSpawnTime = 3f;
    public float maxSpawnTime = 8f;
    public int maxCustomers = 5;
    public int spawnInitCustomers = 3;

    [Header("Simulation Settings")]
    public float averageShoppingTime = 20f; // 손님 1명이 물건 사고 나가는 평균 시간 (초)
    public float offlineEfficiency = 0.4f;  // 💡 [추가] 오프라인 판매 효율 (0.4 = 실제 플레이의 40% 속도로 팔림)

    [Header("Scene Settings")]
    public string shopSceneName;

    private float exitGameTime;

    [HideInInspector] public int pendingOfflineGold = 0;
    [HideInInspector] public int pendingOfflineSoldCount = 0;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }

        LoadExitTime();
    }

    void OnEnable()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    void OnDisable()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == shopSceneName)
        {
            isStoreOpen = ServiceLocator.Get<GameData>().User.GetIsOpen();

            // 1. 오프라인 시간 동안의 판매 시뮬레이션 돌리기
            SimulateOfflineProgress();

            // 정산 직후, 현재 게임 시간이 밤(Night)이라면 가게를 강제로 닫음 처리
            if (isStoreOpen && GameManager.Instance != null && GameManager.Instance.currentPhase == DayPhase.Night)
            {
                isStoreOpen = false;
                ServiceLocator.Get<GameData>().User.SetIsOpen(false);
                Debug.Log("[ShopManager] 밤이 되어 자동으로 가게 문이 닫혔습니다.");
            }

            // 2. 시뮬레이션 끝난 후 시각적 갱신 (가구 비우기 등)
            RefreshAllFurnitureVisuals();

            // 3. (중요) 기존에 멈춰있던 손님 데이터는 시뮬레이션으로 모두 처리되었다고 가정하고 초기화
            activeCustomers.Clear();

            // 4. 가게가 텅 비어있으면 어색하니까, 적당히 몇 명 생성해두기 (생존자 연출)
            if (isStoreOpen)
            {
                SpawnInitialSurvivors();
                StartCoroutine(SpawnCustomerRoutine());
            }
        }
        else
        {
            SaveExitTime();
            StopAllCoroutines();
        }
    }

    private void SpawnInitialSurvivors()
    {
        int initialCount = UnityEngine.Random.Range(1, spawnInitCustomers + 1);
        for (int i = 0; i < initialCount; i++)
        {
            CreateCustomer(isSurvivor: true);
        }
    }

    public void ToggleStoreOpen()
    {
        isStoreOpen = !isStoreOpen;
        ServiceLocator.Get<GameData>().User.SetIsOpen(isStoreOpen);

        if (isStoreOpen)
        {
            StartCoroutine(SpawnCustomerRoutine());
            TutorialEventBus.Raise(TutorialID.ShopOpen);
        }
        else
        {
            StopAllCoroutines();
            TutorialEventBus.Raise(TutorialID.ShopClose);
        }
    }

    IEnumerator SpawnCustomerRoutine()
    {
        while (isStoreOpen)
        {
            if (activeCustomers.Count < maxCustomers)
            {
                float waitTime = UnityEngine.Random.Range(minSpawnTime, maxSpawnTime);
                yield return new WaitForSeconds(waitTime);
                CreateCustomer(false);
            }
            yield return null;
        }
    }

    private void CreateCustomer(bool isSurvivor)
    {
        totalCustomerCount++;
        CustomerData newGuest = new CustomerData(totalCustomerCount);
        newGuest.isSurvivor = isSurvivor;

        // 생존자라면 무조건 고민(Deciding)부터 하게 하여 재고 확인 유도
        if (isSurvivor)
        {
            newGuest.currentState = CustomerData.State.Deciding;
        }

        activeCustomers.Add(newGuest);
        NPCSpawner.Instance?.SpawnNPC(newGuest);
    }

    public void LoadExitTime()
    {
        if (PlayerPrefs.HasKey("LastExitTime"))
        {
            if (DateTime.TryParse(PlayerPrefs.GetString("LastExitTime"), out DateTime savedTime))
                lastExitTime = savedTime;

            exitGameTime = PlayerPrefs.GetFloat("LastExitGameTime", 0f);
        }

        // 대기열 돈 불러오기
        pendingOfflineGold = PlayerPrefs.GetInt("PendingOfflineGold", 0);
        pendingOfflineSoldCount = PlayerPrefs.GetInt("PendingOfflineSoldCount", 0);
    }

    public void SaveExitTime()
    {
        lastExitTime = DateTime.UtcNow;
        PlayerPrefs.SetString("LastExitTime", lastExitTime.ToString("o"));

        if (GameManager.Instance != null)
        {
            exitGameTime = GameManager.Instance.gameTime;
            PlayerPrefs.SetFloat("LastExitGameTime", exitGameTime);
        }
        PlayerPrefs.Save();
    }

    // =========================================================
    // 💡 오프라인 계산 로직 개선
    // =========================================================
    public void SimulateOfflineProgress()
    {
        int totalEarned = pendingOfflineGold;
        int soldCount = pendingOfflineSoldCount;

        // 정산 대기열 싹 비우기 (중복 방지)
        pendingOfflineGold = 0;
        pendingOfflineSoldCount = 0;
        PlayerPrefs.SetInt("PendingOfflineGold", 0);
        PlayerPrefs.SetInt("PendingOfflineSoldCount", 0);

        if (!isStoreOpen)
        {
            if (soldCount > 0) StartCoroutine(GiveOfflineRewardDelayed(totalEarned, soldCount, "밀린 결제 정산"));
            SaveExitTime();
            return;
        }

        if (lastExitTime == default(DateTime))
        {
            if (soldCount > 0) StartCoroutine(GiveOfflineRewardDelayed(totalEarned, soldCount, "초기 밀린 결제"));
            return;
        }

        TimeSpan span = DateTime.UtcNow - lastExitTime;
        float offlineSeconds = (float)span.TotalSeconds;

        // 5초 미만으로 짧게 나갔다 온 경우 빠른 정산
        if (offlineSeconds < 5f)
        {
            if (soldCount > 0) StartCoroutine(GiveOfflineRewardDelayed(totalEarned, soldCount, "가상 결제 빠른 정산"));
            return;
        }

        if (GameManager.Instance != null)
        {
            float daySecondsAtExit = exitGameTime % 86400f;
            float nightStartSeconds = 21f * 3600f; // 밤이 시작되는 시점

            if (daySecondsAtExit < nightStartSeconds)
            {
                float inGameSecondsUntilNight = nightStartSeconds - daySecondsAtExit;
                float realSecondsUntilNight = inGameSecondsUntilNight / GameManager.Instance.timeScale;

                if (offlineSeconds > realSecondsUntilNight)
                {
                    offlineSeconds = realSecondsUntilNight;
                }
            }
            else
            {
                if (soldCount > 0) StartCoroutine(GiveOfflineRewardDelayed(totalEarned, soldCount, "야간 결제 이관"));
                SaveExitTime();
                return;
            }
        }

        if (offlineSeconds > 86400f * 3) offlineSeconds = 86400f * 3; // 최대 3일치만 계산

        // 💡 [핵심 수정] 예상 방문객을 계산할 때 '오프라인 효율(offlineEfficiency)'을 곱해서 판매 속도를 낮춥니다.
        int potentialVisitorCount = (int)((offlineSeconds * offlineEfficiency) / averageShoppingTime);

        Debug.Log($"============== [오프라인 정산 시작] ==============");
        Debug.Log($"부재 시간: {offlineSeconds:F0}초 | 오프라인 효율: {offlineEfficiency * 100}% | 예상 방문객: {potentialVisitorCount}명");

        for (int i = 0; i < potentialVisitorCount; i++)
        {
            if (TrySellRandomItem(out int price))
            {
                totalEarned += price;
                soldCount++;
            }
            else break; // 재고 소진
        }

        if (soldCount > 0)
        {
            StartCoroutine(GiveOfflineRewardDelayed(totalEarned, soldCount, "오프라인 누적 정산"));
        }

        SaveExitTime();
    }

    // 💡 [핵심 수정] 씬이 켜지자마자 몰래 돈을 넣지 않고, 1초 뒤에 눈에 띄게 넣어줍니다.
    IEnumerator GiveOfflineRewardDelayed(int gold, int count, string reason)
    {
        // 씬 로딩, 페이드 인, UI 초기화가 모두 끝날 때까지 여유롭게 1.5초 대기
        yield return new WaitForSeconds(1.5f);

        GameManager.Instance.ChangeGold(gold);
        Debug.Log($"💰 [{reason}] {count}개 판매, 총 {gold}G 획득!");

        // 나중에 UI 연결이 필요하시면 여기에 팝업 호출 코드를 넣으시면 완벽합니다!
        // UIManager.Instance.ShowMessage($"부재중 정산으로 {gold}G를 벌었습니다!");
    }

    bool TrySellRandomItem(out int price)
    {
        price = 0;
        var allTables = ShopStorageDataManager.Instance.interiorData.Table;
        List<int> validTableIDs = new List<int>();

        foreach (var table in allTables)
        {
            if (ShopStorageDataManager.Instance.GetTableClass(table.ID, out var tableData))
            {
                if (tableData.count.Exists(c => c > 0)) validTableIDs.Add(table.ID);
            }
        }

        if (validTableIDs.Count == 0) return false;

        int randomTableID = validTableIDs[UnityEngine.Random.Range(0, validTableIDs.Count)];

        if (ShopStorageDataManager.Instance.GetTableClass(randomTableID, out var targetTable))
        {
            List<int> stockIndices = new List<int>();
            for (int i = 0; i < targetTable.count.Count; i++)
            {
                if (targetTable.count[i] > 0) stockIndices.Add(i);
            }

            if (stockIndices.Count > 0)
            {
                int itemIndex = stockIndices[UnityEngine.Random.Range(0, stockIndices.Count)];
                string itemName = targetTable.itemName[itemIndex];

                price = ServiceLocator.Get<GameData>().Inventory.GetBlanketPrice(itemName);
                ShopStorageDataManager.Instance.UpdateTableData(randomTableID, itemIndex, -1);

                return true;
            }
        }
        return false;
    }

    void RefreshAllFurnitureVisuals()
    {
        var allStorages = FindObjectsOfType<ShopStorageClick>();
        foreach (var storage in allStorages)
        {
            storage.UpdateSpriteState();
        }
    }
}
