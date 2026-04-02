using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CountlessItemPanel : MonoBehaviour
{
    [Header("UI 요소")]
    public PanelItemImg itemImg;
    public TMP_Text nameText;
    public Image priceImg;
    public TMP_Text priceText;

    [Header("구매 팝업창, 설명글 패널 (코드에서 연결)")]
    public BuyPopup buyPopup;
    public DescriptionPanel descriptionPanel;

    [Header("일반 재화, 월석 사진")]
    public Sprite goldSprite;
    public Sprite moonrockSprite;


    private IStoreItemProvider storeItemProvider;
    private string itemName;
    private Sprite itemSprite;
    private int price;
    private static int count = 1;


    public void SetItem(IStoreItemProvider storeItemProvider, string itemName, Sprite itemSprite, int price,
        BuyPopup buyPopup, DescriptionPanel descriptionPanel)
    {
        this.storeItemProvider = storeItemProvider;
        this.itemName = itemName;
        this.itemSprite = itemSprite;
        this.price = price;
        this.buyPopup = buyPopup;
        this.descriptionPanel = descriptionPanel;

        itemImg.SetItemImg(itemSprite, storeItemProvider.GetDescription(itemName), descriptionPanel);
        nameText.text = itemName;
        priceText.text = price.ToString();
        priceImg.sprite = storeItemProvider.isGold ? goldSprite : moonrockSprite;
    }

    public void OnClickBuyBtn()
    {
        buyPopup.SetItem(storeItemProvider, itemName, price, count);
        buyPopup.gameObject.SetActive(true);

        TutorialEventBus.Raise(TutorialID.ClickBuyMaterialButton);
    }

    public void EnableTutorialAnchors(bool enable)
    {
        foreach (var anchor in GetComponentsInChildren<TutorialAnchor>())
        {
            anchor.enabled = enable;
        }
    }
}
