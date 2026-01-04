using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ToolUI : MonoBehaviour
{
    [Header("채집, 낚시 UI 패널")]
    [SerializeField] private GameObject uiPanel;
    [Header("도구 선택 패널")]
    [SerializeField] private GameObject toolPanel;

    public void OnClickToolBtn()
    {
        uiPanel.SetActive(false);
        toolPanel.SetActive(true);
    }

    public void OnClickToolBackBtn()
    {
        uiPanel.SetActive(true);
        toolPanel.SetActive(false);
    }

}
