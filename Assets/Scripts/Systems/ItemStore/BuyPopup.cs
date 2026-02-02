using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;

public class BuyPopup : MonoBehaviour
{
    [Header("UI 요소")]
    public TMP_Text InfoText;

    private IStoreItemProvider storeItemProvider;
    private string itemName;
    private int price;
    private int count;

    public void SetItem(IStoreItemProvider storeItemProvider, string itemName, int price, int count)
    {
        this.storeItemProvider = storeItemProvider;
        this.itemName = itemName;
        this.price = price;
        this.count = count;

        // InfoText 변화
        string moneyName = storeItemProvider.isGold ? "재화" : "월석";
        if (storeItemProvider.isCountable)
        {
            InfoText.text = $"{itemName} {count}개를 {moneyName} {price * count}개로\n구매하시겠습니까?";
        }
        else
        {
            InfoText.text = $"{itemName}을(를) {moneyName} {price}개로\n구매하시겠습니까?";
        }
    }

    public void OnClickYesBtn()
    {
        // 잔액 조회 후, 구매 작업
        if (storeItemProvider.isGold)
        {

        }
        else
        {

        }


        gameObject.SetActive(false);
    }

    public void OnClickNoBtn()
    {
        gameObject.SetActive(false);
    }
}
