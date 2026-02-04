using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatheringManager : MonoBehaviour
{
    public static GatheringManager Instance;

    [Header("전체 제한시간")]
    [SerializeField] private float timeLimit;

    private List<ItemTree> trees = new List<ItemTree>();
    private Coroutine timerCoroutine;
    private float elapsed;


    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        ResetAllButtons();
        StartTimer();
    }


    // === 타이머 외부 제어 및 접근 ===

    public void StartTimer()
    {
        if (timerCoroutine != null)
            return;

        timerCoroutine = StartCoroutine(TimerRoutine());
    }

    public void StopTimer()
    {
        if (timerCoroutine != null)
        {
            StopCoroutine(timerCoroutine);
            timerCoroutine = null;

            ResetAllButtons();
        }
    }

    public bool IsTimerRunning()
    {
        return timerCoroutine != null;
    }

    public float GetRemainingTime()
    {
        return 1 - Mathf.Clamp01(elapsed / timeLimit);
        // 0 ~ 1 사이 값으로 반환, 시작할 때 남은 시간이 1이고 다 지난 후 남은 시간 0이 됨
    }


    // === 타이머 코루틴 ===

    private IEnumerator TimerRoutine()
    {
        while (true)
        {
            elapsed = 0f;

            // 0초 → timeLimit
            while (elapsed < timeLimit)
            {
                elapsed += Time.deltaTime;
                //Debug.Log("남은 시간: " + GetRemainingTime());
                yield return null;
            }

            // 제한시간 도달
            OnTimeOver();
        }
    }

    private void OnTimeOver()
    {
        ResetAllButtons();
    }


    // === 아이템 트리 관리 ===

    public void RegisterItemTree(ItemTree itemTree)
    {
        if (!trees.Contains(itemTree))
            trees.Add(itemTree);
    }

    private void ResetAllButtons()
    {
        foreach (var itemTree in trees)
        {
            itemTree.ResetItemTree();
        }
    }
}
