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

    void Start()
    {
        storeItemProvider = StoreProviderFactory.Create(storeType);

        // 스크롤뷰에 아이템 추가
        if (storeItemProvider.isCountable)
        {
            foreach (var data in storeItemProvider.LoadData())
            {
                GameObject item = Instantiate(countablePrefab, content);
                CountableItemPanel ui = item.GetComponent<CountableItemPanel>();
                ui.SetItem(data.itemName, data.itemSprite, data.price, storeItemProvider.isGold, popup);
            }
        }
        else
        {
            foreach (var data in storeItemProvider.LoadData())
            {
                GameObject item = Instantiate(countlessPrefab, content);
                CountlessItemPanel ui = item.GetComponent<CountlessItemPanel>();
                ui.SetItem(data.itemName, data.itemSprite, data.price, storeItemProvider.isGold, popup);
            }
        }


    }

}