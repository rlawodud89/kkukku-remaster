using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatheringManager : MonoBehaviour
{
    public static GatheringManager Instance;

    [Header("전체 제한시간")]
    [SerializeField] private float timeLimit = 2f;

    private Coroutine timerCoroutine;
    private List<ItemTree> trees = new List<ItemTree>();

    public bool IsTimerRunning => timerCoroutine != null;

    void Awake()
    {
        if (Instance == null)
            Instance = this;
        else
            Destroy(gameObject);
    }

    void Start()
    {
        StartTimer();
    }


    // === 타이머 외부 제어 ===

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

    // === 타이머 코루틴 ===

    private IEnumerator TimerRoutine()
    {
        while (true)
        {
            float elapsed = 0f;

            // 0초 → timeLimit
            while (elapsed < timeLimit)
            {
                elapsed += Time.deltaTime;
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
