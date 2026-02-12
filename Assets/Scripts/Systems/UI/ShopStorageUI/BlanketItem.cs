using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;
using System.Reflection;

public class BlanketItem : MonoBehaviour, IPointerClickHandler
{
    public string itemName;
    public int currentAmount;  // 현재 수량
    public int max;

    public Image highlightImage; // 선택 시 보일 테두리 이미지
    public Image itemImage; // 아이템 이미지

    public TextMeshProUGUI amountText;
    public TextMeshProUGUI nameText;

    public int dataIndex;   // 배열에서의 위치 (Index)
    public int parentID;    // 이불장 ID 혹은 재고함 ID

    public System.Action<BlanketItem> OnItemSelected; // 클릭 시 매니저에 알림용

    public void SetupBlanketItem(int parentID, int index, int amount, int max)
    {
        this.parentID = parentID;
        this.dataIndex = index;
        this.itemName = name;
        this.currentAmount = amount;
        this.max = max;
        amountText.text = $"{currentAmount} / {max}";
    }
    public void SetupItem(int parentID, int index, string name, int amount, Image itemImage)
    {
        this.parentID = parentID;
        this.dataIndex = index;
        this.itemName = name;
        this.currentAmount = amount;
        this.itemImage = itemImage;
        nameText.text = itemName;
        amountText.text = amount.ToString();
    }

    public void RefreshUI(bool isBlanket)
    {
        if (isBlanket)
        {
            amountText.text = $"{currentAmount} / {max}";
        }
        else
        {
            amountText.text = currentAmount.ToString();
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        OnItemSelected?.Invoke(this);
    }

    public void SetHighlight(bool active)
    {
        highlightImage.enabled = active;
    }
}
