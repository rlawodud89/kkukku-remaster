using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GatheringClock : MonoBehaviour
{
    [SerializeField] private Image clockImg;

    void Update()
    {
        if (!GatheringManager.Instance)
            return;

        if (!GatheringManager.Instance.IsTimerRunning())
        {
            clockImg.fillAmount = 0f;
            return;
        }

        clockImg.fillAmount = GatheringManager.Instance.GetRemainingTime();
    }
}
