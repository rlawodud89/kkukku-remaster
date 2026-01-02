using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuClick : MonoBehaviour
{
    [Header("´Þºû¾ð´ö ÆÐ³Î")]
    public GameObject MainMenuPanel;
    public GameObject MaterialPurchasePanel;
    public GameObject WorkerEmployPanel;

    [Header("±¸¸Å ÆË¾÷Ã¢")]
    public BuyPopup buyPopup;

    public void OnClickMaterialBtn()
    {
        MainMenuPanel.SetActive(false);
        MaterialPurchasePanel.SetActive(true);
    }

    public void OnClickLevelBtn()
    {
        buyPopup.gameObject.SetActive(true);
    }

    public void OnClickWorkerBtn()
    {
        MainMenuPanel.SetActive(false);
        MaterialPurchasePanel.SetActive(true);
    }

    public void OnClickMaterialBackBtn()
    {
        MaterialPurchasePanel.SetActive(false);
        MainMenuPanel.SetActive(true);
    }

    public void OnClickWorkerBackBtn()
    {
        WorkerEmployPanel.SetActive(false);
        MainMenuPanel.SetActive(true);
    }

}
