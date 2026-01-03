using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StoreItemLoader : MonoBehaviour
{
    [Header("아이템 띄울 스크롤뷰")]
    public ScrollRect scrollView;
    public GameObject countablePrefab;
    public GameObject countlessPrefab;

    [Header("구매 팝업창")]
    public BuyPopup buyPopup;

    [Header("신성 재료")]
    public RectTransform yarnContent;
    public RectTransform cottonContent;
    public RectTransform moonContent;

    [Header("직원 고용")]
    public RectTransform workerContent;

    [Header("인테리어")]
    public RectTransform shopContent;
    public RectTransform roomContent;
    public RectTransform tileContent;

    [Header("낚시, 채집 도구")]
    public RectTransform fishingContent;
    public RectTransform gatheringContent;


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

    public void OnClickShopBtn()
    {
        scrollView.content = shopContent;

        shopContent.gameObject.SetActive(true);
        roomContent.gameObject.SetActive(false);
        tileContent.gameObject.SetActive(false);
    }

    public void OnClickRoomBtn()
    {
        scrollView.content = roomContent;

        shopContent.gameObject.SetActive(false);
        roomContent.gameObject.SetActive(true);
        tileContent.gameObject.SetActive(false);
    }

    public void OnClickTileBtn()
    {
        scrollView.content = tileContent;

        shopContent.gameObject.SetActive(false);
        roomContent.gameObject.SetActive(false);
        tileContent.gameObject.SetActive(true);
    }

    public void OnClickFishingBtn()
    {
        scrollView.content= fishingContent;

        fishingContent.gameObject.SetActive(true);
        gatheringContent.gameObject .SetActive(false);
    }

    public void OnClickGatheringBtn()
    {
        scrollView.content = gatheringContent;

        fishingContent.gameObject.SetActive(false);
        gatheringContent.gameObject.SetActive(true);
    }
}