using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GatheringRecord : MonoBehaviour
{
    public Transform recordContent;
    public GameObject gatherImagePrefab;

    private const int MAX_COUNT = 4;
    private const float SLIDE_DISTANCE = 60f;
    private const float DURATION = 0.2f;

    private Dictionary<RectTransform, Coroutine> animCoroutines = new();


    public void AddGatheredItem(Sprite sprite)
    {
        // 새 아이템 생성
        GameObject imageGO = Instantiate(gatherImagePrefab, recordContent);
        imageGO.GetComponent<Image>().sprite = sprite;

        // 최신이 맨 앞
        imageGO.transform.SetSiblingIndex(0);

        // 레이아웃 갱신
        LayoutRebuilder.ForceRebuildLayoutImmediate(
            recordContent.GetComponent<RectTransform>()
        );

        // 새 아이템 슬라이드 인
        StartSafeCoroutine(
            imageGO.GetComponent<RectTransform>(),
            SlideIn(imageGO.GetComponent<RectTransform>())
        );


        // 초과 시 마지막 아이템 슬라이드 아웃
        if (recordContent.childCount > MAX_COUNT)
        {
            Transform last = recordContent.GetChild(recordContent.childCount - 1);
            RectTransform lastRT = last.GetComponent<RectTransform>();

            StartSafeCoroutine(
                lastRT,
                SlideOutAndDestroy(lastRT)
            );
        }
    }

    void StartSafeCoroutine(RectTransform target, IEnumerator routine)
    {
        if (target == null) return;

        if (animCoroutines.TryGetValue(target, out var running))
        {
            StopCoroutine(running);
        }

        animCoroutines[target] = StartCoroutine(routine);
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

    IEnumerator SlideOutAndDestroy(RectTransform target)
    {
        if (target == null) yield break;

        Vector3 startPos = target.localPosition;
        Vector3 endPos = startPos + Vector3.right * SLIDE_DISTANCE;

        float t = 0f;

        while (t < DURATION)
        {
            if (target == null) yield break;

            t += Time.deltaTime;
            float lerp = t / DURATION;
            target.localPosition = Vector3.Lerp(startPos, endPos, lerp);
            yield return null;
        }

        if (target != null)
        {
            animCoroutines.Remove(target);
            Destroy(target.gameObject);
        }
    }

}
