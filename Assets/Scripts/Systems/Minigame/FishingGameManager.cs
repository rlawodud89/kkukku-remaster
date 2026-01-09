using UnityEngine;
using System.Collections.Generic;
using UnityEngine.UI; // 점수 표시용 (Text 등 사용 시)

public class FishingGameManager : MonoBehaviour
{
    [Header("Settings")]
    [SerializeField] private GameObject fishPrefab; // 물고기 프리팹 연결
    [SerializeField] private Transform spawnPoint;  // 생성 위치 (화면 위)
    [SerializeField] private Transform targetBar;   // 판정선 위치 (화면 아래)
    
    [Header("Balance")]
    [SerializeField] private float spawnInterval = 1.5f; // 생성 간격 (초)
    [SerializeField] private float fallSpeed = 5f;       // 떨어지는 속도
    [SerializeField] private float perfectDistance = 0.5f; // 퍼펙트 판정 범위
    [SerializeField] private float goodDistance = 1.2f;    // 굿 판정 범위

    private float timer = 0;
    
    // 현재 화면에 떠 있는 물고기들을 관리하는 리스트
    private List<FallingFish> activeFishes = new List<FallingFish>();

    void Update()
    {
        // 1. 물고기 생성 타이머
        timer += Time.deltaTime;
        if (timer >= spawnInterval)
        {
            SpawnFish();
            timer = 0;
        }

        // 2. 터치 입력 감지 (모바일 터치 + PC 마우스 클릭 모두 대응)
        if (Input.GetMouseButtonDown(0))
        {
            CheckTiming();
        }
    }

    void SpawnFish()
    {
        GameObject obj = Instantiate(fishPrefab, spawnPoint.position, Quaternion.identity);
        FallingFish fishScript = obj.GetComponent<FallingFish>();
        
        // 물고기에게 속도와 매니저 정보를 넘겨줌
        fishScript.Setup(fallSpeed, this);
        
        // 리스트에 추가 (판정을 위해)
        activeFishes.Add(fishScript);
    }

    void CheckTiming()
    {
        // 화면에 물고기가 없으면 무시
        if (activeFishes.Count == 0) return;

        // 가장 먼저 생성된(가장 아래에 있는) 물고기를 가져옴
        FallingFish targetFish = activeFishes[0];

        // 물고기와 판정선 사이의 거리 계산 (절대값)
        float distance = Mathf.Abs(targetFish.transform.position.y - targetBar.position.y);

        if (distance <= perfectDistance)
        {
            Debug.Log("<color=cyan>PERFECT!!</color>");
            // TODO: 점수 증가, 이펙트 재생
            CatchFish(targetFish);
        }
        else if (distance <= goodDistance)
        {
            Debug.Log("<color=green>Good</color>");
            // TODO: 점수 조금 증가
            CatchFish(targetFish);
        }
        else
        {
            Debug.Log("<color=red>Miss (Too far)</color>");
            // 너무 멀리 있을 때 누르면 그냥 헛스윙 (아무 일도 안 일어남)
        }
    }

    // 물고기 잡았을 때 처리
    void CatchFish(FallingFish fish)
    {
        activeFishes.Remove(fish); // 리스트에서 제거
        Destroy(fish.gameObject);  // 오브젝트 삭제
    }

    // 물고기가 바닥으로 떨어져버렸을 때 (FallingFish 스크립트에서 호출)
    public void OnFishMiss(FallingFish fish)
    {
        if (activeFishes.Contains(fish))
        {
            Debug.Log("Miss... (놓침)");
            activeFishes.Remove(fish);
            // TODO: 체력 감소 등 패널티
        }
    }
}