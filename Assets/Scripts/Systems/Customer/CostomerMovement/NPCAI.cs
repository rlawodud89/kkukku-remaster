using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic; // List 사용을 위해 추가
using TMPro;
using UnityEditor.VersionControl;

public class NPCAI : MonoBehaviour
{
    public CustomerData myData;
    public Tilemap walkTilemap; // 바닥 타일맵
    public Pathfinding pathfinding; // A* 스크립트 연결을 위해 추가
    public float moveSpeed = 2f;
    private SpriteRenderer sr;
    private Animator animator;
    private Vector3 lastPosition;
    private Vector2 lastMoveDir = Vector2.down;
    public Vector3 questSpot;

    public NPCDataSO npcBaseData; // 인스펙터에서 해당 동물의 SO를 할당

    public enum NPCBehavior { Normal, SpecialQuest, Leaving }
    public NPCBehavior currentBehavior = NPCBehavior.Normal;

    [Header("Dialogue UI")]
    public GameObject speechBubble; // 말풍선 부모 오브젝트
    public TextMeshProUGUI speechText; // 말풍선 안의 텍스트 컴포넌트
    public float displayTime = 2.0f; // 말풍선 유지 시간

    [Header("Quest Effects")]
    public GameObject questIcon; // '!' 아이콘 오브젝트


    // 1. 캐릭터 클릭 감지
    void OnMouseDown()
    {
        string talk = npcBaseData.smallTalks[Random.Range(0, npcBaseData.smallTalks.Length)];

        if (currentBehavior != NPCBehavior.SpecialQuest)
        {
            ShowSpeechBubble(talk);
        }
    }

    public void ShowSpeechBubble(string message)
    {
        // 이미 켜져 있다면 코루틴 중단 후 새로 시작 (타이머 리셋)
        StopCoroutine("CloseSpeechBubble");

        speechText.text = message;

        // 3. 말풍선 오브젝트 활성화
        speechBubble.SetActive(true);


        // 4. 지정된 시간(예: 2초) 후에 꺼지도록 예약
        StartCoroutine(CloseSpeechBubble(2.0f));
    }

    IEnumerator CloseSpeechBubble(float delay)
    {
        yield return new WaitForSeconds(delay);
        speechBubble.SetActive(false);
    }
    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();
        lastPosition = transform.position;

        // 씬에 있는 Pathfinding 스크립트를 자동으로 찾거나 인스펙터에서 할당하세요.
        if (pathfinding == null)
        {
            pathfinding = FindObjectOfType<Pathfinding>();
            walkTilemap = pathfinding.walkTilemap;
        }

        if (npcBaseData != null)
        {
            int progress = npcBaseData.questProgress;

            // 2. 퀘스트 상태에 따른 행동 결정
            if (progress == 1)
            {
                currentBehavior = NPCBehavior.SpecialQuest;
            }
        }

        StartCoroutine(MainBehaviorRoutine());
    }
    IEnumerator MainBehaviorRoutine()
    {
        if (currentBehavior == NPCBehavior.SpecialQuest)
        {
            // 1. 퀘스트 장소로 이동
            yield return StartCoroutine(MoveWithAStar(questSpot));

            // 2. 도착 완료 후 아이콘 활성화
            if (questIcon != null)
            {
                questIcon.SetActive(true);
            }

            // 3. 이동을 멈추고 플레이어를 기다림
            animator.SetFloat("Speed", 0f);
        }
        else
        {
            // [일반 로직] 기존의 이불 고르고 계산대 가는 루틴
            yield return StartCoroutine(NormalShopRoutine());
        }
    }

    void Update()
    {
        // Y축 기반 정렬 (장애물 앞/뒤 처리)
        sr.sortingOrder = Mathf.RoundToInt(transform.position.y * -100);
    }

    void UpdateAnimation(Vector3 currentPosition)
    {
        Vector3 delta = currentPosition - lastPosition;

        if (delta.sqrMagnitude > 0.0001f)
        {
            Vector2 dir = delta.normalized;
            lastMoveDir = dir;

            animator.SetFloat("MoveX", dir.x);
            animator.SetFloat("MoveY", dir.y);
            animator.SetFloat("Speed", delta.magnitude);
        }
        else
        {
            // 멈췄을 때는 마지막 방향 유지
            animator.SetFloat("MoveX", lastMoveDir.x);
            animator.SetFloat("MoveY", lastMoveDir.y);
            animator.SetFloat("Speed", 0f);
        }

        lastPosition = currentPosition;
    }


    IEnumerator NormalShopRoutine()
    {
        // 1. 이불장 이동 및 선택
        Vector3 targetPos = GetRandomWardrobePos();
        yield return StartCoroutine(MoveWithAStar(targetPos));

        myData.currentState = CustomerData.State.Deciding;
        yield return new WaitForSeconds(2f);

        // 이불 결정 및 시각화
        myData.selectedItemID = 0;

        // 2. 캐셔 대기열 합류
        myData.currentState = CustomerData.State.MovingToCashier;
        CashierManager.Instance.JoinQueue(this); // 줄서기 등록

        // 내 차례(리스트의 0번)가 될 때까지 대기
        while (CashierManager.Instance.IsItMyTurn(this) == false)
        {
            yield return null; // 한 프레임 대기
        }

        // 3. 내 차례라면 결제 진행
        myData.currentState = CustomerData.State.Paying;
        yield return new WaitForSeconds(1.5f);

        // 결제 완료 후 줄에서 나감
        CashierManager.Instance.LeaveQueue(this);

        // 4. 퇴장
        yield return StartCoroutine(MoveWithAStar(new Vector3(-5, -5, 0)));
        ShopManager.Instance.activeCustomers.Remove(myData);
        Destroy(gameObject);
    }

    // --- 여기가 핵심입니다: A* 이동 함수 ---
    IEnumerator MoveWithAStar(Vector3 targetWorldPos)
    {
        // 1. 현재 내 위치와 목표 위치를 타일 좌표(Vector3Int)로 변환
        Vector3Int startTile = walkTilemap.WorldToCell(transform.position);
        Vector3Int targetTile = walkTilemap.WorldToCell(targetWorldPos);

        // 2. Pathfinding 스크립트에 길찾기 요청 (타일 리스트를 받아옴)
        List<Vector3Int> path = pathfinding.FindPath(startTile, targetTile);

        if (path != null) // 경로가 존재할 때만 이동
        {
            foreach (Vector3Int nextTile in path)
            {
                // 3. 다음 타일의 월드 중심 좌표 계산
                Vector3 worldPos = walkTilemap.GetCellCenterWorld(nextTile);

                // 4. 해당 타일 중심까지 이동
                while (Vector3.Distance(transform.position, worldPos) > 0.05f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, worldPos, moveSpeed * Time.deltaTime);
                    UpdateAnimation(transform.position);
                    yield return null;
                }
                animator.SetFloat("Speed", 0f);
            }
        }
        else
        {
            Debug.LogWarning("길을 찾을 수 없습니다!");
        }
    }

    Vector3 GetRandomWardrobePos()
    {
        // 실제로는 이불장 오브젝트들의 위치 리스트 중 하나를 랜덤으로 반환하게 수정하세요.
        return new Vector3(2, 3, 0);
    }

  
    public void MoveToQueuePoint()
    {
        // CashierManager에게 내가 서야 할 월드 좌표를 물어봄
        Vector3 targetQueuePos = CashierManager.Instance.GetQueuePosition(this);

        // 기존에 이동 중이던 코루틴이 있다면 멈추고 새로 이동 시작
        StopCoroutine("MoveToQueueRoutine");
        StartCoroutine(MoveToQueueRoutine(targetQueuePos));
    }

    // 대기열 전용 이동 코루틴 (A*를 써도 되고, 줄 안에서는 단순 직선 이동을 써도 됩니다)
    IEnumerator MoveToQueueRoutine(Vector3 targetPos)
    {
        while (Vector3.Distance(transform.position, targetPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            UpdateAnimation(transform.position);
            yield return null;
        }
        animator.SetFloat("Speed", 0f);
    }
}