using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuClick : MonoBehaviour
{
    [Header("´Þºû¾ð´ö ÆÐ³Î")]
    public GameObject mainMenuPanel;
    public GameObject materialPurchasePanel;
    public GameObject workerEmployPanel;

    [Header("±¸¸Å ÆË¾÷Ã¢")]
    public BuyPopup buyPopup;

    public void OnClickMaterialBtn()
    {
        mainMenuPanel.SetActive(false);
        materialPurchasePanel.SetActive(true);
    }

    public void OnClickLevelBtn()
    {
        buyPopup.gameObject.SetActive(true);
    }

    public void OnClickWorkerBtn()
    {
        mainMenuPanel.SetActive(false);
        workerEmployPanel.SetActive(true);
    }

    public void OnClickMaterialBackBtn()
    {
        materialPurchasePanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

    public void OnClickWorkerBackBtn()
    {
        workerEmployPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }

}
