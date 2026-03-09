using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System; // Action 사용을 위해 필수!

public class EmployeeController : MonoBehaviour
{
    [Header("---- 직원 UI ----")]
    public Image progressBar; 
    public Image staminaBar;


    private float workingTime; 
    private int maxStamina;
    public int currentStamina;
    public int staminaCostPerWork = 10; 
    public bool IsWorking { get; private set; } = false;

    private void Start()
    {
        GetInfoFromSO();

        currentStamina -= 30; // 테스트용으로 체력 좀 깎아봅시다!
        UpdateStaminaUI();
    }

    private void GetInfoFromSO()
    {
        string myName = gameObject.name; 
        
        var employeeData = ServiceLocator.Get<GameData>().Inventory.GetRoomInteriorItemSO(myName);
        
        if (employeeData != null)
        {
            workingTime = employeeData.workingTime;
            maxStamina = employeeData.maxStamina;
            currentStamina = maxStamina; // 최대 체력으로 초기화
        }
        else
        {
            // 데이터가 없으면 기본값 2초로 세팅하는 안전장치
            workingTime = 2.0f; 
            Debug.LogWarning($"[EmployeeController] '{myName}' 직원 데이터를 찾을 수 없어 작업 시간을 기본값(2초)으로 설정합니다.");
        }
    }

    // ★ Action 매개변수 추가: "일 끝나면 이거(onComplete) 실행해줄게!"
    public void AssignWork(Action onComplete)
    {
        if (IsWorking) return;
        StartCoroutine(WorkRoutine(onComplete));
    }

    private IEnumerator WorkRoutine(Action onComplete)
    {
        IsWorking = true; 

        currentStamina -= staminaCostPerWork;
        UpdateStaminaUI();

        if (progressBar != null)
        {
            progressBar.gameObject.SetActive(true);
            progressBar.fillAmount = 0f;
        }

        float timer = 0f;
        while (timer < workingTime)
        {
            timer += Time.deltaTime;
            if (progressBar != null)
                progressBar.fillAmount = timer / workingTime;
            
            yield return null;
        }

        if (progressBar != null)
        {
            progressBar.fillAmount = 1f;
            yield return new WaitForSeconds(0.5f); 
            progressBar.gameObject.SetActive(false); 
        }

        // ★ 일이 끝났으니 넘겨받은 약속(함수)을 실행합니다!
        onComplete?.Invoke();

        IsWorking = false; 
    }

    // 간식

    public void EatSnack(int inventoryID, string SnackName, int recoverAmount)
    {
        currentStamina += recoverAmount;
        
        // 최대 체력을 넘지 않도록 방어
        if (currentStamina > maxStamina) 
        {
            currentStamina = maxStamina;
        }

        UpdateStaminaUI();

        // 1. DB(인벤토리)에서 간식 1개 차감
        ServiceLocator.Get<GameData>().Inventory.AdjustSnackCount(inventoryID, SnackName, -1);
        
        // =========================================================
        // ★ 추가된 부분: 상자 용량 갱신 & UI 새로고침
        // =========================================================
        
        // 2. 맵에 배치된 실제 간식 상자의 용량(totalItemCount) 갱신
        if (RoomInteriorManager.Instance != null)
        {
            var targetBox = RoomInteriorManager.Instance.GetStorageBoxByID(inventoryID);
            if (targetBox != null)
            {
                targetBox.UpdateTotalItemCount(); // 상자에 1칸 빈자리가 생김!
            }
        }

        // 3. 현재 열려있는 간식 보관함 UI 즉시 새로고침
        // (RefreshCurrentPopup을 부르면 현재 줄어든 개수로 슬롯을 다시 그려줍니다. 0개가 되면 알아서 슬롯이 사라집니다!)
        if (StorageUIController.Instance != null && StorageUIController.Instance.IsPopupOpen)
        {
            StorageUIController.Instance.RefreshCurrentPopup();
        }

        Debug.Log($"{gameObject.name}이(가) 간식을 먹고 체력을 {recoverAmount} 회복했습니다! (현재: {currentStamina}/{maxStamina})");
    }

    private void UpdateStaminaUI()
    {
        if (staminaBar != null)
        {
            staminaBar.fillAmount = (float)currentStamina / maxStamina;
            Debug.Log($"[EmployeeController] {gameObject.name} 스태미나 UI 업데이트: {currentStamina}/{maxStamina}");
        }
    }
}