using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class StoreItemLoader : MonoBehaviour
{
    [SerializeField] private StoreType storeType;
    [SerializeField] private RectTransform content;
    [SerializeField] private BuyPopup popup;

    [SerializeField] private GameObject countablePrefab;
    [SerializeField] private GameObject countlessPrefab;

    private IStoreItemProvider storeItemProvider;


    void Awake()
    {
        storeItemProvider = StoreProviderFactory.Create(storeType);
    }

    void OnEnable()
    {
        LoadItemPanel();
        // 패널 보일 때마다 현재 아이템 상황 가져와서 UI 생성
        // 레벨 변화 등의 이유로 아이템 현황 변경되어도, 그 상황 그대로 보여줌
    }


    private void LoadItemPanel()
    {
        // 기존 아이템 삭제
        foreach (Transform child in content)
            Destroy(child.gameObject);

        // 스크롤뷰에 새로운 아이템 추가
        if (storeItemProvider.isCountable)
        {
            foreach (var data in storeItemProvider.LoadItemData())
            {
                GameObject item = Instantiate(countablePrefab, content);
                CountableItemPanel ui = item.GetComponent<CountableItemPanel>();
                ui.SetItem(storeItemProvider, data.itemName, data.itemSprite, data.price, popup);
            }
        }
        else
        {
            foreach (var data in storeItemProvider.LoadItemData())
            {
                GameObject item = Instantiate(countlessPrefab, content);
                CountlessItemPanel ui = item.GetComponent<CountlessItemPanel>();
                ui.SetItem(storeItemProvider, data.itemName, data.itemSprite, data.price, popup);
            }
        }
    }

}