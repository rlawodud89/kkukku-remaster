using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;
using System;
using System.Reflection;

public class RoomBlanketItem : MonoBehaviour, IPointerClickHandler
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

    public System.Action<RoomBlanketItem> OnItemSelected; // 클릭 시 매니저에 알림용

    public void SetupItem(int parentID, int index, string name, int amount, Sprite spriteImg)
    {
        this.parentID = parentID;
        this.dataIndex = index;
        this.itemName = name;
        this.currentAmount = amount;

        // 💡 Image 컴포넌트에서 뽑아오는게 아니라, 전달받은 Sprite를 바로 내 Image에 넣습니다.
        if (spriteImg != null && this.itemImage != null)
        {
            this.itemImage.sprite = spriteImg;
        }

        nameText.text = itemName;
        amountText.text = amount.ToString();
    }

    // (추가) 오른쪽 패널에 '판매대'를 표시하기 위한 전용 함수
    public void SetupTableItem(int tableID, int index, int currentTableAmount)
    {
        this.parentID = tableID;
        this.dataIndex = index;
        this.itemName = $"판매대 {tableID}";

        if (nameText != null) nameText.text = this.itemName;

        if (amountText != null) amountText.text = $"수량: {currentTableAmount}";
    }

    public void RefreshUI(bool isBlanket)
    {
        if (isBlanket)
        {
            amountText.text = currentAmount.ToString();
        }
        else
        {
            amountText.text = $"{currentAmount} / {max}";
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        Debug.Log($"클릭됨: {itemName}"); // 이 로그가 뜨는지 확인!
        OnItemSelected?.Invoke(this);
    }

    public void SetHighlight(bool active)
    {
        if (highlightImage != null)
        {
            highlightImage.gameObject.SetActive(active);
        }
    }
}
