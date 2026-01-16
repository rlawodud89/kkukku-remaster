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

    private ToolInventory toolInventory;
    //private ToolSO tool;


    /*public void SetTool(ToolSO toolSO, ToolInventory toolInventory)
    {
        tool = toolSO;
        toolImg = toolSO.image;
        nameText = toolSO.name;
        infoText = toolSO.description;
        this.toolInventory = toolInventory;
    }*/

    public void SetTool(ToolInventory toolInventory)
    {
        this.toolInventory = toolInventory;
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
        toolInventory.SelectTool(this);
    }
}
