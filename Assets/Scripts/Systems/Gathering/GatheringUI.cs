using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatheringUI : MonoBehaviour
{
    public GameObject treePanel;
    public GameObject uiPanel;
    public GameObject toolPanel;

    public void OnClickToolBtn()
    {
        treePanel.SetActive(false);
        uiPanel.SetActive(false);
        toolPanel.SetActive(true);
    }

    public void OnClickToolBackBtn()
    {
        treePanel.SetActive(true);
        uiPanel.SetActive(true);
        toolPanel.SetActive(false);
    }

}
