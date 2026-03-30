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

    [Header("Scene Settings")]
    public string shopSceneName;

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
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
            Debug.Log($"[ShopManager] 가게 복귀. 상태: {(isStoreOpen ? "영업중" : "준비중")}");

            // 1. 오프라인 시간 동안의 판매 시뮬레이션 돌리기
            SimulateOfflineProgress();

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

    // 가게 들어왔을 때 자연스럽게 몇 명 깔아두는 코루틴
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
        newGuest.isSurvivor = isSurvivor; // NPC 생성 시 이 플래그를 보고 행동 결정

        // 생존자라면 상태를 랜덤하게 설정 (쇼핑중 or 계산중)
        if (isSurvivor)
        {
            // 70% 확률로 구경 중, 30% 확률로 계산 중
            newGuest.currentState = (UnityEngine.Random.value > 0.3f)
                ? CustomerData.State.Deciding
                : CustomerData.State.Paying;
        }

        activeCustomers.Add(newGuest);
        NPCSpawner.Instance?.SpawnNPC(newGuest);
    }

    public void SaveExitTime()
    {
        lastExitTime = DateTime.UtcNow;
    }

    // =========================================================
    // 💡 핵심 수정: 오프라인 계산 로직 개선
    // =========================================================
    public void SimulateOfflineProgress()
    {
        TimeSpan span = DateTime.UtcNow - lastExitTime;
        float offlineSeconds = (float)span.TotalSeconds;

        // 너무 짧은 시간(예: 5초 미만)은 무시
        if (offlineSeconds < 5f) return;
        if (offlineSeconds > 86400f * 3) offlineSeconds = 86400f * 3; // 최대 3일치만 계산 (오버플로우 방지)

        Debug.Log($"============== [오프라인 정산 시작] ==============");
        Debug.Log($"부재 시간: {offlineSeconds:F0}초");

        if (!isStoreOpen)
        {
            Debug.Log("가게 문을 닫아놓고 나가서 수익이 없습니다.");
            return;
        }

        // 1. 이 시간 동안 다녀갔을 '가상의 손님 수' 계산
        // (부재 시간 / 한 명당 소요 시간) * (동시 입장 가능 비율 보정 1.5배 등)
        int potentialVisitorCount = (int)(offlineSeconds / averageShoppingTime);

        Debug.Log($"예상 방문객: {potentialVisitorCount}명");

        int totalEarned = 0;
        int soldCount = 0;

        // 2. 가상의 손님들이 물건 사가는 로직 (데이터만 빠르게 처리)
        for (int i = 0; i < potentialVisitorCount; i++)
        {
            // 랜덤한 물건 하나 판매 시도
            if (TrySellRandomItem(out int price))
            {
                totalEarned += price;
                soldCount++;
            }
            else
            {
                // 재고가 다 떨어졌으면 더 이상 계산 의미 없음
                Debug.Log("재고 소진으로 오프라인 판매 조기 종료!");
                break;
            }
        }

        if (soldCount > 0)
        {
            // 한 번에 골드 추가
            GameManager.Instance.ChangeGold(totalEarned);
            Debug.Log($"💰 [정산 완료] {soldCount}개 판매, 총 {totalEarned}G 획득!");

            // (선택사항) 여기서 "부재중 수익 팝업"을 띄워주면 좋습니다.
            // UIManager.Instance.ShowOfflineRewardPopup(totalEarned, soldCount);
        }
        else
        {
            Debug.Log("💤 판매된 물건이 없습니다 (재고 부족 등).");
        }

        Debug.Log($"============== [오프라인 정산 종료] ==============");
    }

    // 데이터상으로만 판매 처리하는 함수 (비주얼 갱신 X)
    bool TrySellRandomItem(out int price)
    {
        price = 0;

        // 1. 재고가 있는 모든 테이블 찾기
        var allTables = ShopStorageDataManager.Instance.interiorData.Table;
        List<int> validTableIDs = new List<int>();

        // (최적화를 위해 ShopStorageDataManager에 '재고 있는 테이블 리스트'를 캐싱해두면 더 좋음)
        foreach (var table in allTables)
        {
            if (ShopStorageDataManager.Instance.GetTableClass(table.ID, out var tableData))
            {
                if (tableData.count.Exists(c => c > 0)) validTableIDs.Add(table.ID);
            }
        }

        if (validTableIDs.Count == 0) return false; // 팔 게 없음

        // 2. 랜덤 테이블 선택
        int randomTableID = validTableIDs[UnityEngine.Random.Range(0, validTableIDs.Count)];

        if (ShopStorageDataManager.Instance.GetTableClass(randomTableID, out var targetTable))
        {
            // 3. 그 테이블 안에서 재고 있는 아이템 찾기
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

                // 4. 재고 차감 (데이터만)
                ShopStorageDataManager.Instance.UpdateTableData(randomTableID, itemIndex, -1);

                return true;
            }
        }

        return false;
    }

    // 시뮬레이션 끝난 후 화면에 보이는 가구들 한 번에 새로고침
    void RefreshAllFurnitureVisuals()
    {
        var allStorages = FindObjectsOfType<ShopStorageClick>();
        foreach (var storage in allStorages)
        {
            storage.UpdateSpriteState();
        }
    }
}