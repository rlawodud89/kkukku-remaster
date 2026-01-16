using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;

public class FishingManagerUI : MonoBehaviour
{
    [Header("UI Objects")]
    [SerializeField] private GameObject fishPrefab; // Image가 달린 프리팹
    [SerializeField] private RectTransform spawnPoint;  // 생성 위치 (빈 UI 오브젝트)
    [SerializeField] private RectTransform targetBar;   // 판정선 (Image)
    [SerializeField] private Transform fishParent;      // 물고기가 생성될 부모 (보통 Panel)

    [Header("Balance")]
    [SerializeField] private float spawnInterval = 1.0f;
    [SerializeField] private float fallSpeed = 500f; // UI는 픽셀 단위라 숫자가 커야 함 (중요!)
    [SerializeField] private float perfectDistance = 50f; // 픽셀 단위 판정 범위
    [SerializeField] private float spawnRangeX = 300f;
    private float timer = 0;
    private List<FallingFishUI> activeFishes = new List<FallingFishUI>();

    void Update()
    {
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnFish();
            timer = 0;
        }

        if (Input.GetMouseButtonDown(0))
        {
            CheckTiming();
        }
    }

    void SpawnFish()
    {
        // 1. 생성 및 부모 설정 (중요: 두 번째 인자가 부모 Transform)
        GameObject obj = Instantiate(fishPrefab, fishParent);
        
        // 2. 위치를 스폰 포인트로 강제 이동
        RectTransform fishRect = obj.GetComponent<RectTransform>();
        float randomX = Random.Range(-spawnRangeX / 2f, spawnRangeX / 2f);
        fishRect.anchoredPosition = new Vector2(spawnPoint.anchoredPosition.x + randomX, spawnPoint.anchoredPosition.y);

        // 3. 셋업
        FallingFishUI fishScript = obj.GetComponent<FallingFishUI>();
        // targetBar의 Y좌표를 넘겨줌
        fishScript.Setup(fallSpeed, this, targetBar.anchoredPosition.y);
        
        activeFishes.Add(fishScript);
    }

    void CheckTiming()
    {
        if (activeFishes.Count == 0) return;

        FallingFishUI targetFish = activeFishes[0];
        RectTransform fishRect = targetFish.GetComponent<RectTransform>();

        // 거리 계산 (Y좌표 차이의 절대값)
        float distance = Mathf.Abs(fishRect.anchoredPosition.y - targetBar.anchoredPosition.y);

        if (distance <= perfectDistance)
        {
            Debug.Log($"<color=cyan>Perfect! (거리: {distance})</color>");
            CatchFish(targetFish);
        }
        else if (distance <= perfectDistance * 2.5f) // Good 범위
        {
            Debug.Log($"Good (거리: {distance})");
            CatchFish(targetFish);
        }
    }

    void CatchFish(FallingFishUI fish)
    {
        activeFishes.Remove(fish);
        Destroy(fish.gameObject);
    }

    public void OnFishMiss(FallingFishUI fish)
    {
        if (activeFishes.Contains(fish))
        {
            activeFishes.Remove(fish);
        }
    }
}