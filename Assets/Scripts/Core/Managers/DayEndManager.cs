using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayEndManager : MonoBehaviour
{
    private static DayEndManager Instance;

    private void Awake()
    {
        // 이미 존재하면 중복 제거
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 루트 전체를 유지
        DontDestroyOnLoad(gameObject);
    }

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
