using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToolItemPanel : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private Image toolImg;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text infoText;
    [SerializeField] private GameObject UsedImg;

    private ToolInventoryUI toolInventoryUI;
    private ToolItemSO toolSO;


    public void SetTool(ToolItemSO toolSO, ToolInventoryUI toolInventoryUI)
    {
        this.toolSO = toolSO;
        toolImg.sprite = toolSO.image;
        nameText.text = toolSO.name;

        if (toolSO.toolType == ToolType.GATHERING)
        {
            infoText.text = $"클릭 필요 횟수: {toolSO.needClickCount}번";
        }

        this.toolInventoryUI = toolInventoryUI;
    }

    public void UsedOn()
    {
        UsedImg.SetActive(true);
    }

    public void UsedOff()
    {
        UsedImg.SetActive(false);
    }

    public void OnClickToolContent()
    {
        toolInventoryUI.SelectTool(toolSO, this);
    }
}
