using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class CountableItemPanel : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler,
    IBeginDragHandler
{
    [Header("UI 요소")]
    public Image itemImg;
    public TMP_Text nameText;
    public Image priceImg;
    public TMP_Text priceText;
    public TMP_Text countText;

    [Header("구매 팝업창 (코드에서 연결)")]
    public BuyPopup buyPopup;
    public DescriptionPanel descriptionPanel;

    [Header("일반 재화, 월석 사진")]
    public Sprite goldSprite;
    public Sprite moonrockSprite;

    [Header("설명글 누르기 시간 설정")]
    public float longPressTime = 1f;


    private IStoreItemProvider storeItemProvider;
    private string itemName;
    private Sprite itemSprite;
    private int price;
    private int count;

    private Coroutine pressCoroutine;
    private bool isPressed;


    void Start()
    {
        count = 1;
        countText.text = count.ToString();
    }


    public void SetItem(IStoreItemProvider storeItemProvider, string itemName, Sprite itemSprite, int price,
        BuyPopup buyPopup, DescriptionPanel descriptionPanel)
    {
        this.storeItemProvider = storeItemProvider;
        this.itemName = itemName;
        this.itemSprite = itemSprite;
        this.price = price;
        this.buyPopup = buyPopup;
        this.descriptionPanel = descriptionPanel;

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


    // === 설명글 패널 관련 메서드 ===

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        pressCoroutine = StartCoroutine(LongPressRoutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        CancelPress();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        CancelPress();
    }
    public void OnBeginDrag(PointerEventData eventData)
    {
        CancelPress();
    }

    private IEnumerator LongPressRoutine()
    {
        yield return new WaitForSeconds(longPressTime);

        if (isPressed)
        {
            descriptionPanel.Show(storeItemProvider.GetDescription(itemName), transform as RectTransform);
        }
    }

    private void CancelPress()
    {
        isPressed = false;

        if (pressCoroutine != null)
        {
            StopCoroutine(pressCoroutine);
            pressCoroutine = null;
        }

        descriptionPanel.Hide();
    }
}
