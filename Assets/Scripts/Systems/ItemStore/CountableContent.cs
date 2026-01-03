using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class CountableContent : MonoBehaviour
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

    //private ItemSo item;
    private bool isGold;
    private int count;


    void Start()
    {
        count = 1;
        countText.text = count.ToString();
    }


    /*public void SetItem(ItemSO itemSO, bool isGold, BuyPopup buyPopup)
    {
        item = itemSO;
        ItemImg.sprite = itemSO.image;
        NameText.text = itemSO.name;
        PriceText.text = itemSO.price.ToString();

        this.isGold = isGold;
        PriceImg.sprite = isGold ? goldSprite : moonrockSprite;
        this.buyPopup = buyPopup;
    }*/


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
        //buyPopup.SetItem();
        //buyPopup.gameObject.SetActive(true);
    }
}
