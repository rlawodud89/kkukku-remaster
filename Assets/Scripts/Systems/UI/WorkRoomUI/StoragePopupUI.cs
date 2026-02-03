using UnityEngine;
using System.Collections.Generic;

public class StoragePopupUI : MonoBehaviour
{

    [Header("UI Objects")]
    [SerializeField] private GameObject popupPanel;   
    [SerializeField] private Transform contentParent; 

    [Header("Prefabs")]
    [SerializeField] private GameObject blanketSlotPrefab; 
    [SerializeField] private GameObject snackSlotPrefab;

    public enum StorageType
    {
        Blanket, 
        Material,
        Snack    
    }


   public void OpenPopup(int id, StorageType type)
    {
        string targetPanelName = "";

        switch (type)
        {
            case StorageType.Blanket:
                targetPanelName = "BlanketStorage_Panel";
                break;
            case StorageType.Snack:
                targetPanelName = "SnackBox_Panel";
                break;
            case StorageType.Material:
                targetPanelName = "MaterialStorage_Panel";
                break;
            default:
                Debug.LogError($"[OpenPopup] 지원하지 않는 StorageType입니다: {type}");
                return;
        }

        if (popupPanel == null || popupPanel.name != targetPanelName)
        {
            FindTargetPanel(targetPanelName);
        }
        
        if (popupPanel != null)
        {
            popupPanel.SetActive(true);
            RefreshList(id, type);
        }
        else
        {
            Debug.LogError($"{targetPanelName} 오브젝트를 씬에서 찾을 수 없습니다!");
        }
    }

    private void FindTargetPanel(string panelName)
    {
        // 1. 현재 씬에 있는 모든 루트 오브젝트를 가져와서 
        //    비활성화된 것까지 싹 다 뒤집니다.
        GameObject[] allObjects = Resources.FindObjectsOfTypeAll<GameObject>();
        
        foreach (GameObject obj in allObjects)
        {
            // 씬에 존재하는 오브젝트인지 확인 (에셋 폴더의 프리팹 제외)
            if (obj.hideFlags == HideFlags.None && obj.name == panelName)
            {
                popupPanel = obj;
                
                // 해당 패널 안에서 Content 찾기
                Transform contentTransform = obj.transform.Find("Content");
                if (contentTransform != null)
                {
                    contentParent = contentTransform;
                }
                else
                {
                    Debug.LogWarning($"{panelName} 안에서 'Content'라는 자식을 찾을 수 없습니다!");
                }
                
                Debug.Log($"[성공] {panelName}을 찾아 연결했습니다.");
                return;
            }
        }
    }

    // 기존 로직 유지
    private void RefreshList(int id, StorageType type)
    {
        // 1. 기존 목록 청소
        foreach (Transform child in contentParent)
            Destroy(child.gameObject);

        // 2. 타입에 따라 다른 로직 수행
        if (type == StorageType.Blanket)
        {
            LoadBlankets(id);
        }
        else if (type == StorageType.Snack)
        {
            LoadSnacks(id);
        }
    }

    private void LoadBlankets(int id)
    {
        // (기존 코드 유지)
        var list = ServiceLocator.Get<GameData>().Inventory.GetBlanketsInBox(id);
        if (list == null) return;

        foreach (var item in list)
        {
            GameObject go = Instantiate(blanketSlotPrefab, contentParent);
            var slot = go.GetComponent<StorageSlotUI>(); 
            if(slot != null) slot.SetData(item.count);
        }
    }

    private void LoadSnacks(int id)
    {
        // (기존 코드 유지)
        var list = ServiceLocator.Get<GameData>().Inventory.GetSnackItems(id);
        if (list == null) return;

        foreach (var item in list)
        {
            GameObject go = Instantiate(snackSlotPrefab, contentParent);
            var slot = go.GetComponent<SnackSlotUI>();
            if (slot != null)
            {
                Sprite icon = Resources.Load<Sprite>($"SnackIcons/{item.itemName}");
                int stamina = 10; 
                slot.SetSlotData(icon, item.count, stamina);
            }
        }
    }

    public void ClosePopup()
    {
        popupPanel.SetActive(false);
    }
}