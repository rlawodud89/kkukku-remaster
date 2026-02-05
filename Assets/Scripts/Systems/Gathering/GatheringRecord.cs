using System.Collections;
using UnityEngine;
using UnityEngine.UI;

public class GatheringRecord : MonoBehaviour
{
    public Transform recordContent;
    public GameObject gatherImagePrefab;

    private const int MAX_COUNT = 3;
    private const float SLIDE_DISTANCE = 60f;
    private const float DURATION = 0.2f;

    public void AddGatheredItem(Sprite sprite)
    {
        GameObject imageGO = Instantiate(gatherImagePrefab, recordContent);
        imageGO.GetComponent<Image>().sprite = sprite;

        // 최신이 맨 앞
        imageGO.transform.SetSiblingIndex(0);

        // 레이아웃 갱신 강제
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            recordContent.GetComponent<RectTransform>()
        );

        // 슬라이드 애니메이션
        StartCoroutine(SlideIn(imageGO.GetComponent<RectTransform>()));

        // 3개 초과 시 제거
        if (recordContent.childCount > MAX_COUNT)
        {
            Destroy(recordContent.GetChild(recordContent.childCount - 1).gameObject);
        }
    }

    IEnumerator SlideIn(RectTransform target)
    {
        Vector3 endPos = target.localPosition;
        Vector3 startPos = endPos + Vector3.left * SLIDE_DISTANCE;

        float t = 0f;

        target.localPosition = startPos;

        while (t < DURATION)
        {
            t += Time.deltaTime;
            float lerp = t / DURATION;
            target.localPosition = Vector3.Lerp(startPos, endPos, lerp);
            yield return null;
        }

        target.localPosition = endPos;
    }
}
