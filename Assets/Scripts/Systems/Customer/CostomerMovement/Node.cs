using UnityEngine;

public class Node
{
    public bool walkable;     // 이동 가능 여부 (장애물 체크 결과)
    public Vector3Int gridPos; // 타일맵 좌표
    public int gCost;         // 시작점에서 이 칸까지의 거리
    public int hCost;         // 이 칸에서 목표점까지의 예상 거리
    public Node parent;       // 경로를 역추적하기 위한 부모 노드

    public int fCost => gCost + hCost; // 총 비용

    public Node(bool _walkable, Vector3Int _gridPos)
    {
        walkable = _walkable;
        gridPos = _gridPos;
    }
}
