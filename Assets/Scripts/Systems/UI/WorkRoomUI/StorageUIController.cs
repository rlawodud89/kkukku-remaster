using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

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
    [Header("---- 배경 블로커 ----")]
    [SerializeField] private GameObject blockerObj;

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
                foreach (var item in eList)
                {
                    Debug.Log(item.itemName);
                }
                RefreshRecipeSlots(employeeContent, employeeSlotPrefab, eList);
                break;

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
        // 1. 기존 슬롯 청소
        foreach (Transform child in content) Destroy(child.gameObject);

        if (items == null || items.Count == 0) return;

// 2. 루프 돌면서 생성
        foreach (var item in items)
        {
            string itemName = item.name;
            int count = item.count;

            Sprite icon = GetIconSprite(type, itemName);

            GameObject go = Instantiate(prefab, content);

            // StorageSlotUI 세팅
            var slot = go.GetComponent<StorageSlotUI>();
            if (slot != null)
            {
                slot.SetData(storageID, itemName, count, icon);
            }
            else
            {
                var snackSlot = go.GetComponent<SnackSlotUI>();
                if (snackSlot != null) 
                {
                    snackSlot.SetSlotData(storageID, itemName,icon, count, 10);
                }
            }

            go.transform.localScale = Vector3.one;
            go.transform.localPosition = Vector3.zero;
        }

        // 레이아웃 갱신
        if (content.GetComponent<RectTransform>() != null)
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
        
        if (slot != null)
        {
            slot.SetData(itemSO);
        }

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
            return itemSO != null ? itemSO.image : null;
        }
        else if (type == StorageType.Material || type == StorageType.CraftBox)
        {
            var itemSO = inventory.GetMaterialItemSO(name);
            return itemSO != null ? itemSO.image : null;
        }
        else if (type == StorageType.Snack)
        {
            var itemSO = inventory.GetSnackItemSO(name);
            return itemSO != null ? itemSO.image : null;
        }

        return null;
    }
}