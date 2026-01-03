using System.Collections;
using System.Collections.Generic;
using System.Data;
using TMPro;
using UnityEngine;
using static UnityEditor.Progress;

public class BuyPopup : MonoBehaviour
{
    [Header("UI 요소")]
    public TMP_Text InfoText;

    //private ItemSO item;
    private bool isGold;
    private int count;

    /*public void SetItem(ItemSO itemSO, bool isGold, int count)
    {
        item = itemSO;
        this.isGold = isGold;
        this.count = count;

        // InfoText 변화 필요
    }*/

    public void OnClickYesBtn()
    {
        // 잔액 조회 후, 구매 작업

        //gameObject.SetActive(false);
    }

    public void OnClickNoBtn()
    {
        gameObject.SetActive(false);
    }
}
