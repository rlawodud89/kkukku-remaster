using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemFruit : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private RectTransform ItemFruitPanel;
    [SerializeField] private Button itemBtn;
    [SerializeField] private TMP_Text countText;

    private int maxClickCount = 5;
    private int currentClickCount = 0;
    private SnackItemSO snackItem;

    void Start()
    {
        countText.text = (maxClickCount - currentClickCount).ToString();
    }


    public void OnClickItemFruit()
    {
        currentClickCount++;

        if (currentClickCount >= maxClickCount)
        {
            GatheringManager.Instance.AddSnackToInventory(snackItem);
            gameObject.SetActive(false);
        }
        else
        {
            countText.text = (maxClickCount - currentClickCount).ToString();
        }
    }


    public void ResetItemFruit(Vector2 newPos)
    {
        int snackLevel = Get_RandomLevel();
        do
        {
            snackItem = ServiceLocator.Get<GameData>().Inventory.GetRandomSnackItemSO();
        } while (snackItem.level == snackLevel);

        itemBtn.image.sprite = snackItem.image;
        currentClickCount = 0;
        countText.text = maxClickCount.ToString();
        gameObject.SetActive(true);

        ItemFruitPanel.anchoredPosition = newPos;
    }

    public void SetMaxClickCount(int maxClickCount)
    {
        this.maxClickCount = maxClickCount;
        countText.text = (maxClickCount - currentClickCount).ToString();
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

    private int Get_RandomLevel()
    {
        // 높은 레벨이 덜 선택되도록 가중치 설정 
        int weight1 = 60;
        int weight2 = 30;
        int weight3 = 10;
        int totalWeight = weight1 + weight2 + weight3;
        int rand = UnityEngine.Random.Range(1, totalWeight + 1);
        if (rand <= weight1) return 1;
        else if (rand <= weight1 + weight2) return 2;
        else return 3;
    }

}
