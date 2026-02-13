using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

//DontDestroyOnLoad 사용
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    public List<CustomerData> activeCustomers = new List<CustomerData>();
    private DateTime lastExitTime;
    private int totalCustomerCount = 0;

    public bool isStoreOpen = false; // 가게 오픈 상태
    public float minSpawnTime = 3f;  // 최소 대기 시간
    public float maxSpawnTime = 8f;  // 최대 대기 시간
    public int maxCustomers = 10;    // 가게 최대 수용 인원

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
        // 씬이 '로드'될 때 실행되는 이벤트
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 💡 추가: 씬이 '바뀌는 순간' 실행되는 이벤트
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    void OnDisable()
    {
        if (Instance != this) return;
        SceneManager.sceneLoaded -= OnSceneLoaded;
        SceneManager.activeSceneChanged -= OnSceneChanged; // 이벤트 해제
    }

    void OnSceneChanged(Scene current, Scene next)
    {
        // current: 방금 떠난 씬 / next: 새로 들어간 씬

        // 1. 방금 떠난 씬이 "가게 씬"이라면? -> 시간 저장!
        if (current.name == shopSceneName)
        {
            SaveExitTime();
            Debug.Log($"가게 씬을 떠났습니다. 현재 시간 저장됨! (이동하는 곳: {next.name})");
        }
    }


    void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        Debug.Log($"[System] 씬 로드됨: '{scene.name}' / 설정된 가게 이름: '{shopSceneName}'");

        // 현재 로드된 씬이 "가게 씬"일 경우
        if (scene.name == shopSceneName)
        {

            isStoreOpen = ServiceLocator.Get<GameData>().User.GetIsOpen();

            Debug.Log($"데이터 불러오기 완료! 현재 가게 상태: {(isStoreOpen ? "영업중" : "준비중")}");

            Debug.Log("가게 씬 진입! 오프라인 계산 및 NPC 배치 시작");
            SimulateOfflineProgress();

            // 가게에 돌아왔으니 기존 손님들을 다시 화면에 소환
            foreach (var customer in activeCustomers)
            {
                NPCSpawner.Instance?.SpawnNPC(customer);
            }

            // 문이 열려있다면 다시 손님 생성 코루틴 시작
            if (isStoreOpen)
            {
                StartCoroutine(SpawnCustomerRoutine());
            }
        }
        // "가게 씬"이 아닌 다른 씬(미니게임, 맵 등)일 경우
        else
        {
            Debug.Log($"다른 씬({scene.name}) 진입. 가게 로직 일시정지");
            StopAllCoroutines(); // 다른 씬에서는 손님이 더 이상 생성되지 않도록 중지
        }
    }

    // 가게 문을 열 때 호출 (유저 버튼 클릭)
    // 영업 상태를 켜고 끄는 스위치
    public void ToggleStoreOpen()
    {
        isStoreOpen = !isStoreOpen;

        ServiceLocator.Get<GameData>().User.SetIsOpen(isStoreOpen);

        if (isStoreOpen)
        {
            StartCoroutine(SpawnCustomerRoutine());
            Debug.Log("영업 시작!");
        }
        else
        {
            StopAllCoroutines(); // 모든 생성 루틴 중지
            Debug.Log("영업 종료!");
        }
    }

    // 영업 중일 때 주기적으로 손님을 부르는 타이머
    IEnumerator SpawnCustomerRoutine()
    {
        while (isStoreOpen)
        {
            if (activeCustomers.Count < maxCustomers)
            {
                float waitTime = UnityEngine.Random.Range(minSpawnTime, maxSpawnTime);
                yield return new WaitForSeconds(waitTime);

                // 이제 이름이 명확한 이 함수를 호출합니다.
                CreateCustomer();
            }
            yield return null;
        }
    }

    // 실제로 손님 한 명을 생성하는 함수
    private void CreateCustomer()
    {
        totalCustomerCount++;
        CustomerData newGuest = new CustomerData(totalCustomerCount);
        activeCustomers.Add(newGuest);

        // NPCSpawner에게 씬에 NPC를 만들라고 시킴
        NPCSpawner.Instance?.SpawnNPC(newGuest);
    }

    // 씬을 나갈 때 시간 저장
    public void SaveExitTime()
    {
        lastExitTime = DateTime.UtcNow;
    }

    // 씬에 다시 들어왔을 때 시뮬레이션
    // ShopManager.cs

    public void SimulateOfflineProgress()
    {
        TimeSpan span = DateTime.UtcNow - lastExitTime;
        float offlineSeconds = (float)span.TotalSeconds;
        if (offlineSeconds < 0 || offlineSeconds > 999999f) offlineSeconds = 0;

        // 🟢 [로그 시작]
        Debug.Log($"============== [오프라인 정산 시작] ==============");
        Debug.Log($"경과 시간: {offlineSeconds:F1}초");

        int totalEarned = 0;
        int soldCount = 0;
        float shoppingDuration = 10f;
        float payingDuration = 5f;

        for (int i = activeCustomers.Count - 1; i >= 0; i--)
        {
            var customer = activeCustomers[i];
            float remainTime = offlineSeconds;

            // [1] 쇼핑 단계 체크
            if (customer.currentState == CustomerData.State.MovingToWardrobe ||
                customer.currentState == CustomerData.State.Deciding)
            {
                if (remainTime >= shoppingDuration)
                {
                    customer.currentState = CustomerData.State.Paying;
                    remainTime -= shoppingDuration;
                    // Debug.Log($"손님({customer.id}): 쇼핑 완료 -> 계산 대기");
                }
                else
                {
                    customer.currentState = CustomerData.State.Deciding;
                    remainTime = 0;
                }
            }

            // [2] 계산 단계 체크
            if (customer.currentState == CustomerData.State.MovingToCashier ||
                customer.currentState == CustomerData.State.Paying)
            {
                if (remainTime >= payingDuration)
                {
                    // 판매 처리 및 로그 정보 받기
                    string itemName;
                    int price;

                    if (CompletePurchase(customer, out itemName, out price))
                    {
                        Debug.Log($"💰 [판매 성공] 손님({customer.id})에게 '{itemName}' 판매 (+{price}G)");
                        totalEarned += price;
                        soldCount++;
                    }
                    else
                    {
                        Debug.Log($"💨 [판매 실패] 손님({customer.id}): 재고가 없어서 빈손으로 퇴장");
                    }

                    activeCustomers.RemoveAt(i);
                    continue;
                }
                else
                {
                    customer.currentState = CustomerData.State.Paying;
                }
            }

            // 생존자
            customer.isSurvivor = true;
        }

        // 🟢 [최종 요약 로그]
        if (soldCount > 0)
        {
            Debug.Log($"📈 <color=yellow>총 수익: {totalEarned} G (판매: {soldCount}건)</color>");
        }
        else
        {
            Debug.Log($"💤 판매된 내역이 없습니다.");
        }
        Debug.Log($"============== [오프라인 정산 종료] ==============");
    }


    bool CompletePurchase(CustomerData data, out string item, out int price)
    {
        item = "";
        price = 0;

        var allTables = ShopStorageDataManager.Instance.tableClasses;
        List<TableClass> availableTables = allTables.FindAll(t => t.count.Exists(c => c > 0));

        // 재고 없음
        if (availableTables.Count == 0) return false;

        // 랜덤 판매 로직
        TableClass randomTable = availableTables[UnityEngine.Random.Range(0, availableTables.Count)];
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < randomTable.count.Count; i++)
        {
            if (randomTable.count[i] > 0) availableIndices.Add(i);
        }

        if (availableIndices.Count == 0) return false;

        int selectedIndex = availableIndices[UnityEngine.Random.Range(0, availableIndices.Count)];
        string selectedItemName = randomTable.itemName[selectedIndex];
        int itemPrice = ServiceLocator.Get<GameData>().Inventory.GetBlanketPrice(selectedItemName);

        // 데이터 갱신 (재고 차감)
        ShopStorageDataManager.Instance.UpdateTableData(randomTable.tableID, selectedIndex, -1);

        // 이미지 갱신
        ShopStorageClick[] allStorages = FindObjectsOfType<ShopStorageClick>();
        foreach (ShopStorageClick storage in allStorages)
        {
            if (storage.storageID == randomTable.tableID)
            {
                storage.UpdateSpriteState();
                break;
            }
        }

        // 돈 획득
        ServiceLocator.Get<GameData>().User.ChangeGold(itemPrice);

        // 🚨 [로그용 정보 전달]
        item = selectedItemName;
        price = itemPrice;

        return true;
    }


}
