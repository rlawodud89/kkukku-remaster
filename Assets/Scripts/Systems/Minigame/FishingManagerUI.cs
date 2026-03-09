using UnityEngine;
using UnityEngine.UI;
using System.Collections.Generic;
using System.Linq; // ★ 리스트 필터링(.Where) 사용을 위해 필수

public class FishingManagerUI : MonoBehaviour
{
    [Header("Item Data (Drag & Drop Here)")]
    // ★ 여기에 만드신 이불 아이템 SO들을 직접 드래그해서 넣어주세요!
    public List<MaterialItemSO> allItems = new List<MaterialItemSO>();

    [Header("Game Status")]
    public int currentFishingLevel = 1; // 현재 낚시 레벨 (이게 오르면 더 좋은 게 나옴)

    [Header("UI Objects")]
    [SerializeField] private GameObject fishPrefab;     // 떨어지는 물고기 프리팹
    [SerializeField] private RectTransform spawnPoint;  // 생성 위치 (빈 오브젝트)
    [SerializeField] private RectTransform targetBar;   // 판정선 (초록색 바)
    [SerializeField] private Transform fishParent;      // 물고기가 생성될 부모 패널

    [Header("Balance")]
    [SerializeField] private float spawnInterval = 1.0f; // 생성 간격
    [SerializeField] private float fallSpeed = 500f;     // 떨어지는 속도
    [SerializeField] private float perfectDistance = 50f;// 판정 범위
    [SerializeField] private float spawnRangeX = 300f;   // 생성 좌우 범위

    private float timer = 0;
    private List<FallingFishUI> activeFishes = new List<FallingFishUI>();

    void Update()
    {
        // 1. 일정 시간마다 생성
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnFish();
            timer = 0;
        }

        // 2. 클릭 시 판정
        if (Input.GetMouseButtonDown(0))
        {
            CheckTiming();
        }
    }

    void SpawnFish()
    {
        // 리스트가 비어있으면 아무것도 안 함 (오류 방지)
        if (allItems.Count == 0) return;

        // ★ 레벨에 맞는 아이템 하나 뽑기
        MaterialItemSO selectedItem = GetItemByLevel();
        if (selectedItem == null) return;

        // 생성 로직
        GameObject obj = Instantiate(fishPrefab, fishParent);
        RectTransform fishRect = obj.GetComponent<RectTransform>();
        
        // 위치 잡기
        float randomX = Random.Range(-spawnRangeX / 2f, spawnRangeX / 2f);
        fishRect.anchoredPosition = new Vector2(spawnPoint.anchoredPosition.x + randomX, spawnPoint.anchoredPosition.y);

        // 데이터(이미지) 주입
        FallingFishUI fishScript = obj.GetComponent<FallingFishUI>();
        fishScript.Setup(fallSpeed, this, targetBar.anchoredPosition.y, selectedItem.image);
        
        activeFishes.Add(fishScript);
    }

    // 내 레벨에 맞는 아이템 랜덤 선택
    MaterialItemSO GetItemByLevel()
    {
        // 1. 내 레벨보다 낮거나 같은 아이템만 추려냄
        var candidates = allItems.Where(item => item.level <= currentFishingLevel).ToList();

        // 2. 후보가 없으면 null 반환
        if (candidates.Count == 0) return null;

        // 3. 후보 중 랜덤 1개 선택
        int randomIndex = Random.Range(0, candidates.Count);
        return candidates[randomIndex];
    }

    // 판정 로직
    void CheckTiming()
    {
        if (activeFishes.Count == 0) return;

        FallingFishUI targetFish = activeFishes[0];
        if (targetFish == null) 
        {
            activeFishes.RemoveAt(0);
            return;
        }

        RectTransform fishRect = targetFish.GetComponent<RectTransform>();
        float distance = Mathf.Abs(fishRect.anchoredPosition.y - targetBar.anchoredPosition.y);

        if (distance <= perfectDistance)
        {
            Debug.Log($"<color=cyan>Perfect!</color> ({targetFish.name})");
            // TODO: 여기서 아이템 획득 로직 추가
            CatchFish(targetFish);
        }
        else if (distance <= perfectDistance * 2.5f)
        {
            Debug.Log($"Good ({targetFish.name})");
            CatchFish(targetFish);
        }
    }

    void CatchFish(FallingFishUI fish)
    {
        if (activeFishes.Contains(fish))
        {
            activeFishes.Remove(fish);
            Destroy(fish.gameObject);
        }
    }

    public void OnFishMiss(FallingFishUI fish)
    {
        if (activeFishes.Contains(fish))
        {
            activeFishes.Remove(fish);
        }
    }
}