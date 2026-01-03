using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreItemLoader : MonoBehaviour
{
    public ScrollRect scrollView;

    [Header("신성 재료")]
    public RectTransform yarnContent;
    public RectTransform cottonContent;
    public RectTransform moonContent;

    [Header("직원 고용")]
    public RectTransform workerContent;

    [Header("구매 팝업창")]
    public BuyPopup buyPopup;

    void Start()
    {

    }

    public void OnClickYarnBtn()
    {
        scrollView.content = yarnContent;

        yarnContent.gameObject.SetActive(true);
        cottonContent.gameObject.SetActive(false);
        moonContent.gameObject.SetActive(false);
    }

    public void OnClickCottonBtn()
    {
        scrollView.content = cottonContent;

        yarnContent.gameObject.SetActive(false);
        cottonContent.gameObject.SetActive(true);
        moonContent.gameObject.SetActive(false);
    }

    public void OnClickMoonBtn()
    {
        scrollView.content = moonContent;

        yarnContent.gameObject.SetActive(false);
        cottonContent.gameObject.SetActive(false);
        moonContent.gameObject.SetActive(true);
    }
}