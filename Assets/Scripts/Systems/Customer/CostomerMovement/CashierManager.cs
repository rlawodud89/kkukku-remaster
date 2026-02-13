using UnityEngine;
using System.Collections.Generic;

public class CashierManager : MonoBehaviour
{
    public static CashierManager Instance;
    public float queueSpacing = 1.0f; // 줄 서는 간격
    public Pathfinding pathfinding;

    public int cashierPosIndex;
    public int cashierWidth;
    // 현재 줄 서 있는 NPC 리스트
    private List<NPCAI> waitingQueue = new List<NPCAI>();

    void Awake() { Instance = this; }

    public float interactionDistance = 0.6f; // 💡 이 값을 조절해서 바짝 붙이세요!
                                             // 1.0f = 한 칸 아래, 0.5f = 반 칸 아래

    public Vector3 GetCashierPosition()
    {
        // 1. 계산대 타일과 너비 계산 (기존 동일)
        Vector3Int leftTilePos = pathfinding.IndexToPos(cashierPosIndex);
        Vector3Int rightTilePos = leftTilePos;
        if (cashierWidth > 1) rightTilePos.x += (cashierWidth - 1);

        Vector3 leftWorld = pathfinding.walkTilemap.GetCellCenterWorld(leftTilePos);
        Vector3 rightWorld = pathfinding.walkTilemap.GetCellCenterWorld(rightTilePos);
        
        Vector3 center = (leftWorld + rightWorld) / 2f;

        // 2. 🚨 수정된 부분: interactionDistance 변수 사용
        // Vector3.down * 1.0f 대신 변수를 곱해줍니다.
        return center + (Vector3.down * interactionDistance); 
    }

}