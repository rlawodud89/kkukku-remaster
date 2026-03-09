using UnityEngine;
using UnityEngine.EventSystems;

public class WR_StorageController : MonoBehaviour, IPointerClickHandler
{
    [Header("---- 기본 정보 (Data) ----")]
    public int myStorageID; // InteriorManager가 생성 시 주입
    public RoomInteriorType myType;
    public StorageUIController.StorageType myStorageType;

    [Header("---- 상태 및 용량 (Status) ----")]
    public int totalItemCount; // 현재 들어있는 아이템 총합
    public int maxCapacity;    // 이 상자의 최대 보관 가능 개수

    [Header("---- UI 연결 ----")]
    [SerializeField] private StorageUIController storageUIController;


    private void Start()
    {
        // 1. UI 컨트롤러 자동 연결
        if (storageUIController == null)
        {
            storageUIController = FindObjectOfType<StorageUIController>();
        }

        // 2. 이 상자의 최대 용량(SO) 가져오기
        InitMaxCapacity();

        // 3. 현재 들어있는 아이템 개수 계산
        UpdateTotalItemCount();
    }
    
    // ==========================================
    // 1. 초기화 및 상태 갱신 로직
    // ==========================================
    private void InitMaxCapacity()
    {
        RoomInteriorItemSO interiorData = null;
        var inventory = ServiceLocator.Get<GameData>().Inventory;

        switch (myStorageType) 
        {
            case StorageUIController.StorageType.Blanket:
                interiorData = inventory.GetRoomInteriorItemSO("BlanketStorage"); 
                break;

            case StorageUIController.StorageType.Material:
            case StorageUIController.StorageType.CraftBox:
                interiorData = inventory.GetRoomInteriorItemSO("PersonalCraftBox");
                break;

            case StorageUIController.StorageType.Snack:
                interiorData = inventory.GetRoomInteriorItemSO("SnackBox");
                break;

            case StorageUIController.StorageType.Employee:
                maxCapacity = 0; // 직원은 수납장이 아니므로 0
                return; // 더 이상 찾을 필요 없이 바로 함수 종료
        }


        if (interiorData != null)
        {
            maxCapacity = interiorData.slotCount; // SO에 정의해둔 변수 사용
        }
        else
        {
            maxCapacity = 0; 
        }
    }

    public void UpdateTotalItemCount()
    {
        totalItemCount = 0; // 계산 전 초기화
        var inventory = ServiceLocator.Get<GameData>().Inventory;

        switch (myStorageType)
        {
            case StorageUIController.StorageType.Blanket:
                var bList = inventory.GetBlanketsInBox(myStorageID);
                if (bList != null) 
                    foreach (var item in bList) totalItemCount += item.count;
                break;

            case StorageUIController.StorageType.Material:
            case StorageUIController.StorageType.CraftBox:
                var mList = inventory.GetMaterialItems(myStorageID);
                if (mList != null) 
                    foreach (var item in mList) totalItemCount += item.count;
                break;

            case StorageUIController.StorageType.Snack:
                var sList = inventory.GetSnackItems(myStorageID);
                if (sList != null) 
                    foreach (var item in sList) totalItemCount += item.count;
                break;
                
            case StorageUIController.StorageType.Employee:
                // 직원은 아이템 카운트에서 제외
                break;
        }
    }


    /// <summary>
    /// 외부(InteriorManager 등)에서 이 상자에 아이템을 넣으려 할 때 호출합니다.
    /// </summary>
    public bool TryAddItem(string itemName, int amountToAdd)
    {
        // 최신 상태로 한 번 더 갱신
        UpdateTotalItemCount();

        // 1. 용량 검사 (꽉 찼으면 거절!)
        if (totalItemCount + amountToAdd > maxCapacity)
        {
            return false; // 수납 실패
        }

        // 2. 수납 통과 시 DB에 추가
        var inventory = ServiceLocator.Get<GameData>().Inventory;
        
        switch (myStorageType)
        {
            case StorageUIController.StorageType.Blanket:
                inventory.AdjustBlanketCount(myStorageID, itemName, amountToAdd); 
                break;

            case StorageUIController.StorageType.Material:


            case StorageUIController.StorageType.CraftBox:
                inventory.AdjustMaterialCount(myStorageID, itemName, amountToAdd); 
                break;

            case StorageUIController.StorageType.Snack:
                // inventory.AddSnack(myStorageID, itemName, amountToAdd); // 스낵 추가 함수명
                break;
        }
        
        UpdateTotalItemCount();
        
        return true; // 수납 성공!
    }
   

    // ==========================================
    // 3. UI 상호작용 (클릭)
    // ==========================================
    public void OnPointerClick(PointerEventData eventData)
    {
        if (myStorageType==StorageUIController.StorageType.None)
        {
            return;
        }
        if (StorageUIController.Instance.IsPopupOpen) return;
        
        // 편집 모드 체크
        if (RoomInteriorManager.Instance != null && RoomInteriorManager.Instance.IsEditMode) 
        {
            return;
        }

        if (storageUIController == null)
            storageUIController = FindObjectOfType<StorageUIController>();
        
        if (storageUIController != null)
        {
            UpdateTotalItemCount(); // UI 열기 전 갱신

            storageUIController.OpenPopup(myStorageID, myStorageType);
        }
        else
        {
            Debug.LogError($"[Click Test] {gameObject.name}에 popupUI가 연결되지 않았습니다!");
        }
    }

    public void OnclickExitBtn()
    {
        if (storageUIController != null)
        {
            storageUIController.CloseAllPanels();
        }
    }
}