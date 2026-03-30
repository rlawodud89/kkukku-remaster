using System;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public enum DayPhase { Morning, Day, Evening, Night }

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;

    public static GameManager Instance
    {
        get
        {
            // 씬에 생성된 싱글톤이 없으면 자동 생성
            if (_instance == null)
            {
                var obj = new GameObject("GameManager");
                _instance = obj.AddComponent<GameManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as GameManager;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private int gold;
    private int moonRock;
    private int level;
    private float energy;
    private int maxEnergy = 200;

    public TMP_Text goldText;
    public TMP_Text moonRockText;
    public TMP_Text levelText;
    public UnityEngine.UI.Image energyBar;

    [Header("시간 설정")]
    public float gameTime = 0f; // 현재 총 경과 시간
    public float timeScale = 60f; // 실제 1초가 게임의 1분 (게임 24시간=실제 24분)

    public int hour;
    public int minute;

    //ui
    public GameObject timeUI;
    public TMP_Text timeText;
    public RectTransform clockHand;

    public DayPhase currentPhase;
    public static event Action<DayPhase> OnPhaseChangedEvent;

    [Header("디버그 도구")]
    [Range(0, 23)] public int debugHour; // 인스펙터 슬라이더로 조절
    public bool useDebugTime = false;   // 체크하면 시간을 강제로 고정/조절 가능

    // 인스펙터에서 값이 바뀔 때 실행되는 유니티 콜백
    private void OnValidate()
    {
        if (useDebugTime && Application.isPlaying)
        {
            SetGameTime(debugHour, 0);
        }
    }

    // 특정 시간으로 워프하는 함수
    public void SetGameTime(int targetHour, int targetMinute = 0)
    {
        // gameTime 계산: (시간 * 3600) + (분 * 60)
        // 현재 날짜(Day)는 유지하고 시간만 바꾸고 싶다면 날짜 값을 유지하는 로직이 필요할 수 있습니다.
        float dayInSeconds = 86400f;
        float currentDayCount = (int)(gameTime / dayInSeconds);

        gameTime = (currentDayCount * dayInSeconds) + (targetHour * 3600) + (targetMinute * 60);

        // 즉시 UI와 페이즈 업데이트
        hour = targetHour;
        minute = targetMinute;
        UpdateDayPhase();
        UpdateTimeUI(hour, minute);

        Debug.Log($"<color=yellow>[Debug]</color> 시간을 {targetHour:D2}:{targetMinute:D2}로 설정했습니다.");
    }


    void Start()
    {
        LoadGameData();
    }

    void Update()
    {
        // 매 프레임마다 시간 누적
        gameTime += Time.deltaTime * timeScale;

        // 초 단위를 시/분으로 변환
        hour = (int)(gameTime / 3600) % 24;
        minute = (int)(gameTime / 60) % 60;

        UpdateDayPhase(); // 시간대 체크
        UpdateTimeUI(hour, minute);

        ServiceLocator.Get<GameData>().User.SetPlayTime(gameTime);
    }

    void UpdateDayPhase()
    {
        DayPhase lastPhase = currentPhase;

        if (hour >= 6 && hour < 12) currentPhase = DayPhase.Morning;
        else if (hour >= 12 && hour < 17) currentPhase = DayPhase.Day;
        else if (hour >= 17 && hour < 21) currentPhase = DayPhase.Evening;
        else currentPhase = DayPhase.Night;

        // 시간대가 바뀐 순간에만 이벤트 발생
        if (lastPhase != currentPhase)
        {
            OnPhaseChanged();
        }
    }

    void OnPhaseChanged()
    {
        OnPhaseChangedEvent?.Invoke(currentPhase);
    }

    public void UpdateTimeUI(int hour, int minute)
    {
        if (timeText != null)
        {
            timeText.text = string.Format("{0:D2}:{1:D2}", hour, minute);
        }

        // 1. 전체 시간을 분 단위로 환산 (하루 = 1440분)
        float totalMinutes = (hour * 60) + minute;

        // 2. 각도 계산 (360도 / 1440분 = 1분당 0.25도)
        // 유니티는 시계 반대방향이 +이므로 시계방향 회전을 위해 -를 붙입니다.
        float angle = 45f - totalMinutes * 0.25f;

        // 3. 회전 적용
        clockHand.localRotation = Quaternion.Euler(0, 0, angle);
    }


    // 현재 진행 중인 코루틴을 저장 (중복 실행 방지)
    private Coroutine currentCoroutine;

    public void ClockButtonClick()
    {
        if (currentCoroutine != null)
        {
            StopCoroutine(currentCoroutine);
        }

        currentCoroutine = StartCoroutine(DisplayRoutine());
    }

    private IEnumerator DisplayRoutine()
    {
        // 시간 2초동안 보이게
        timeUI.gameObject.SetActive(true);
        yield return new WaitForSeconds(2f);
        timeUI.gameObject.SetActive(false);

        currentCoroutine = null;
    }



    void LoadGameData()
    {
        gold = ServiceLocator.Get<GameData>().User.GetCurrentGold();
        moonRock = ServiceLocator.Get<GameData>().User.GetCurrentMoonrock();

        var userData = ServiceLocator.Get<GameData>().User.GetUserData();
        level = userData.level;
        energy = userData.energy;

        gameTime = ServiceLocator.Get<GameData>().User.GetPlayTime();

        UpdateUI();
    }

    void UpdateUI()
    {
        goldText.text = gold.ToString();
        moonRockText.text = moonRock.ToString();

        levelText.text = $"Lv {level.ToString()}";
        energyBar.fillAmount = energy / maxEnergy;
        Debug.Log($"포근에너지: {energy}");
    }

    public void ChangeGold(int amount)
    {
        gold += amount;
        ServiceLocator.Get<GameData>().User.ChangeGold(amount);
        if (amount > 0) ServiceLocator.Get<GameData>().User.AddTodayGold(amount);
        UpdateUI();
    }

    public void ChangeMoonRock(int amount)
    {
        moonRock += amount;
        ServiceLocator.Get<GameData>().User.ChangeMoonrock(amount);
        if (amount > 0) ServiceLocator.Get<GameData>().User.AddTodayMoonrock(amount);
        UpdateUI();
    }

    public void ChangeEnergy(int amount)
    {
        energy += amount;
        ServiceLocator.Get<GameData>().User.SetLevelEnergy(level, energy);
        UpdateUI();

        if (maxEnergy == energy)
        {
            ChangeLevel();
        }
    }

    public void ChangeLevel()
    {
        level += 1;
        ServiceLocator.Get<GameData>().User.SetLevelEnergy(level, energy);
        UpdateUI();
    }



}
