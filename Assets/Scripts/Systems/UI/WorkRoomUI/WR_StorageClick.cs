using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class WR_StorageClick : MonoBehaviour
{
    public ShopStoragePanel storagePanel; // �г� ��ũ��Ʈ ���� ����
    public int storageID;

    public void OnPointerClick(PointerEventData eventData)
    {
        // UI�� �տ� �ִٸ�(��: �̹� ���� �г� ��) ���� ������Ʈ Ŭ�� ����
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (storagePanel != null)
        {
            //storagePanel.OpenStorageByID(storageID);
        }
    }
}
