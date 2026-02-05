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

        StartCoroutine(InitAndStartBehavior());
    }

    IEnumerator InitAndStartBehavior()
    {
        // 데이터 매니저가 로딩될 때까지 0.3~0.5초 정도 충분히 대기
        yield return new WaitForSeconds(0.5f);

        // 데이터가 여전히 비어있다면 에러 방지를 위해 한 번 더 체크
        if (ShopStorageDataManager.Instance == null || ShopStorageDataManager.Instance.interiorData == null)
        {
            Debug.LogError("가구 데이터가 로드되지 않았습니다! NPC 행동을 중지합니다.");
            yield break;
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

    // 기존 UpdateAnimation 함수는 삭제하거나 사용하지 않습니다.
    // 대신 아래 함수를 새로 만듭니다.
    void SetDirection(Vector3 dir)
    {
        // 방향 벡터가 너무 작으면 무시 (0일 때 휙 도는 것 방지)
        if (dir.sqrMagnitude < 0.01f) return;

        lastMoveDir = dir;
        animator.SetFloat("MoveX", dir.x);
        animator.SetFloat("MoveY", dir.y);
        animator.SetFloat("Speed", 1f); // 움직인다고 알림
    }

    IEnumerator MoveWithAStar(Vector3 targetWorldPos)
    {
        Vector3Int startTile = walkTilemap.WorldToCell(transform.position);
        Vector3Int targetTile = walkTilemap.WorldToCell(targetWorldPos);

        if (startTile == targetTile) yield break;

        List<Vector3Int> path = pathfinding.FindPath(startTile, targetTile);

        if (path != null && path.Count > 0)
        {
            foreach (Vector3Int nextTile in path)
            {
                Vector3 worldPos = walkTilemap.GetCellCenterWorld(nextTile);

                // 💡 핵심 변경: 이동하기 전에 "방향"을 먼저 고정합니다.
                // 덜덜 떨리는 delta 값이 아니라, 명확한 목적지 벡터를 사용합니다.
                Vector3 direction = (worldPos - transform.position).normalized;
                SetDirection(direction);

                // 이동 로직
                while (Vector3.Distance(transform.position, worldPos) > 0.05f)
                {
                    transform.position = Vector3.MoveTowards(transform.position, worldPos, moveSpeed * Time.deltaTime);

                    // 이동 중에는 UpdateAnimation을 호출하지 않습니다. 
                    // 이미 위에서 SetDirection으로 방향을 고정했기 때문입니다.
                    yield return null;
                }
                transform.position = worldPos;
            }
        }
        else
        {
            Debug.LogWarning($"[{name}] 길을 찾을 수 없습니다! 목표: {targetTile}");
        }

        // 도착 후 멈춤 처리
        animator.SetFloat("Speed", 0f);
        animator.SetFloat("MoveX", lastMoveDir.x);
        animator.SetFloat("MoveY", lastMoveDir.y);
    }

    IEnumerator NormalShopRoutine()
    {
        // 1. 가구 탐색 및 스마트 위치 선정
        Interiorinfo targetTable = GetAnyTableInfo();

        if (targetTable != null)
        {
            // 💡 변경점: 무조건 아래가 아니라, 갈 수 있는 빈 곳을 찾음
            Vector3 targetPos = GetSmartInteractionPos(targetTable);

            // 만약 갈 수 있는 곳이 아예 없다면(사방이 벽) 스킵
            if (targetPos != Vector3.zero)
            {
                yield return StartCoroutine(MoveWithAStar(targetPos));

                // 도착 후 고민
                myData.currentState = CustomerData.State.Deciding;
                animator.SetFloat("Speed", 0f); // 도착하면 멈춤 애니메이션
                yield return new WaitForSeconds(2f);

                // 재고 확인 및 구매/실망 로직 (기존과 동일)
                if (HasItemsInTable(targetTable.ID))
                {
                    // ... 구매 로직 ...
                    myData.currentState = CustomerData.State.MovingToCashier;
                    CashierManager.Instance.JoinQueue(this);

                    while (!CashierManager.Instance.IsItMyTurn(this)) yield return null;

                    myData.currentState = CustomerData.State.Paying;
                    yield return new WaitForSeconds(1.5f);
                    CashierManager.Instance.LeaveQueue(this);
                }
                else
                {
                    ShowSpeechBubble("다 팔렸나보네...");
                    yield return new WaitForSeconds(1f);
                }
            }
            else
            {
                Debug.LogWarning($"ID {targetTable.ID} 가구 주변에 설 자리가 없습니다.");
            }
        }

        // 4. 퇴장 (입구로)
        if (NPCSpawner.Instance != null)
        {
            yield return StartCoroutine(MoveWithAStar(NPCSpawner.Instance.entranceTransform.position));
        }

        ShopManager.Instance.activeCustomers.Remove(myData);
        Destroy(gameObject);
    }

    // 디버깅용 리스트 (인스펙터에서 볼 필요 없음)
    private List<Vector3> debugCandidates = new List<Vector3>();

    Vector3 GetSmartInteractionPos(Interiorinfo item)
    {
        debugCandidates.Clear(); // 디버깅용 초기화

        // 데이터 좌표 -> 타일 좌표 변환
        Vector3Int startNode = pathfinding.IndexToPos(item.placement);

        // 가구의 점유 범위 계산
        int minX = startNode.x;
        int maxX = startNode.x + item.Width - 1;
        int topY = startNode.y;                 // 위쪽 끝 (벽 쪽)
        int bottomY = startNode.y - item.Height + 1; // 아래쪽 끝 (앞 쪽)

        List<Vector3Int> validTiles = new List<Vector3Int>();

        // ====================================================
        // 1순위: 가구의 '앞쪽' (아래, Y - 1)
        // ====================================================
        int frontY = bottomY - 1;
        for (int x = minX; x <= maxX; x++)
        {
            Vector3Int pos = new Vector3Int(x, frontY, 0);
            if (CheckPos(pos)) validTiles.Add(pos);
        }

        // 앞쪽에 자리가 있으면 즉시 반환 (옆/뒤 볼 필요 없음)
        if (validTiles.Count > 0)
        {
            Vector3Int chosen = validTiles[Random.Range(0, validTiles.Count)];
            return walkTilemap.GetCellCenterWorld(chosen);
        }

        // ====================================================
        // 2순위: 가구의 '양 옆' (좌/우)
        // ====================================================
        // 왼쪽 (minX - 1)
        for (int y = bottomY; y <= topY; y++)
        {
            Vector3Int pos = new Vector3Int(minX - 1, y, 0);
            if (CheckPos(pos)) validTiles.Add(pos);
        }
        // 오른쪽 (maxX + 1)
        for (int y = bottomY; y <= topY; y++)
        {
            Vector3Int pos = new Vector3Int(maxX + 1, y, 0);
            if (CheckPos(pos)) validTiles.Add(pos);
        }

        // 옆쪽에 자리가 있으면 반환
        if (validTiles.Count > 0)
        {
            Vector3Int chosen = validTiles[Random.Range(0, validTiles.Count)];
            return walkTilemap.GetCellCenterWorld(chosen);
        }

        // ====================================================
        // 3순위: 가구의 '뒤쪽' (위, Y + 1) - 최후의 수단
        // ====================================================
        int backY = topY + 1;
        for (int x = minX; x <= maxX; x++)
        {
            Vector3Int pos = new Vector3Int(x, backY, 0);
            if (CheckPos(pos)) validTiles.Add(pos);
        }

        if (validTiles.Count > 0)
        {
            Vector3Int chosen = validTiles[Random.Range(0, validTiles.Count)];
            return walkTilemap.GetCellCenterWorld(chosen);
        }

        // 여기까지 왔다면 사방이 다 막힌 것
        Debug.LogWarning($"[NPC] ID {item.ID} 가구는 사방이 막혀 접근 불가!");
        return Vector3.zero;
    }

    // 좌표 체크 헬퍼 함수 (코드 중복 방지)
    bool CheckPos(Vector3Int pos)
    {
        // 디버깅을 위해 기즈모 리스트에 추가
        debugCandidates.Add(walkTilemap.GetCellCenterWorld(pos));

        // Pathfinding을 통해 갈 수 있는지(바닥O, 벽X, 장애물X) 확인
        return pathfinding.IsWalkable(pos);
    }

    // 씬 뷰에서 후보 위치를 보여줌
    void OnDrawGizmos()
    {
        Gizmos.color = Color.cyan;
        foreach (var pos in debugCandidates)
        {
            Gizmos.DrawSphere(pos, 0.3f);
        }

        // 현재 가려고 하는 목표 지점은 빨간색
        if (pathfinding != null && myData != null)
        {
            Gizmos.color = Color.red;
            // 만약 이동 중이라면 목표 지점을 표시
            // (변수로 저장해둔 게 있다면 사용, 없으면 생략)
        }
    }

    void CheckAndAddCandidate(Vector3Int pos, List<Vector3Int> list)
    {
        // IsWalkable 내부에서 WallTilemap 체크가 추가되었으므로 벽도 자동으로 걸러짐
        if (pathfinding.IsWalkable(pos))
        {
            list.Add(pos);
        }
    }

    


    Interiorinfo GetAnyTableInfo()
    {
        var tables = ShopStorageDataManager.Instance.interiorData.Table;
        if (tables == null || tables.Count == 0) return null;
        return tables[Random.Range(0, tables.Count)];
    }

    bool HasItemsInTable(int tableID)
    {
        if (ShopStorageDataManager.Instance.GetTableClass(tableID, out var table))
        {
            return table.count.Exists(c => c > 0);
        }
        return false;
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
        // 1. 이동 시작 전에 방향을 먼저 계산합니다.
        // (목표지점 - 현재위치)를 하면 바라봐야 할 방향 벡터가 나옵니다.
        Vector3 direction = (targetPos - transform.position).normalized;

        // 2. 아까 만든 SetDirection 함수로 방향을 고정시킵니다.
        // (NPC가 몸을 먼저 돌리고 이동을 시작하게 됩니다)
        SetDirection(direction);

        // 3. 목표 지점까지 이동
        while (Vector3.Distance(transform.position, targetPos) > 0.05f)
        {
            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);

            // 💡 중요: 여기서 UpdateAnimation을 또 호출하면 안 됩니다!
            // 이미 위에서 방향을 고정했기 때문에, 이동만 하면 됩니다.
            yield return null;
        }

        // 4. 도착 완료 처리
        transform.position = targetPos; // 좌표를 깔끔하게 맞춤
        animator.SetFloat("Speed", 0f); // 걷기 모션 정지
    }

    // NPCAI.cs 내부 수정
    public Vector3 GetFurnitureFrontPos(Interiorinfo targetItem)
    {
        // 1. 가구의 왼쪽 위 타일 좌표를 가져옵니다 (Pathfinding의 보정된 함수 사용)
        Vector3Int startTile = pathfinding.IndexToPos(targetItem.placement);

        // 2. 가구 너비 중 랜덤 위치 선정
        int randomXOffset = Random.Range(0, targetItem.Width);

        // 3. 최종 목적지 타일 계산
        // x는 오른쪽으로 더하고, y는 아래쪽(앞쪽)으로 가야 하므로 Height만큼 뺍니다.
        int targetX = startTile.x + randomXOffset;
        int targetY = startTile.y - targetItem.Height; // 유니티 좌표계에서 아래는 -Y

        Vector3Int targetTile = new Vector3Int(targetX, targetY, 0);

        // 4. 장애물 체크 및 월드 좌표 반환
        if (!pathfinding.IsWalkable(targetTile))
        {
            targetX = startTile.x + (targetItem.Width / 2);
            targetTile = new Vector3Int(targetX, targetY, 0);
        }

        return walkTilemap.GetCellCenterWorld(targetTile);
    }


}