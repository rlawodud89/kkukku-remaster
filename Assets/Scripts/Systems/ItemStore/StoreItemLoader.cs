using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreItemLoader : MonoBehaviour
{
    public ScrollRect scrollView;
    public GameObject countablePrefab;
    public GameObject countlessPrefab;

    [Header("�ż� ���")]
    public RectTransform yarnContent;
    public RectTransform cottonContent;
    public RectTransform moonContent;

    [Header("���� ����")]
    public RectTransform workerContent;

    [Header("���� �˾�â")]
    public BuyPopup buyPopup;

    void Start()
    {
        // 스크롤뷰에 아이템 추가할 때 사용하는 코드
        //GameObject item = Instantiate(countablePrefab, moonContent);
        //CountableContent ui = item.GetComponent<CountableContent>();
        //ui.SetItem();
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