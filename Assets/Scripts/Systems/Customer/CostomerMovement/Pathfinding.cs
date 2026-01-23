using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public class Pathfinding : MonoBehaviour
{

    public Tilemap walkTilemap; // 바닥 타일맵
    public LayerMask obstacleLayer; // 유니티 인스펙터에서 'Obstacle' 레이어를 선택해주세요.

    public List<Vector3Int> FindPath(Vector3Int startPos, Vector3Int targetPos)
    {
        List<Node> openList = new List<Node>();   // 조사할 후보들
        HashSet<Vector3Int> closedList = new HashSet<Vector3Int>(); // 조사 완료된 좌표들

        Node startNode = new Node(true, startPos);
        openList.Add(startNode);

        while (openList.Count > 0)
        {
            // 1. fCost가 가장 낮은 노드를 현재 노드로 선택
            Node currentNode = openList.OrderBy(n => n.fCost).ThenBy(n => n.hCost).First();

            openList.Remove(currentNode);
            closedList.Add(currentNode.gridPos);

            // 2. 목표 도착 시 경로 반환
            if (currentNode.gridPos == targetPos)
            {
                return RetracePath(startNode, currentNode);
            }

            // 3. 주변 4방향 타일 조사
            foreach (Vector3Int neighborPos in GetNeighbors(currentNode.gridPos))
            {
                // 이미 조사했거나 이동 불가능(가구 등)하면 패스
                if (closedList.Contains(neighborPos) || !IsWalkable(neighborPos)) continue;

                int newCostToNeighbor = currentNode.gCost + 10; // 한 칸 이동 비용 10
                Node neighborNode = openList.FirstOrDefault(n => n.gridPos == neighborPos);

                if (neighborNode == null)
                {
                    neighborNode = new Node(true, neighborPos);
                    neighborNode.gCost = newCostToNeighbor;
                    neighborNode.hCost = GetDistance(neighborPos, targetPos) * 10;
                    neighborNode.parent = currentNode;
                    openList.Add(neighborNode);
                }
                else if (newCostToNeighbor < neighborNode.gCost)
                {
                    neighborNode.gCost = newCostToNeighbor;
                    neighborNode.parent = currentNode;
                }
            }
        }
        return null; // 경로 없음
    }

    // 장애물 체크: 바닥은 있고, 가구는 없는지 확인

    bool IsWalkable(Vector3Int pos)
    {
        // 1. 우선 바닥 타일 자체가 있는지 확인 (길이 없는 곳은 못 가니까요)
        if (!walkTilemap.HasTile(pos)) return false;

        // 2. 타일의 중앙 월드 좌표를 가져옵니다.
        Vector3 worldPos = walkTilemap.GetCellCenterWorld(pos);

        // 3. 해당 위치에 장애물 오브젝트(Collider2D)가 있는지 확인합니다.
        // new Vector2(0.8f, 0.8f)는 체크할 박스의 크기입니다. 
        // 타일 크기(1.0)보다 살짝 작게 잡아야 옆 타일에 있는 장애물과 간섭이 생기지 않습니다.
        Collider2D hit = Physics2D.OverlapBox(worldPos, new Vector2(0.8f, 0.8f), 0, obstacleLayer);

        // 4. 장애물(hit)이 아무것도 검출되지 않았다면(null) 갈 수 있는 길(true)입니다.
        return hit == null;
    }

    // 상하좌우 이웃 타일 가져오기
    List<Vector3Int> GetNeighbors(Vector3Int pos)
    {
        return new List<Vector3Int> {
            pos + Vector3Int.up, pos + Vector3Int.down,
            pos + Vector3Int.left, pos + Vector3Int.right
        };
    }

    // 부모 노드를 따라가며 최종 경로 리스트 생성
    List<Vector3Int> RetracePath(Node start, Node end)
    {
        List<Vector3Int> path = new List<Vector3Int>();
        Node curr = end;
        while (curr != start)
        {
            path.Add(curr.gridPos);
            curr = curr.parent;
        }
        path.Reverse();
        return path;
    }

    int GetDistance(Vector3Int a, Vector3Int b)
    {
        return Mathf.Abs(a.x - b.x) + Mathf.Abs(a.y - b.y);
    }
}