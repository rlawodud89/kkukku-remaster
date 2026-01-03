using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MenuClick : MonoBehaviour
{
    [Header("달빛언덕 패널")]
    public GameObject mainMenuPanel;
    public GameObject materialPurchasePanel;
    public GameObject workerEmployPanel;

    [Header("구매 팝업창")]
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
