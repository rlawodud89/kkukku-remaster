using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class StorePanelUI : MonoBehaviour
{
    [Header("아이템 띄울 스크롤뷰")]
    [SerializeField] private ScrollRect scrollView;

    [Header("신성 재료")]
    [SerializeField] private RectTransform yarnContent;
    [SerializeField] private RectTransform cottonContent;
    [SerializeField] private RectTransform moonpieceContent;

    [Header("직원 고용")]
    [SerializeField] private RectTransform workerContent;

    [Header("인테리어")]
    [SerializeField] private RectTransform shopContent;
    [SerializeField] private RectTransform roomContent;
    [SerializeField] private RectTransform tileContent;

    [Header("낚시, 채집 도구")]
    [SerializeField] private RectTransform fishingContent;
    [SerializeField] private RectTransform gatheringContent;


    // === 스크롤뷰에 표시되는 부류 변경 버튼 핸들러 ===

    public void OnClickYarnBtn()
    {
        scrollView.content = yarnContent;

        yarnContent.gameObject.SetActive(true);
        cottonContent.gameObject.SetActive(false);
        moonpieceContent.gameObject.SetActive(false);
    }

    public void OnClickCottonBtn()
    {
        scrollView.content = cottonContent;

        yarnContent.gameObject.SetActive(false);
        cottonContent.gameObject.SetActive(true);
        moonpieceContent.gameObject.SetActive(false);
    }

    public void OnClickMoonBtn()
    {
        scrollView.content = moonpieceContent;

        yarnContent.gameObject.SetActive(false);
        cottonContent.gameObject.SetActive(false);
        moonpieceContent.gameObject.SetActive(true);
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
        scrollView.content = fishingContent;

        fishingContent.gameObject.SetActive(true);
        gatheringContent.gameObject.SetActive(false);
    }

    public void OnClickGatheringBtn()
    {
        scrollView.content = gatheringContent;

        fishingContent.gameObject.SetActive(false);
        gatheringContent.gameObject.SetActive(true);
    }
}