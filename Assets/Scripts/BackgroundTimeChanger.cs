using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class BackgroundTimeChanger : MonoBehaviour
{
    [Header("시간에 따라 킬 배경")]
    public List<TimeBackgroundEntry> backgroundImages = new();
    [Header("시간에 따라 색감을 변경할 모든 오브젝트들")]
    public List<GameObject> backgroundObjects = new();
    [Header("각 시간에 오브젝트들에 적용할 색감")]
    public List<TimeColorEntry> objectColors = new();

    public void Start()
    {
        // 시작할 때의 시간으로 배경 변경
        OnTimeChange(GameManager.Instance.currentPhase);
    }

    private void OnEnable()
    {
        GameManager.OnPhaseChangedEvent += OnTimeChange;
    }

    private void OnDisable()
    {
        GameManager.OnPhaseChangedEvent -= OnTimeChange;
    }

    private void OnTimeChange(DayPhase dayPhase)
    {
        // 배경 이미지 변경
        foreach (var entry in backgroundImages)
        {
            entry.background.SetActive(false);
        }
        var bg = backgroundImages.Find(x => x.dayPhase == dayPhase);
        if (bg != null)
        {
            bg.background.SetActive(true);
        }

        // 오브젝트 색감 변경
        var objColorEntry = objectColors.Find(x => x.dayPhase == dayPhase);
        if (objColorEntry != null)
        {
            foreach (var obj in backgroundObjects)
            {
                var img = obj.GetComponent<Image>();
                if (img != null)
                    img.color = objColorEntry.color;
            }
        }

    }

}

[System.Serializable]
public class TimeBackgroundEntry
{
    public DayPhase dayPhase;
    public GameObject background;
}

[System.Serializable]
public class TimeColorEntry
{
    public DayPhase dayPhase;
    public Color color;
}