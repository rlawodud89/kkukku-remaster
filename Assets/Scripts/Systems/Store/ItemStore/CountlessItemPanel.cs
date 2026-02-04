using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;
using static UnityEditor.Timeline.TimelinePlaybackControls;

public class CountlessItemPanel : MonoBehaviour
{
    [Header("UI 요소")]
    public Image itemImg;
    public TMP_Text nameText;
    public Image priceImg;
    public TMP_Text priceText;

    [Header("구매 팝업창 (코드에서 연결)")]
    public BuyPopup buyPopup;

    [Header("일반 재화, 월석 사진")]
    public Sprite goldSprite;
    public Sprite moonrockSprite;

    private IStoreItemProvider storeItemProvider;
    private string itemName;
    private Sprite itemSprite;
    private int price;
    private static int count = 1;


    public void SetItem(IStoreItemProvider storeItemProvider, string itemName, Sprite itemSprite, int price, BuyPopup buyPopup)
    {
        this.storeItemProvider = storeItemProvider;
        this.itemName = itemName;
        this.itemSprite = itemSprite;
        this.price = price;
        this.buyPopup = buyPopup;

        itemImg.sprite = itemSprite;
        nameText.text = itemName;
        priceText.text = price.ToString();
        priceImg.sprite = storeItemProvider.isGold ? goldSprite : moonrockSprite;
    }

    public void OnClickBuyBtn()
    {
        buyPopup.SetItem(storeItemProvider, itemName, price, count);
        buyPopup.gameObject.SetActive(true);
    }
}
