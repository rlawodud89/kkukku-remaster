using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ToolContent : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private Image toolImg;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text infoText;

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

    public void HighlightOn()
    {
        toolImg.color = Color.red;
    }

    public void HighlightOff()
    {
        toolImg.color = Color.white;
    }

    public void OnClickToolContent()
    {
        //toolInventory.SelectTool();
    }
}
