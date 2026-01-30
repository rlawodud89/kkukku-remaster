using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopStorageClick : MonoBehaviour
{
    public ShopStoragePanel storagePanel; // 패널 스크립트 직접 연결
    public int storageID;

    public void OnPointerClick(PointerEventData eventData)
    {
        // UI가 앞에 있다면(예: 이미 열린 패널 등) 뒤의 오브젝트 클릭 방지
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (storagePanel != null)
        {
            storagePanel.OpenStorage(storageID);
        }
    }
}
