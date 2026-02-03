using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UI;

public class CountableItemPanel : MonoBehaviour
{
    [Header("UI 요소")]
    public Image itemImg;
    public TMP_Text nameText;
    public Image priceImg;
    public TMP_Text priceText;
    public TMP_Text countText;

    [Header("구매 팝업창 (코드에서 연결)")]
    public BuyPopup buyPopup;

    [Header("일반 재화, 월석 사진")]
    public Sprite goldSprite;
    public Sprite moonrockSprite;

    private IStoreItemProvider storeItemProvider;
    private string itemName;
    private Sprite itemSprite;
    private int price;
    private int count;


    void Start()
    {
        count = 1;
        countText.text = count.ToString();
    }


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


    public void OnClickPlusBtn()
    {
        count++;
        countText.text = count.ToString();
    }

    public void OnClickMinusBtn()
    {
        if (count != 1)
        {
            count--;
            countText.text = count.ToString();
        }
    }

    public void OnClickBuyBtn()
    {
        buyPopup.SetItem(storeItemProvider, itemName, price, count);
        buyPopup.gameObject.SetActive(true);
    }
}
