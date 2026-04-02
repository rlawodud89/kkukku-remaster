using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class DayEndManager : MonoBehaviour
{
    private static DayEndManager Instance;

    public UpdateDayEndUI dayEndUI;

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
        GameManager.OnDayEndEvent += HandleDayEnd;
    }

    private void OnDisable()
    {
        GameManager.OnDayEndEvent -= HandleDayEnd;
    }

    private void HandleDayEnd()
    {
        dayEndUI.Setup(
            ServiceLocator.Get<GameData>().User.GetTodayGold(),
            ServiceLocator.Get<GameData>().User.GetTodayMoonrock()
            );
        dayEndUI.gameObject.SetActive(true);
    }
}
