using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class BlanketItem : MonoBehaviour
{
    //public FurnitureData data; // 이 아이템의 데이터
    public int currentAmount;  // 현재 수량
    public int max;
    public Image highlightImage; // 선택 시 보일 테두리 이미지

    public System.Action<BlanketItem> OnItemSelected; // 클릭 시 매니저에 알림용
    /*
    public void Setup(FurnitureData newData, int amount)
    {
        data = newData;
        currentAmount = amount;
        // 텍스트나 이미지 업데이트 로직 추가 (예: nameText.text = data.name;)
    }*/

    /*
    public void Setup(FurnitureData newData, int amount, int maxCapacity)
    {
        data = newData;
        currentAmount = amount;
        max = maxCapacity
        // 텍스트나 이미지 업데이트 로직 추가 (예: nameText.text = data.name;)
    }*/

    public void OnPointerClick(PointerEventData eventData)
    {
        OnItemSelected?.Invoke(this);
    }

    public void SetHighlight(bool active)
    {
        highlightImage.enabled = active;
    }
}
