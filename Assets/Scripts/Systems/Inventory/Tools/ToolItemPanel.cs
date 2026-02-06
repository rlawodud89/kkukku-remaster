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
    private IToolPerformer toolPerformer;


    public void SetTool(ToolItemSO toolSO, ToolInventoryUI toolInventoryUI, IToolPerformer toolPerformer)
    {
        this.toolSO = toolSO;
        toolImg.sprite = toolSO.image;
        nameText.text = toolSO.name;
        infoText.text = toolPerformer.GetDescription(toolSO);

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
