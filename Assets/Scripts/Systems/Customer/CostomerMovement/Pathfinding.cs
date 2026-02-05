using UnityEngine;
using UnityEngine.Tilemaps;
using System.Collections.Generic;
using System.Linq;

public static class GridSystem
{
    public static Vector3Int IndexToPos(int index, int totalWidth, int totalHeight)
    {
        int x = index % totalWidth;
        int y = index / totalWidth;
        int offsetX = totalWidth / 2;
        int offsetY = totalHeight / 2;
        return new Vector3Int(x - offsetX, offsetY - y, 0);
    }
}

public class Pathfinding : MonoBehaviour
{

    public Tilemap walkTilemap; // 바닥 타일맵
    public LayerMask obstacleLayer; // 유니티 인스펙터에서 'Obstacle' 레이어를 선택해주세요.


    public int totalGridWidth; // 인스펙터에서 설정
    public int totalGridHeight;

    private HashSet<Vector3Int> obstacleTiles = new HashSet<Vector3Int>();

    public void BuildObstacleMap(ShopInteriorData data)
    {
        obstacleTiles.Clear();

        // 1. 모든 가구 리스트 합치기
        var allItems = new List<Interiorinfo>();
        if (data.Casher != null) allItems.Add(data.Casher);
        allItems.AddRange(data.Interior);
        allItems.AddRange(data.Table);

        foreach (var item in allItems)
        {
            // 가구의 시작점(왼쪽 위)
            Vector3Int startPos = IndexToPos(item.placement);

            // 가구 크기만큼 점유 처리
            for (int w = 0; w < item.Width; w++)
            {
                for (int h = 0; h < item.Height; h++)
                {
                    // 왼쪽 위 기준이므로 x는 +, y는 - 방향으로 확장
                    Vector3Int occupied = startPos + new Vector3Int(w, -h, 0);
                    obstacleTiles.Add(occupied);
                }
            }
        }
    }

    private Vector3Int IndexToPos(int index) => new Vector3Int(index % totalGridWidth, -(index / totalGridWidth), 0);

    // 이제 IsWalkable은 물리 체크 없이 데이터만 봅니다.
    public bool IsWalkable(Vector3Int pos)
    {
        if (!walkTilemap.HasTile(pos)) return false;
        return !obstacleTiles.Contains(pos);
    }

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

    ///////////////
    void OnDrawGizmos()
    {
        if (obstacleTiles == null) return;
        Gizmos.color = Color.red;
        foreach (var tile in obstacleTiles)
        {
            Gizmos.DrawSphere(walkTilemap.GetCellCenterWorld(tile), 0.2f);
        }
    }
}