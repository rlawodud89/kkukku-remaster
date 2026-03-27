using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using TMPro;

public class StorageUIController : MonoBehaviour
{
    public static StorageUIController Instance;

    [Header("---- UI 패널 연결 ----")]
    [SerializeField] private GameObject blanketPanel;
    [SerializeField] private GameObject materialPanel;
    [SerializeField] private GameObject snackPanel;
    [SerializeField] private GameObject craftBoxPanel;
    [SerializeField] private GameObject EmployeePanel;

    [Header("---- 슬롯 생성 위치 (Content) ----")]
    [SerializeField] private Transform blanketContent;
    [SerializeField] private Transform materialContent;
    [SerializeField] private Transform snackContent;
    [SerializeField] private Transform craftingMaterialContent;
    [SerializeField] private Transform employeeContent;

    [Header("---- 프리팹 ----")]
    [SerializeField] private GameObject blanketSlotPrefab;
    [SerializeField] private GameObject materialSlotPrefab;
    [SerializeField] private GameObject snackSlotPrefab;
    [SerializeField] private GameObject craftingMaterialSlotPrefab;
    [SerializeField] private GameObject employeeSlotPrefab;
    
    [Header("---- 용량 표시 텍스트 ----")]
    [SerializeField] private TextMeshProUGUI blanketCapacityText;
    [SerializeField] private TextMeshProUGUI materialCapacityText;
    [SerializeField] private TextMeshProUGUI snackCapacityText;
    
    [Header("---- 배경 블로커 ----")]
    [SerializeField] private GameObject blockerObj;

    [Header("---- 오브젝트 풀링 관리 ----")]
    private List<GameObject> slotPool = new List<GameObject>();
    
    public bool IsPopupOpen { get; private set; } = false;
    public enum StorageType { Blanket, Material, Snack, CraftBox, Employee, None }
    
    // 제작으로 인해 display 업데이트
    private int currentOpenBoxID = -1;
    private StorageType currentStorageType;

    public class SimpleItemData
    {
        public string name;
        public int count;
    }

    public class RecipeItemData
    {
        public string name;
        public List<RecipePair> recipe;
    }   

    private void Awake()
    {
        if (Instance == null) Instance = this;


    }

    public void OpenPopup(int id, StorageType type)
    {
        currentOpenBoxID = id;
        currentStorageType = type;

        CloseAllPanels();
        if (blockerObj && type != StorageType.Snack) blockerObj.SetActive(true);
        IsPopupOpen = true;

        List<SimpleItemData> uiList = new List<SimpleItemData>();

        switch (type)
        {
            case StorageType.Blanket:
                blanketPanel.SetActive(true);
                var bList = ServiceLocator.Get<GameData>().Inventory.GetBlanketsInBox(id);
                if (bList != null)
                    foreach (var item in bList) uiList.Add(new SimpleItemData { name = item.itemName, count = item.count });


                RefreshSlots(blanketContent, blanketSlotPrefab, uiList, type, id);
                break;

            case StorageType.Material:
                materialPanel.SetActive(true);
                var mList = ServiceLocator.Get<GameData>().Inventory.GetMaterialItems(id);
                if (mList != null)
                    foreach (var item in mList) uiList.Add(new SimpleItemData { name = item.itemName, count = item.count });



                RefreshSlots(materialContent, materialSlotPrefab, uiList, type, id);
                break;

            case StorageType.Snack:
                snackPanel.SetActive(true);
                var sList = ServiceLocator.Get<GameData>().Inventory.GetSnackItems(id);
                if (sList != null)
                    foreach (var item in sList) uiList.Add(new SimpleItemData { name = item.itemName, count = item.count });


                RefreshSlots(snackContent, snackSlotPrefab, uiList, type, id);
                break;

            case StorageType.CraftBox:
                craftBoxPanel.SetActive(true);
                var cList = ServiceLocator.Get<GameData>().Inventory.GetMaterialItems(id);
                if (cList != null)
                    foreach (var item in cList) uiList.Add(new SimpleItemData { name = item.itemName, count = item.count });


                RefreshSlots(craftingMaterialContent, craftingMaterialSlotPrefab, uiList, StorageType.Material, id);
                break;

            case StorageType.Employee:
                EmployeePanel.SetActive(true);

                var clickedBox = RoomInteriorManager.Instance.GetEmployeeControllerByID(id);
                if (clickedBox != null && clickedBox.TryGetComponent<EmployeeController>(out var empController))
                {
                    BlanketCraftController.Instance.setCurrentEmployee(empController);
                }

                var eList = ServiceLocator.Get<GameData>().BlanketCraft.GetCurrentRecipes();
                RefreshRecipeSlots(employeeContent, employeeSlotPrefab, eList);
                break;

        }
        
        
        if (type != StorageType.Employee && type !=StorageType.CraftBox) 
        {
            UpdateCapacityUI(id, type);
        }
    }


    public void RefreshCurrentPopup()
    {
        if (currentOpenBoxID != -1)
        {
            OpenPopup(currentOpenBoxID, currentStorageType);
        }
    }
    
    public void CloseAllPanels()
    {
        IsPopupOpen = false;
        if (blockerObj) blockerObj.SetActive(false);
        if (blanketPanel) blanketPanel.SetActive(false);
        if (materialPanel) materialPanel.SetActive(false);
        if (snackPanel) snackPanel.SetActive(false);
        if (craftBoxPanel) craftBoxPanel.SetActive(false);
        if (EmployeePanel) EmployeePanel.SetActive(false);
    }

    public void OnclickExitBtn()
    {
        CloseAllPanels();
    }
    

    private void RefreshSlots(Transform content, GameObject prefab, List<SimpleItemData> items, StorageType type, int storageID)
    {
        // 1. 모든 기존 슬롯을 일단 비활성화 (풀로 반납)
        foreach (var slot in slotPool)
        {
            if (slot.activeSelf) slot.SetActive(false);
        }

        if (items == null || items.Count == 0) return;

        // 2. 데이터 개수만큼 슬롯 활성화 및 세팅
        for (int i = 0; i < items.Count; i++)
        {
            GameObject go;

            // 풀에 여유가 있으면 재사용, 없으면 새로 생성
            if (i < slotPool.Count)
            {
                go = slotPool[i];
                go.SetActive(true);
            }
            else
            {
                go = Instantiate(prefab, content);
                slotPool.Add(go);
            }

            // 데이터 바인딩 로직 (기존과 동일)
            var item = items[i];
            Sprite icon = GetIconSprite(type, item.name);
        
            var slotUI = go.GetComponent<StorageSlotUI>();
            if (slotUI != null) slotUI.SetData(storageID, item.name, item.count, icon);

            // 부모 설정 (만약 다른 패널로 이동했다면)
            go.transform.SetParent(content);
            go.transform.localScale = Vector3.one;
        }

        // 3. 레이아웃 갱신 최적화 (한 프레임 뒤에 하거나 필요한 경우만)
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
    }

    private void RefreshRecipeSlots(Transform content, GameObject prefab, List<BlanketItemSO> items)
    {
        // 1. 기존 슬롯 청소
        foreach (Transform child in content) Destroy(child.gameObject);

        if (items == null || items.Count == 0) return;

        foreach (var itemSO in items)
        {
        // 엉성한 이불 등 제외 로직 (필요시)
        if (itemSO.recipe == null || itemSO.recipe.Count == 0) continue;

        GameObject go = Instantiate(prefab, content);
        var slot = go.GetComponent<RecipeUIItem>();
        
        slot.SetData(itemSO);

        go.transform.localScale = Vector3.one;
        go.transform.localPosition = Vector3.zero;
        }

    // 레이아웃 갱신
    if (content.GetComponent<RectTransform>() != null)
        LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
    }

    public Sprite GetIconSprite(StorageType type, string name)
    {
        var inventory = ServiceLocator.Get<GameData>().Inventory;

        if (type == StorageType.Blanket)
        {
            var itemSO = inventory.GetBlanketItemSO(name);
            return itemSO.image;
        }
        else if (type == StorageType.Material || type == StorageType.CraftBox)
        {
            var itemSO = inventory.GetMaterialItemSO(name);
            return itemSO.image;
        }
        else if (type == StorageType.Snack)
        {
            var itemSO = inventory.GetSnackItemSO(name);
            return itemSO.image;
        }

        return null;
    }
    
    
    private void UpdateCapacityUI(int storageID, StorageType type)
    {
        // 1. 방에 배치된 해당 상자(가구)를 찾아서 최대 용량(SO)을 가져옵니다.
        var targetBox = RoomInteriorManager.Instance.GetStorageBoxByID(storageID);
        if (targetBox == null) return;

        // 원본 이름 (예: "BlanketStorage(Clone)" -> "BlanketStorage")
        string boxName = targetBox.gameObject.name.Replace("(Clone)", "").Trim();
        var boxSO = ServiceLocator.Get<GameData>().Inventory.GetRoomInteriorItemSO(boxName);
        
        int maxCapacity = boxSO != null ? boxSO.slotCount : 0;
        int currentCount = 0;

        // 2. 상자 안에 현재 몇 개가 들어있는지 계산합니다.
        var inventory = ServiceLocator.Get<GameData>().Inventory;

        if (type == StorageType.Blanket)
        {
            var items = inventory.GetBlanketsInBox(storageID);
            if (items != null) foreach (var item in items) currentCount += item.count;
            
            if (blanketCapacityText != null) 
                blanketCapacityText.text = $"{currentCount} / {maxCapacity}";
        }
        else if (type == StorageType.Material)
        {
            var items = inventory.GetMaterialItems(storageID);
            if (items != null) foreach (var item in items) currentCount += item.count;
            
            if (type == StorageType.Material && materialCapacityText != null) 
                materialCapacityText.text = $"{currentCount} / {maxCapacity}";
        }
        else if (type == StorageType.Snack)
        {
            var items = inventory.GetSnackItems(storageID);
            if (items != null) foreach (var item in items) currentCount += item.count;
            
            if (snackCapacityText != null) 
                snackCapacityText.text = $"{currentCount} / {maxCapacity}";
        }
    }
}