using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreItemLoader : MonoBehaviour
{
    public ScrollRect ScrollView;

    [Header("신성 재료")]
    public RectTransform YarnContent;
    public RectTransform CottonContent;
    public RectTransform MoonContent;

    [Header("직원 고용")]
    public RectTransform WorkerContent;

    void Start()
    {

    }

    public void OnClickYarnBtn()
    {
        ScrollView.content = YarnContent;

        YarnContent.gameObject.SetActive(true);
        CottonContent.gameObject.SetActive(false);
        MoonContent.gameObject.SetActive(false);
    }

    public void OnClickCottonBtn()
    {
        ScrollView.content = CottonContent;

        YarnContent.gameObject.SetActive(false);
        CottonContent.gameObject.SetActive(true);
        MoonContent.gameObject.SetActive(false);
    }

    public void OnClickMoonBtn()
    {
        ScrollView.content = MoonContent;

        YarnContent.gameObject.SetActive(false);
        CottonContent.gameObject.SetActive(false);
        MoonContent.gameObject.SetActive(true);
    }
}