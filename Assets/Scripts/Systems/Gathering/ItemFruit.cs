using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemFruit : MonoBehaviour
{
    [SerializeField] private static int maxClickCount = 5;
    [Header("UI 요소")]
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

    public void ResetItemFruit()
    {
        currentClickCount = 0;
        countText.text = maxClickCount.ToString();
        gameObject.SetActive(true);
    }
}
