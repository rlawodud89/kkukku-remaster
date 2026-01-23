using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;


//DontDestroyOnLoad 사용
public class ShopManager : MonoBehaviour
{
    public static ShopManager Instance;

    public List<CustomerData> activeCustomers = new List<CustomerData>();
    public List<ItemData> itemDatabase; // 전체 아이템 리스트
    private DateTime lastExitTime;
    private int totalCustomerCount = 0;

    public bool isStoreOpen = false; // 가게 오픈 상태
    public float minSpawnTime = 3f;  // 최소 대기 시간
    public float maxSpawnTime = 8f;  // 최대 대기 시간
    public int maxCustomers = 10;    // 가게 최대 수용 인원

    void Awake()
    {
        if (Instance == null) { Instance = this; DontDestroyOnLoad(gameObject); }
        else { Destroy(gameObject); }
    }

    // 씬이 로드될 때 실행되는 Start 함수 등에 추가
    void Start()
    {
        // 만약 다른 씬에서 돌아온 것이라면 시뮬레이션 실행
        SimulateOfflineProgress();
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
        // 골드 추가 로직 등
        Debug.Log("오프라인 수익 발생!");
    }


    

    // 가게 문을 열 때 호출 (유저 버튼 클릭)
   

    /*
     추가해야 할 부분: 언제 시간을 저장할 것인가?
지금 코드에는 SaveExitTime() 함수는 있지만, 언제 이 함수를 실행할지가 정해져 있지 않습니다. 유저가 버튼을 눌러 씬을 이동하거나 게임을 끌 때 이 함수가 실행되어야 오프라인 수익이 계산됩니다.
     ShopManager.Instance.SaveExitTime(); // 시간 저장
     */
}
