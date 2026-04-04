using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum EmployeeState
{
    Idle,       // 대기 중
    Working    // 일하는 중
}

public class EmployeeController : MonoBehaviour
{
    [Header("직원 기본 정보")]
    public int myWorkerID;
    public EmployeeState currentState = EmployeeState.Idle;

    [Header("스탯 정보")]
    public int currentStamina = 100;
    public int maxStamina = 100;
    public int staminaCostPerWork = 10;
    public float workingTime = 10f;
    public string currentWorkingItem = null;

    [Header("UI 및 시스템")]
    public Image progressBar;
    public Image staminaBarImage;
    private Action currentCallback;

    [Header("체력 툴팁")]
    public GameObject staminaTooltipObj; // 껐다 켤 툴팁 전체 오브젝트 (배경 포함)
    public TextMeshProUGUI staminaTooltipText;

    private void Start()
    {
        if (staminaTooltipObj != null) staminaTooltipObj.SetActive(false);

        // 씬 진입 시 오프라인 시간 계산 및 복구
        LoadAndCalculateOfflineProgress();
    }

    public void OnPointerEnterStaminaBar()
    {
        if (staminaTooltipObj != null)
        {
            staminaTooltipObj.SetActive(true);
            staminaTooltipText.text = $"{currentStamina} / {maxStamina}";
        }
    }

    public void OnPointerExitStaminaBar()
    {
        if (staminaTooltipObj != null)
        {
            staminaTooltipObj.SetActive(false);
        }
    }

    // ==========================================================
    // 1. 작업 시작 (UI 매니저가 호출)
    // ==========================================================
    public bool StartCrafting(string itemName, Action onComplete)
    {
        if (currentState != EmployeeState.Idle)
        {
            Debug.Log("직원이 이미 일하고 있습니다!");
            return false;
        }

        if (currentStamina < staminaCostPerWork)
        {
            Debug.Log($"스태미나가 부족해서 일을 시작할 수 없습니다! {currentStamina}/{staminaCostPerWork}");
            return false;
        }

        // 상태 및 스탯 갱신
        currentState = EmployeeState.Working;
        currentWorkingItem = itemName;
        currentStamina -= staminaCostPerWork;
        currentCallback = onComplete;

        
        UpdateUI();
        
        // DB에 방금 시작함(진행도 0%)으로 저장
        SaveStateToDB(0f);

        
        
        // 작업 타이머(코루틴) 시작
        StartCoroutine(CraftingRoutine(0f));

        return true; // 수락!
    }

    // ==========================================================
    // 2. 간식 먹기 (상태 관리용 추가)
    // ==========================================================
    public void EatSnack(int recoverAmount)
    {
        currentStamina = Mathf.Min(currentStamina + recoverAmount, maxStamina);

        Debug.Log($"간식을 먹었습니다! 현재 체력: {currentStamina}");

        TutorialEventBus.Raise(TutorialID.FeedSnack);

        SaveStateToDB(0f);
        UpdateUI();
    }

    // ==========================================================
    // 3. 씬 나갈 때 / 게임 끌 때 저장
    // ==========================================================
    private void OnDestroy()
    {
        if (currentState == EmployeeState.Working)
        {
            float currentProgress = (progressBar != null) ? progressBar.fillAmount : 0f;
            SaveStateToDB(currentProgress);
        }
    }

    private void OnApplicationQuit()
    {
        if (currentState == EmployeeState.Working)
        {
            float currentProgress = (progressBar != null) ? progressBar.fillAmount : 0f;
            SaveStateToDB(currentProgress);
        }
    }

    // ==========================================================
    // 4. 안전한 DB 저장 로직 (다른 직원 데이터 보호)
    // ==========================================================
    private void SaveStateToDB(float progressToSave)
    {
        // 💡 주의: 전체 직원을 불러와서 내 데이터만 쏙 바꾸고 다시 덮어씌워야 합니다!
        var allWorkers = ServiceLocator.Get<GameData>().ShopState.GetAllWorkers() ?? new List<WorkerState>();
        var myData = allWorkers.Find(x => x.workerID == myWorkerID);

        if (myData != null)
        {
            // 기존 데이터 갱신
            myData.stamina = currentStamina;
            myData.workingItem = currentWorkingItem;
            myData.progress = progressToSave;
            myData.lastSceneTime = DateTime.UtcNow.ToString("O");
        }
        else
        {
            // 데이터가 아예 없었다면 새로 추가
            allWorkers.Add(new WorkerState()
            {
                workerID = myWorkerID,
                stamina = currentStamina,
                workingItem = currentWorkingItem,
                progress = progressToSave,
                lastSceneTime = DateTime.UtcNow.ToString("O")
            });
        }

        ServiceLocator.Get<GameData>().ShopState.SaveAllWorkers(allWorkers);
        Debug.Log($"[저장 완료] ID:{myWorkerID}, 진행도:{progressToSave * 100}%");
    }

    // ==========================================================
    // 5. 씬 진입 시 오프라인 시간 계산
    // ==========================================================
    private void LoadAndCalculateOfflineProgress()
    {
        string myName = gameObject.name.Replace("(Clone)", "").Trim();
        var mySO = ServiceLocator.Get<GameData>().Inventory.GetRoomInteriorItemSO(myName);

        if (mySO != null)
        {
            maxStamina = mySO.maxStamina;
            workingTime = mySO.workingTime; // <- 진짜 작업 시간(workingTime) 갱신이 누락된 것 같습니다!

            Debug.Log($"<color=cyan>[오프라인 1단계]</color> SO 로드 성공! ({myName}) | 최대 체력: {maxStamina}, 세팅된 작업시간(workingTime 변수): {workingTime}");
        }
        else
        {
            Debug.LogWarning($"<color=red>[오프라인 1단계 실패]</color> '{myName}'의 SO 데이터를 찾을 수 없습니다!");
        }

        currentStamina = 0;

        WorkerState worker = ServiceLocator.Get<GameData>().ShopState.GetWorkerState(myWorkerID);

        if (worker == null)
        {
            Debug.Log($"<color=yellow>[오프라인 2단계]</color> ID:{myWorkerID} 직원의 DB 기록이 없습니다. (새로 배치됨) UI 갱신 후 종료합니다.");
            UpdateUI();
            return;
        }

        currentStamina = worker.stamina;
        Debug.Log($"<color=green>[오프라인 2단계]</color> DB 기록 로드 성공! 현재 체력 복구: {currentStamina}");

        if (string.IsNullOrEmpty(worker.workingItem))
        {
            Debug.Log($"<color=yellow>[오프라인 3단계]</color> 직원이 씬을 나갈 때 쉬고 있었습니다(Idle). 오프라인 계산 없이 종료합니다.");
            UpdateUI();
            return;
        }

        // ==========================================================
        // 3단계: 일하던 중이었다면 오프라인 시간 계산 시작! (기존 로직 동일)
        // ==========================================================
        currentWorkingItem = worker.workingItem;
        currentState = EmployeeState.Working;

        Debug.Log($"<color=orange>[오프라인 3단계]</color> '{currentWorkingItem}' 제작 중이었습니다. 시간 계산을 시작합니다.");

        if (!string.IsNullOrEmpty(worker.lastSceneTime))
        {
            DateTime lastTime = DateTime.Parse(worker.lastSceneTime).ToUniversalTime();
            float passedSeconds = (float)(DateTime.UtcNow - lastTime).TotalSeconds;


            if (passedSeconds < 0)
            {
                passedSeconds = 0f;
            }

            if (workingTime <= 0)
            {
                workingTime = 1f; // 임시 
            }

            float addedProgress = passedSeconds / workingTime;
            float finalProgress = worker.progress + addedProgress;

            Debug.Log($"<color=white>[계산 결과]</color> 기존 진행도: {worker.progress * 100}% + 추가 진행도: {addedProgress * 100}% = 최종 진행도: {finalProgress * 100}%");

            if (finalProgress >= 1f)
            {
                FinishWorkImmediately();
            }
            else
            {
                float startTimer = finalProgress * workingTime;
                Debug.Log($"<color=cyan>오프라인 복구 완료:</color> 아직 덜 만들었습니다. 타이머 {startTimer}초부터 코루틴을 재시작합니다.");
                StartCoroutine(CraftingRoutine(startTimer));
            }
        }
        else
        {
            Debug.LogWarning("DB에 lastSceneTime(마지막 접속 시간)이 비어있습니다. 시간 계산을 패스합니다.");
        }

        UpdateUI();
    }

    public void UpdateUI()
    {
        staminaBarImage.fillAmount = (float)currentStamina / (float)maxStamina;
        Debug.Log($"UI 업데이트: 현재 체력 {currentStamina}/{maxStamina}");

        if (currentState == EmployeeState.Idle)
        {
            progressBar.fillAmount = 0f;
        }

        if (staminaTooltipObj != null && staminaTooltipObj.activeSelf)
        {
            staminaTooltipText.text = $"{currentStamina} / {maxStamina}";
        }
    }

    // ==========================================================
    // 6. 작업 코루틴
    // ==========================================================
    private IEnumerator CraftingRoutine(float startTimer)
    {
        float timer = startTimer;

        while (timer < workingTime)
        {
            timer += Time.deltaTime;
            if (progressBar != null)
                progressBar.fillAmount = timer / workingTime;

            yield return null;
        }

        // 일이 끝났으니 공통 완료 로직 호출!
        HandleCraftingComplete();
    }

    private void FinishWorkImmediately()
    {
        // 일이 끝났으니 공통 완료 로직 호출!
        HandleCraftingComplete();
    }

    // ==========================================================
    // 🌟 [핵심] 일이 끝났을 때 처리하는 공통 함수 (콜백 증발 대비용)
    // ==========================================================
    private void HandleCraftingComplete()
    {
        // 1. 씬 이동을 안 해서 UI 매니저와의 약속(Action)을 기억하고 있다면?
        if (currentCallback != null)
        {
            currentCallback.Invoke();
        }
        else // 2. 🚨 씬을 나갔다 와서 약속을 까먹었다면? (직접 수납!)
        {
            if (!string.IsNullOrEmpty(currentWorkingItem))
            {
                bool isAdded = false;

                // 작업실 씬인지 확인
                if (RoomInteriorManager.Instance != null)
                {
                    isAdded = RoomInteriorManager.Instance.TryAddToAnyStorage(
                        StorageUIController.StorageType.Blanket,
                        currentWorkingItem,
                        1
                    );
                }
                else
                {
                    // 다른 미니게임 씬 등이라면 CrossScene 방식 사용
                    // (※ 만약 이 함수가 없다면 위쪽 if문만 사용해도 됩니다)
                    isAdded = RoomInteriorManager.Instance.TryAddToAnyStorage_CrossScene(
                        StorageUIController.StorageType.Blanket,
                        currentWorkingItem,
                        1
                    );
                }

                if (isAdded)
                {
                    Debug.Log($"<color=green>[오프라인 보상]</color> 직원이 직접 '{currentWorkingItem}'을 이불장에 수납했습니다!");
                    
                    if (QuestManager.Instance != null)
                    {
                        QuestManager.Instance.UpdateQuestProgressByID(1);
                    }
                    
                }
                else
                {
                    Debug.LogWarning($"<color=red>[오프라인 보상 경고]</color> 이불장이 꽉 차서 '{currentWorkingItem}'을 넣지 못했습니다!");
                }
            }
        }

        // 상태 초기화 및 DB 저장
        currentWorkingItem = null;
        currentState = EmployeeState.Idle;
        currentCallback = null;

        SaveStateToDB(0f);
        UpdateUI();
    }
}