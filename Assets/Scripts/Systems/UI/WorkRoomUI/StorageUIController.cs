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

    [Header("---- 슬롯 생성 위치 (Content) ----")]
    [SerializeField] private Transform blanketContent;
    [SerializeField] private Transform materialContent;
    [SerializeField] private Transform snackContent;
    [SerializeField] private Transform craftingMaterialContent;

    [Header("---- 프리팹 ----")]
    [SerializeField] private GameObject blanketSlotPrefab;
    [SerializeField] private GameObject materialSlotPrefab;
    [SerializeField] private GameObject snackSlotPrefab;
    [SerializeField] private GameObject craftingMaterialSlotPrefab;

    [Header("---- 배경 블로커 ----")]
    [SerializeField] private GameObject blockerObj;

    public bool IsPopupOpen { get; private set; } = false;
    public enum StorageType { Blanket, Material, Snack, CraftBox }
    
    // 제작으로 인해 display 업데이트
    private int currentOpenBoxID = -1;
    private StorageType currentStorageType;

    public class SimpleItemData
    {
        public string name;
        public int count;
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
    }

    public void OnclickExitBtn()
    {
        CloseAllPanels();
    }

    // ★ 수정: int storageID 파라미터 추가
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
                // ★ 여기서 넘겨받은 storageID를 사용합니다.
                slot.SetData(storageID, itemName, count, icon);
            }
            else
            {
                // SnackSlotUI 등의 예외 처리
                var snackSlot = go.GetComponent<SnackSlotUI>();
                if (snackSlot != null) snackSlot.SetSlotData(null, count, 10);
            }

            go.transform.localScale = Vector3.one;
            go.transform.localPosition = Vector3.zero;
        }

        // 레이아웃 갱신
        if (content.GetComponent<RectTransform>() != null)
            LayoutRebuilder.ForceRebuildLayoutImmediate(content.GetComponent<RectTransform>());
    }

    private Sprite GetIconSprite(StorageType type, string name)
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

        return null;
    }
}