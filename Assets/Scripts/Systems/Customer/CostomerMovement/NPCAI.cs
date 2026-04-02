using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic; // List 사용을 위해 추가
using TMPro;

public class NPCAI : MonoBehaviour
{
    public CustomerData myData;
    public Tilemap walkTilemap; // 바닥 타일맵
    public Pathfinding pathfinding; // A* 스크립트 연결을 위해 추가
    public float moveSpeed = 1f;
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

    [Header("Shopping Info")]
    private int selectedTableID;      // 어느 가구에서 가져왔는지
    private int selectedItemIndex;    // 그 가구의 몇 번째 칸인지
    private string selectedItemName;  // 물건 이름
    private int priceToPay;           // 내야 할 돈


    void Awake()
    {
        sr = GetComponent<SpriteRenderer>();
        animator = GetComponent<Animator>();

        // Pathfinding은 Awake에서 미리 찾아둡니다. 
        // (Start에서 찾으면 늦을 수 있음)
        if (pathfinding == null)
        {
            pathfinding = FindObjectOfType<Pathfinding>();
            if (pathfinding != null) walkTilemap = pathfinding.walkTilemap;
        }
    }

    // 💡 [추가] 생성되자마자 즉시 호출될 초기화 함수
    public void SetupSurvivor(CustomerData data)
    {
        myData = data;

        // 데이터 매니저가 혹시라도 준비 안 됐을 경우를 대비한 안전장치
        if (ShopStorageDataManager.Instance == null) return;

        // 생존자(기존 손님)라면 즉시 위치 이동
        if (myData.isSurvivor)
        {
            // 1. 쇼핑 중이었던 경우
            if (myData.currentState == CustomerData.State.Deciding ||
                myData.currentState == CustomerData.State.MovingToWardrobe)
            {
                Interiorinfo targetTable = GetAnyTableInfo(); // 랜덤 테이블 or 기존 타겟
                if (targetTable != null)
                {
                    Vector3 standPos = GetSmartInteractionPos(targetTable);
                    if (standPos != Vector3.zero)
                    {
                        transform.position = standPos; // ✨ 순간이동!
                        SetFaceToFurniture(targetTable);

                        // 이미 자리를 잡았으니 걸어가는 애니메이션 끄기
                        if (animator != null) animator.SetFloat("Speed", 0f);
                    }
                }
            }
            // 2. 계산 중이었던 경우
            else if (myData.currentState == CustomerData.State.Paying ||
                     myData.currentState == CustomerData.State.MovingToCashier)
            {
                Vector3 cashierPos = CashierManager.Instance.GetCashierPosition();
                transform.position = cashierPos; // ✨ 순간이동!
                SetDirection(Vector3.up);
                if (animator != null) animator.SetFloat("Speed", 0f);
            }
        }
    }

    void Start()
    {
        lastPosition = transform.position;

        // SetupSurvivor에서 pathfinding을 못 찾았을 수도 있으니 재확인
        if (pathfinding == null)
        {
            pathfinding = FindObjectOfType<Pathfinding>();
            walkTilemap = pathfinding.walkTilemap;
        }

        // 기존 로직 유지
        if (npcBaseData != null && npcBaseData.questProgress == 1)
        {
            currentBehavior = NPCBehavior.SpecialQuest;
        }

        StartCoroutine(InitAndStartBehavior());
    }

    IEnumerator InitAndStartBehavior()
    {
        // 0.5초 대기는 "새로 들어오는 손님"이나 "데이터 로딩 대기"를 위해 유지하되,
        // 이미 자리를 잡은 생존자(Survivor)는 위치 이동 로직을 건너뛰게 해야 함
        yield return new WaitForSeconds(0.5f);

        if (ShopStorageDataManager.Instance == null || ShopStorageDataManager.Instance.interiorData == null)
        {
            Debug.LogError("가구 데이터 로드 실패");
            yield break;
        }

        StartCoroutine(MainBehaviorRoutine());
    }



    // 1. 캐릭터 클릭 감지
    void OnMouseDown()
    {
        string talk = npcBaseData.smallTalks[Random.Range(0, npcBaseData.smallTalks.Length)];

        if (currentBehavior != NPCBehavior.SpecialQuest)
        {
            ShowSpeechBubble(talk);
        }

        QuestManager.Instance.UpdateQuestProgressByID(4);
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
        if (currentBehavior != NPCBehavior.SpecialQuest)
        {
            if (myData.isSurvivor)
            {
                // 💡 [수정] SetupSurvivor에서 이미 위치를 잡았으므로,
                // 여기서는 '순간이동' 코드를 빼고 '행동(말풍선, 대기)'만 수행하도록 로직 간소화

                myData.isSurvivor = false; // 플래그 해제

                switch (myData.currentState)
                {
                    case CustomerData.State.Deciding:
                    case CustomerData.State.MovingToWardrobe: // 이 상태들도 그냥 구경하는 척
                        yield return new WaitForSeconds(2f); // 고민하는 척

                        // 이후 로직은 동일 (구매 시도 -> 계산대 이동)
                        // 현재 서 있는 가구(랜덤으로 잡은 곳)의 ID를 찾아야 하는데,
                        // 복잡하니까 그냥 바로 구매 시도 로직(GetAnyTableInfo 등 활용)으로 넘김
                        Interiorinfo targetTable = GetRandomTableWithStock();
                        if (targetTable != null && TryPickItem(targetTable.ID))
                        {
                            yield return StartCoroutine(GoToCashierRoutine());
                        }
                        else
                        {
                            yield return StartCoroutine(LeaveShop());
                        }
                        break;

                    case CustomerData.State.Paying:
                    case CustomerData.State.MovingToCashier: // 이미 계산대에 있음
                        yield return new WaitForSeconds(1.5f);
                        GameManager.Instance.ChangeGold(priceToPay > 0 ? priceToPay : 100); // 가격 정보 없으면 기본값
                        yield return StartCoroutine(LeaveShop());
                        break;
                }
            }
            else
            {
                // 신규 손님은 입구부터 걸어옴 (기존 유지)
                yield return StartCoroutine(NormalShopRoutine());
            }
        }
    }

    // --- [생존자 행동 1] 쇼핑 중이었던 척 하기 ---
    IEnumerator SurvivorDecidingRoutine()
    {
        // 1. 랜덤 가구 앞으로 순간이동
        Interiorinfo targetTable = GetAnyTableInfo(); // (기존 함수 활용)

        if (targetTable != null)
        {
            Vector3 standPos = GetSmartInteractionPos(targetTable);
            if (standPos != Vector3.zero)
            {
                transform.position = standPos;
                SetFaceToFurniture(targetTable); // 방향 설정
                animator.SetFloat("Speed", 0f);  // 멈춤

                yield return new WaitForSeconds(2f);

                // 3. 이후 로직은 똑같음 (집기 시도 -> 계산대 이동)
                if (TryPickItem(targetTable.ID))
                {
                    yield return StartCoroutine(GoToCashierRoutine());
                }
                else
                {
                    // 없으면 퇴장
                    yield return StartCoroutine(LeaveShop());
                }
            }
        }
        else
        {
            yield return StartCoroutine(LeaveShop());
        }
    }

    // --- [생존자 행동 2] 계산 중이었던 척 하기 ---
    IEnumerator SurvivorPayingRoutine()
    {
        // 1. 계산대 앞으로 순간이동
        Vector3 cashierPos = CashierManager.Instance.GetCashierPosition();
        transform.position = cashierPos;

        // 2. 방향 위로, 멈춤
        SetDirection(Vector3.up);
        animator.SetFloat("Speed", 0f);

        yield return new WaitForSeconds(1.5f);

        // 4. 돈 처리
        GameManager.Instance.ChangeGold(priceToPay); // (priceToPay는 데이터에서 복구하거나 랜덤값)

        // 5. 퇴장
        yield return StartCoroutine(LeaveShop());
    }

    // (참고) 코드 중복을 줄이기 위한 계산대 이동 함수
    IEnumerator GoToCashierRoutine()
    {
        Vector3 cashierPos = CashierManager.Instance.GetCashierPosition();
        myData.currentState = CustomerData.State.MovingToCashier;

        yield return StartCoroutine(MoveToQueueRoutine(cashierPos)); // 걸어가기

        // 도착하면 바로 Paying 루틴으로 이어짐 (혹은 여기서 처리)
        yield return StartCoroutine(SurvivorPayingRoutine());
        // 주의: SurvivorPayingRoutine은 순간이동을 포함하므로, 
        // 걸어온 경우에는 순간이동 코드를 뺀 순수 계산 로직만 실행하도록 분리하는 게 좋습니다.
    }

    // 기존 UpdateAnimation 함수는 삭제하거나 사용하지 않습니다.
    // 대신 아래 함수를 새로 만듭니다.
    void SetDirection(Vector3 dir)
    {
        // 방향 벡터가 너무 작으면 무시 (0일 때 휙 도는 것 방지)
        if (dir.sqrMagnitude < 0.02f) return;

        lastMoveDir = dir;
        animator.SetFloat("MoveX", dir.x);
        animator.SetFloat("MoveY", dir.y);
        animator.SetFloat("Speed", 1f); // 움직인다고 알림
    }

    IEnumerator MoveToQueueRoutine(Vector3 targetPos)
    {
        // 1. 계산대 목표 위치가 속한 타일의 '정중앙' 좌표를 먼저 구합니다.
        Vector3Int targetTile = walkTilemap.WorldToCell(targetPos);
        Vector3 tileCenterPos = walkTilemap.GetCellCenterWorld(targetTile);

        // 2. 무조건 타일 중앙까지는 A* 길찾기로 90도씩 꺾어서 정상적으로 이동합니다.
        yield return StartCoroutine(MoveWithAStar(tileCenterPos));

        // 3. 도착한 타일 중앙에서 계산대 전용 미세 좌표(targetPos)로 살짝만 이동 (정밀 주차)
        // 거리가 매우 짧으므로 미끄러지는 느낌 없이 자연스럽게 자리를 잡습니다.
        float timeOut = 1.0f; // 타임아웃도 짧게 줄임

        while (Vector3.Distance(transform.position, targetPos) > 0.05f && timeOut > 0)
        {
            timeOut -= Time.deltaTime;

            float currentDist = Vector3.Distance(transform.position, targetPos);

            if (currentDist > 0.1f)
            {
                Vector3 direction = (targetPos - transform.position).normalized;
                SetDirection(direction);
            }

            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // 4. 도착 확정 및 뒷모습(계산대 방향) 고정
        transform.position = targetPos;
        animator.SetFloat("Speed", 0f);
        SetDirection(Vector3.up);

        yield return null;
        animator.SetFloat("MoveX", 0);
        animator.SetFloat("MoveY", 1); // 뒷모습
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
    // [NPCAI.cs 수정]

    IEnumerator NormalShopRoutine()
    {
        // 1. 첫 번째 시도: 일단 아무 테이블이나 무작위 선정
        Interiorinfo targetTable = GetAnyTableInfo();

        // (함수 분리: 이동 및 대기 로직을 재사용하기 위해)
        yield return StartCoroutine(VisitTableAndCheck(targetTable));
    }

    // 가구 방문 및 구매 시도를 처리하는 서브 코루틴
    IEnumerator VisitTableAndCheck(Interiorinfo targetTable)
    {
        if (targetTable == null) yield break;

        // --- [이동] ---
        Vector3 targetPos = GetSmartInteractionPos(targetTable);
        if (targetPos != Vector3.zero)
        {
            yield return StartCoroutine(MoveWithAStar(targetPos));

            // --- [도착 및 방향 전환] ---
            myData.currentState = CustomerData.State.Deciding;
            animator.SetFloat("Speed", 0f);

            // 방향 보기 (아까 만든 로직)
            Vector3Int myGridPos = walkTilemap.WorldToCell(transform.position);
            Vector3Int tableOrigin = pathfinding.IndexToPos(targetTable.placement);
            int tMinX = tableOrigin.x;
            int tMaxX = tableOrigin.x + targetTable.Width - 1;
            int tMaxY = tableOrigin.y;
            int tMinY = tableOrigin.y - targetTable.Height + 1;

            Vector3 lookDir = Vector3.zero;
            if (myGridPos.x < tMinX) lookDir = Vector3.right;
            else if (myGridPos.x > tMaxX) lookDir = Vector3.left;
            else if (myGridPos.y < tMinY) lookDir = Vector3.up;
            else if (myGridPos.y > tMaxY) lookDir = Vector3.down;

            if (lookDir != Vector3.zero)
            {
                SetDirection(lookDir);
                animator.SetFloat("Speed", 0f);
            }

            // 2초 고민
            yield return new WaitForSeconds(2f);

            // --- [구매 시도 1차] ---
            if (TryPickItem(targetTable.ID))
            {
                // 성공! 계산대로 이동 (기존 코드)
                yield return StartCoroutine(GoToCashierAndPay());
            }
            else
            {
                // 🚨 실패! (여기가 핵심 변경)
                yield return new WaitForSeconds(1.5f);

                // 2. 다른 재고 있는 테이블 탐색
                Interiorinfo newTarget = GetRandomTableWithStock();

                // (방금 갔던 곳이랑 똑같은 곳이면 굳이 또 안 가고 포기, 혹은 다른 곳이 있으면 이동)
                if (newTarget != null && newTarget.ID != targetTable.ID)
                {

                    // --- [이동 (재시도)] ---
                    // 재귀 호출보다는 코드를 반복하거나, 여기서 바로 이동 로직 수행
                    // 복잡도를 줄이기 위해 여기서 바로 이동합니다.

                    Vector3 newPos = GetSmartInteractionPos(newTarget);
                    yield return StartCoroutine(MoveWithAStar(newPos));

                    // 도착 후 다시 고민
                    animator.SetFloat("Speed", 0f);
                    // (방향 전환 로직은 생략하거나 위와 동일하게 복사)

                    yield return new WaitForSeconds(1.5f); // 2차 고민

                    // --- [구매 시도 2차] ---
                    if (TryPickItem(newTarget.ID))
                    {
                        yield return StartCoroutine(GoToCashierAndPay());
                    }
                    else
                    {
                        // 2번이나 가봤는데도 없으면 진짜 퇴장
                        ShowSpeechBubble("다 팔렸나보네...");
                        yield return new WaitForSeconds(1f);
                        yield return StartCoroutine(LeaveShop());
                    }
                }
                else
                {
                    // 재고 있는 테이블이 아예 없음
                    ShowSpeechBubble("아무것도 없네...");
                    yield return new WaitForSeconds(1f);
                    yield return StartCoroutine(LeaveShop());
                }
            }
        }
        else
        {
            // 갈 자리가 없어서 못 간 경우 -> 바로 퇴장 혹은 다른 테이블 시도
            yield return StartCoroutine(LeaveShop());
        }
    }


    IEnumerator GoToCashierAndPay()
    {
        Vector3 cashierPos = CashierManager.Instance.GetCashierPosition();
        myData.currentState = CustomerData.State.MovingToCashier;
        yield return StartCoroutine(MoveToQueueRoutine(cashierPos));

        myData.currentState = CustomerData.State.Paying;
        SetDirection(Vector3.up);
        animator.SetFloat("Speed", 0f);

        yield return new WaitForSeconds(1.5f);

        GameManager.Instance.ChangeGold(priceToPay);
        Debug.Log($"[판매] {priceToPay}G 획득!");

        yield return StartCoroutine(LeaveShop());
    }

    // 퇴장 로직 분리
    IEnumerator LeaveShop()
    {
        if (NPCSpawner.Instance != null)
            yield return StartCoroutine(MoveWithAStar(NPCSpawner.Instance.entranceTransform.position));

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



    bool TryPickItem(int tableID)
    {
        // 1. 매니저한테 테이블 정보 달라고 함
        if (ShopStorageDataManager.Instance.GetTableClass(tableID, out var table))
        {
            // 2. 재고가 1개 이상인 아이템들의 인덱스를 다 찾음
            List<int> availableIndices = new List<int>();
            for (int i = 0; i < table.count.Count; i++)
            {
                if (table.count[i] > 0) availableIndices.Add(i);
            }

            // 3. 살 게 하나라도 있다면?
            if (availableIndices.Count > 0)
            {
                // 랜덤으로 하나 선택
                int randomIndex = availableIndices[Random.Range(0, availableIndices.Count)];

                // 정보 기억하기
                selectedTableID = tableID;
                selectedItemIndex = randomIndex;
                selectedItemName = table.itemName[randomIndex];

                // 가격 확인
                priceToPay = ServiceLocator.Get<GameData>().Inventory.GetBlanketPrice(selectedItemName);

                // 🚨 핵심 변경: 물건을 집는 순간 즉시 재고 차감! (선점)
                // ---------------------------------------------------------
                ShopStorageDataManager.Instance.UpdateTableData(selectedTableID, selectedItemIndex, -1);

                //이불장 이미지가 비어야 하니까 즉시 갱신
                var allStorages = FindObjectsOfType<ShopStorageClick>();
                foreach (var storage in allStorages)
                {
                    if (storage.storageID == selectedTableID) storage.UpdateSpriteState();
                }
                // ---------------------------------------------------------

                Debug.Log($"[NPC] '{selectedItemName}' 찜함! (이제 다른 애들은 못 가져감)");
                return true; // 성공!
            }
        }
        return false; // 재고 없음
    }


    public void MoveToQueuePoint()
    {
        // 1. 목표 지점 받아오기
        Vector3 targetQueuePos = CashierManager.Instance.GetCashierPosition();

        // 2. 기존 이동 코루틴들 모두 중지 (충돌 방지)
        StopCoroutine("MoveToQueueRoutine");
        StopCoroutine("MoveWithAStar");

        // 3. 새 이동 시작
        StartCoroutine(MoveToQueueRoutine(targetQueuePos));
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

    void OnDestroy()
    {
        // 1. 내가 물건을 집었는데(가격이 있음)
        // 2. 아직 계산 완료 상태(Paying 끝남)가 아니라면? -> 도둑놈이거나 버그임
        // 3. 다시 재고를 +1 해줘야 함

        if (priceToPay > 0 && myData.currentState != CustomerData.State.Paying)
        {
            ShopStorageDataManager.Instance.UpdateTableData(selectedTableID, selectedItemIndex, 1); 
        }

        if (ShopManager.Instance != null)
            ShopManager.Instance.activeCustomers.Remove(myData);
    }

    Interiorinfo GetRandomTableWithStock()
    {
        var allTables = ShopStorageDataManager.Instance.interiorData.Table;
        List<Interiorinfo> validTables = new List<Interiorinfo>();

        foreach (var tableInfo in allTables)
        {
            // 매니저를 통해 실제 재고(TableClass) 확인
            if (ShopStorageDataManager.Instance.GetTableClass(tableInfo.ID, out var tableData))
            {
                // 재고가 하나라도 0보다 큰 게 있는지 확인 (Exists 함수 사용)
                if (tableData.count.Exists(c => c > 0))
                {
                    validTables.Add(tableInfo);
                }
            }
        }

        if (validTables.Count > 0)
        {
            return validTables[Random.Range(0, validTables.Count)];
        }

        return null; // 모든 가구가 텅 빔
    }

    // 가구를 바라보는 방향을 계산해서 적용하는 함수
    void SetFaceToFurniture(Interiorinfo targetTable)
    {
        // 1. 내 위치(그리드)와 테이블 기준점(그리드) 가져오기
        Vector3Int myGridPos = walkTilemap.WorldToCell(transform.position);
        Vector3Int tableOrigin = pathfinding.IndexToPos(targetTable.placement);

        // 2. 테이블의 범위(Bounds) 계산
        int tMinX = tableOrigin.x;
        int tMaxX = tableOrigin.x + targetTable.Width - 1;
        int tMaxY = tableOrigin.y;
        int tMinY = tableOrigin.y - targetTable.Height + 1;

        // 3. 비교해서 방향 정하기
        Vector3 lookDir = Vector3.zero;

        if (myGridPos.x < tMinX) lookDir = Vector3.right; // 왼쪽에서 접근 -> 오른쪽 봄
        else if (myGridPos.x > tMaxX) lookDir = Vector3.left;  // 오른쪽에서 접근 -> 왼쪽 봄
        else if (myGridPos.y < tMinY) lookDir = Vector3.up;    // 아래에서 접근 -> 위쪽 봄 (뒷모습)
        else if (myGridPos.y > tMaxY) lookDir = Vector3.down;  // 위에서 접근 -> 아래쪽 봄 (앞모습)

        // 4. 시선 적용 및 걷기 모션 정지
        if (lookDir != Vector3.zero)
        {
            SetDirection(lookDir);       // 방향 돌리고
            animator.SetFloat("Speed", 0f); // 제자리걸음 방지 (멈춤)
        }
    }

    public void RedirectToNewCashier()
    {
        // 현재 계산대로 걷고 있거나, 줄 서 있거나, 결제 중인 손님만 해당
        if (myData.currentState == CustomerData.State.MovingToCashier ||
            myData.currentState == CustomerData.State.Paying)
        {
            // 1. 하던 코루틴(옛날 위치로 가는 길찾기 등) 전부 정지!
            StopAllCoroutines();

            // 2. 상태를 다시 이동 중으로 돌려놓기
            myData.currentState = CustomerData.State.MovingToCashier;
            animator.SetFloat("Speed", 1f);

            // 3. 바뀐 최신 위치를 향해 계산 프로세스를 다시 시작!
            StartCoroutine(GoToCashierAndPay());
        }
    }
}