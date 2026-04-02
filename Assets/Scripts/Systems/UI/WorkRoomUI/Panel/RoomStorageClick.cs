using UnityEngine;
using UnityEngine.EventSystems;

public class RoomStorageClick : MonoBehaviour, IPointerClickHandler
{
    public RoomStoragePanel storagePanel; // 작업실 전용 패널 연결
    public int inventoryID; // 재고함 고유 ID (인스펙터에서 설정)

    void Awake()
    {
        // 씬 내에서 RoomStoragePanel을 자동으로 찾음
        if (storagePanel == null)
        {
            storagePanel = FindObjectOfType<RoomStoragePanel>(true);
        }
        if (TryGetComponent<WR_StorageController>(out var script))
        {
            inventoryID = script.myStorageID;
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        // 필요 시 작업실 내 편집 모드 확인 로직 추가
        // if (ShopInteriorManager.Instance.IsEditMode) return; 

        Debug.Log($"{inventoryID}번 재고함 클릭됨!");

        if (storagePanel != null)
        {
            UIEventManager.HideMainUI();
            storagePanel.OpenStorage(inventoryID);
        }
    }
}