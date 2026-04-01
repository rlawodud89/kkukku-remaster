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

    [Header("마을 패널")]
    public GameObject interiorStorePanel;
    public GameObject craftStorePanel;
    public GameObject toolPurcahsePanel;
    public GameObject facilityUpgradePanel;


    // 달빛 언덕
    public void OnClickMaterialBtn()
    {
        mainMenuPanel.SetActive(false);
        materialPurchasePanel.SetActive(true);
        TutorialEventBus.Raise(TutorialID.ClickMaterialMenu);
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
        TutorialEventBus.Raise(TutorialID.ExitMaterialMenu);
    }

    public void OnClickWorkerBackBtn()
    {
        workerEmployPanel.SetActive(false);
        mainMenuPanel.SetActive(true);
    }


    // 마을
    public void OnClickInteriorStoreBtn()
    {
        interiorStorePanel.SetActive(true);
    }

    public void OnClickCraftStoreBtn()
    {
        craftStorePanel.SetActive(true);
    }

    public void OnClickInteriorStoreBackBtn()
    {
        interiorStorePanel.SetActive(false);
    }

    public void OnClickToolBtn()
    {
        toolPurcahsePanel.SetActive(true);
        craftStorePanel.SetActive(false);
    }

    public void OnClickFacilityBtn()
    {
        facilityUpgradePanel.SetActive(true);
        craftStorePanel.SetActive(false);
    }

    public void OnClickCraftStoreBackBtn()
    {
        craftStorePanel.SetActive(false);
    }

    public void OnClickToolPurchaseBackBtn()
    {
        toolPurcahsePanel.SetActive(false);
        craftStorePanel.SetActive(true);
    }

    public void OnClickFacilityUpgradeBackBtn()
    {
        facilityUpgradePanel.SetActive(false);
        craftStorePanel.SetActive(true);
    }
}
