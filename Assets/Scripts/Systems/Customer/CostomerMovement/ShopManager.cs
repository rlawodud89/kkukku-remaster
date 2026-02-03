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
        // 씬이 '로드'될 때 실행되는 이벤트
        SceneManager.sceneLoaded += OnSceneLoaded;

        // 💡 추가: 씬이 '바뀌는 순간' 실행되는 이벤트
        SceneManager.activeSceneChanged += OnSceneChanged;
    }

    void OnDisable()
    {
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
        // 현재 로드된 씬이 "가게 씬"일 경우
        if (scene.name == shopSceneName)
        {
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
    public void SimulateOfflineProgress()
    {
        TimeSpan span = DateTime.UtcNow - lastExitTime;
        float secondsPassed = (float)span.TotalSeconds;

        for (int i = activeCustomers.Count - 1; i >= 0; i--)
        {
            var customer = activeCustomers[i];
            // 예: 쇼핑은 10초, 결제는 5초 걸린다고 가정하고 로직 처리
            // 시간 경과에 따라 상태를 넘기고, 결제가 끝나면 리스트에서 제거 & 돈 추가
            if (customer.currentState != CustomerData.State.Leaving)
            {
                // 단순화된 계산: 일정 시간 지나면 구매 완료 처리
                if (secondsPassed > 15f)
                {
                    CompletePurchase(customer);
                    activeCustomers.RemoveAt(i);
                }
            }
        }
    }

    void CompletePurchase(CustomerData data)
    {
        // 1. ShopStorageDataManager에서 현재 가게에 있는 테이블 정보 가져오기
        var allTables = ShopStorageDataManager.Instance.tableClasses;

        // 2. 이불이 하나라도 남아있는 테이블만 추려내기
        List<TableClass> availableTables = allTables.FindAll(t => t.count.Exists(c => c > 0));

        // 팔 이불이 없으면 그냥 리턴 (손님이 못 사고 감)
        if (availableTables.Count == 0)
        {
            Debug.Log("가게에 팔 이불이 없습니다!");
            return;
        }

        // 3. 랜덤으로 테이블 하나 선택
        TableClass randomTable = availableTables[UnityEngine.Random.Range(0, availableTables.Count)];

        // 4. 해당 테이블에서 재고가 있는 이불의 인덱스(몇 번째 칸인지) 찾기
        List<int> availableIndices = new List<int>();
        for (int i = 0; i < randomTable.count.Count; i++)
        {
            if (randomTable.count[i] > 0)
                availableIndices.Add(i);
        }
        int selectedIndex = availableIndices[UnityEngine.Random.Range(0, availableIndices.Count)];

        // 5. 선택된 이불 이름 가져오기
        string selectedItemName = randomTable.itemName[selectedIndex];

        
        
        
        //이름으로 SO 찾아서 가격 가져오기 ---
        int price = 0;




        // 6. 재고 차감 (ShopStorageDataManager의 함수 활용)
        ShopStorageDataManager.Instance.UpdateTableData(randomTable.tableID, selectedIndex, -1);


        ShopStorageClick[] allStorages = FindObjectsOfType<ShopStorageClick>();

        foreach (ShopStorageClick storage in allStorages)
        {
            // 방금 손님이 이불을 꺼내간 바로 그 이불장을 찾았다면
            if (storage.storageID == randomTable.tableID)
            {
                // 이불장아, 네 재고 상태를 다시 확인하고 이미지를 바꿔라! 라고 명령합니다.
                storage.UpdateSpriteState();
                break; // 찾았으니 더 이상 찾을 필요 없음
            }
        }

        // 7. 플레이어 지갑에 돈 추가 (현재 지갑 시스템에 맞게 수정해주세요)
        ServiceLocator.Get<GameData>().User.ChangeGold(price);
        Debug.Log($"오프라인 판매: {selectedItemName} 판매 완료! (+{price}G)");
    }

}
