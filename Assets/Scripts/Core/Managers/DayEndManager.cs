using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayEndManager : MonoBehaviour
{
    private void OnEnable()
    {
        GameManager.OnPhaseChangedEvent += HandlePhaseChanged;
    }

    private void OnDisable()
    {
        GameManager.OnPhaseChangedEvent -= HandlePhaseChanged;
    }

    private void HandlePhaseChanged(DayPhase phase)
    {
        if (phase == DayPhase.Morning)
        {
            // TODO: 하루 마무리 패널 띄우기

            ServiceLocator.Get<GameData>().User.ResetTodayGoldMoonrock();
            ServiceLocator.Get<GameData>().Store.ResetAllStoreItemList();
        }
    }
}
