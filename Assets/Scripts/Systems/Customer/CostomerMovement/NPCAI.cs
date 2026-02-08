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

    [Header("Shopping Info")]
    private int selectedTableID;      // 어느 가구에서 가져왔는지
    private int selectedItemIndex;    // 그 가구의 몇 번째 칸인지
    private string selectedItemName;  // 물건 이름
    private int priceToPay;           // 내야 할 돈

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
        if (dir.sqrMagnitude < 0.02f) return;

        lastMoveDir = dir;
        animator.SetFloat("MoveX", dir.x);
        animator.SetFloat("MoveY", dir.y);
        animator.SetFloat("Speed", 1f); // 움직인다고 알림
    }

    IEnumerator MoveToQueueRoutine(Vector3 targetPos)
    {
        // 1. 멀면 A* (장애물 회피)
        float dist = Vector3.Distance(transform.position, targetPos);
        if (dist > 1.5f)
        {
            // 목표지점 자체가 장애물일 수 있으므로 살짝 앞에서 A* 종료
            Vector3 safeSpot = targetPos + Vector3.down * 0.5f;
            yield return StartCoroutine(MoveWithAStar(safeSpot));
        }

        // 2. 가까우면 직선 이동 (정밀 주차)
        float timeOut = 3.0f;

        while (Vector3.Distance(transform.position, targetPos) > 0.05f && timeOut > 0)
        {
            timeOut -= Time.deltaTime;

            float currentDist = Vector3.Distance(transform.position, targetPos);

            // 🚨 핵심 수정: 거리가 너무 가까우면(0.1f 미만) 방향을 바꾸지 않음!
            // (도착 직전에 벡터가 뒤집히는 것을 방지)
            if (currentDist > 0.1f)
            {
                Vector3 direction = (targetPos - transform.position).normalized;
                SetDirection(direction);
            }

            transform.position = Vector3.MoveTowards(transform.position, targetPos, moveSpeed * Time.deltaTime);
            yield return null;
        }

        // 3. 도착 확정
        transform.position = targetPos;
        animator.SetFloat("Speed", 0f);
        SetDirection(Vector3.up);

        // (혹시 애니메이션이 튀는 걸 방지하기 위해 한 프레임 대기 후 한번 더 고정)
        yield return null;
        animator.SetFloat("MoveX", 0);
        animator.SetFloat("MoveY", 1); // 1 = 위쪽(뒷모습)
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

    // 코드 중복을 줄이기 위해 계산대 이동 로직 분리
    IEnumerator GoToCashierAndPay()
    {
        Vector3 cashierPos = CashierManager.Instance.GetCashierPosition();
        myData.currentState = CustomerData.State.MovingToCashier;
        yield return StartCoroutine(MoveToQueueRoutine(cashierPos));

        myData.currentState = CustomerData.State.Paying;
        SetDirection(Vector3.up);
        animator.SetFloat("Speed", 0f);

        yield return new WaitForSeconds(1.5f);

        ServiceLocator.Get<GameData>().User.ChangeGold(priceToPay);
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

                // (선택사항) 이불장 이미지가 비어야 하니까 즉시 갱신
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
}