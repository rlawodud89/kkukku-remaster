using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemFruit : MonoBehaviour
{
    [SerializeField] private static int maxClickCount = 5;
    [Header("UI 요소")]
    [SerializeField] private RectTransform ItemFruitPanel;
    [SerializeField] private Button itemBtn;
    [SerializeField] private TMP_Text countText;

    private int currentClickCount = 0;
    //private ItemSO item;

    void Start()
    {
        countText.text = (maxClickCount - currentClickCount).ToString();
    }


    public void OnClickItemFruit()
    {
        currentClickCount++;

        if (currentClickCount >= maxClickCount)
        {
            gameObject.SetActive(false);
        }
        else
        {
            countText.text = (maxClickCount - currentClickCount).ToString();
        }
    }


    public void ResetItemFruit(Vector2 newPos)
    {
        currentClickCount = 0;
        countText.text = maxClickCount.ToString();
        gameObject.SetActive(true);

        ItemFruitPanel.anchoredPosition = newPos;
    }

    public Rect GetFruitPanelRectAtPosition(Vector2 pos)
    {
        Vector2 size = ItemFruitPanel.rect.size;

        return new Rect(
            pos - size / 2,
            size
        );
    }

    public Rect GetFruitPanelRect()
    {
        return ItemFruitPanel.rect;
    }

}
