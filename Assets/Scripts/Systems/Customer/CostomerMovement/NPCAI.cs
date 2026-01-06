using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections;
using System.Collections.Generic; // List 사용을 위해 추가

public class NPCAI : MonoBehaviour
{
    public CustomerData myData;
    public Tilemap walkTilemap; // 바닥 타일맵
    public Pathfinding pathfinding; // A* 스크립트 연결을 위해 추가
    public float moveSpeed = 2f;
    private SpriteRenderer sr;

    void Start()
    {
        sr = GetComponent<SpriteRenderer>();
        // 씬에 있는 Pathfinding 스크립트를 자동으로 찾거나 인스펙터에서 할당하세요.
        if (pathfinding == null) pathfinding = FindObjectOfType<Pathfinding>();

        StartCoroutine(BehaviorRoutine());
    }

    void Update()
    {
        // Y축 기반 정렬 (장애물 앞/뒤 처리)
        sr.sortingOrder = Mathf.RoundToInt(transform.position.y * -100);
    }

    IEnumerator BehaviorRoutine()
    {
        // 1. 이불장 이동 및 선택
        Vector3 targetPos = GetRandomWardrobePos();
        yield return StartCoroutine(MoveWithAStar(targetPos));

        myData.currentState = CustomerData.State.Deciding;
        yield return new WaitForSeconds(2f);

        // 이불 결정 및 시각화
        myData.selectedItemID = 0;
        VisualizedSelectedItem(); // 이불 아이콘 띄우기

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
        itemDisplaySR.gameObject.SetActive(false); // 이불 아이콘 끄기

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
                    yield return null;
                }
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

    public SpriteRenderer itemDisplaySR; // NPC 머리 위에 배치한 자식 오브젝트의 SpriteRenderer

    // 아이템 이미지를 보여주는 함수
    public void VisualizedSelectedItem()
    {
        if (myData.selectedItemID != -1)
        {
            // ShopManager의 데이터베이스에서 ID로 아이템 데이터를 찾아 이미지 적용
            ItemData selectedItem = ShopManager.Instance.itemDatabase.Find(x => x.itemID == myData.selectedItemID);
            if (selectedItem != null)
            {
                itemDisplaySR.sprite = selectedItem.itemSprite;
                itemDisplaySR.gameObject.SetActive(true); // 이미지 활성화
            }
        }
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
            yield return null;
        }
    }
}