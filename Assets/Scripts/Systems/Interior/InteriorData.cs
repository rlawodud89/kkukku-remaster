using System.Collections.Generic;
using System.Linq; // ★ Linq 필수 (Find, Any 등 사용)
using UnityEngine;

[System.Serializable]
public class InteriorData
{
    // 실제 저장될 가구 리스트
    public List<RoomInteriorPlaced> placedItems = new List<RoomInteriorPlaced>();

    // 그리드 설정 (가로 폭이 있어야 2차원 좌표 변환 가능)
    // InteriorManager의 gridWidth와 맞춰야 합니다.
    private int mapWidth = 10; 

    // =================================================================
    // ★ [핵심] 가구 추가 및 ID 발급 함수
    // =================================================================
    public int AddRoomInterior(int gridNumber, string itemName)
    {
        // 1. 유효성 검사: 이미 해당 그리드에 가구가 있는지 확인
        if (IsGridOccupied(gridNumber))
        {
            Debug.LogWarning($"[Data] 그리드 {gridNumber}번은 이미 점유되어 있습니다.");
            return -1; // 실패 시 -1 반환
        }

        // 2. 새로운 ID 발급 (현재 가장 높은 ID + 1)
        int newID = GenerateNewID();

        // 3. 데이터 생성 및 리스트 추가
        RoomInteriorPlaced newItem = new RoomInteriorPlaced
        {
            ID = newID,
            gridNumber = gridNumber,
            itemName = itemName
        };

        placedItems.Add(newItem);

        Debug.Log($"[Data] 저장 완료 : {itemName} (ID: {newID}, Grid: {gridNumber})");
        return newID; // 성공 시 ID 반환
    }

    // =================================================================
    // [기능] 가구 삭제 (필요할 경우 사용)
    // =================================================================
    public bool RemoveRoomInterior(int id)
    {
        var target = placedItems.Find(x => x.ID == id);
        if (target != null)
        {
            placedItems.Remove(target);
            return true;
        }
        return false;
    }

    // =================================================================
    // [헬퍼] 이미 있는 자리인지 확인 (중복 설치 방지)
    // =================================================================
    public bool IsGridOccupied(int gridIndex)
    {
        // 리스트 중에 같은 gridNumber를 가진 녀석이 있는지 검사
        return placedItems.Any(x => x.gridNumber == gridIndex);
    }

    // =================================================================
    // [헬퍼] ID 생성기 (Auto Increment)
    // =================================================================
    private int GenerateNewID()
    {
        if (placedItems.Count == 0) return 1; // 첫 아이템은 1번
        
        // 리스트에서 가장 큰 ID를 찾아 +1 함
        return placedItems.Max(x => x.ID) + 1;
    }
    
    // 리스트 전체 반환 (로드용)
    public List<RoomInteriorPlaced> GetCurrentRoomInterior()
    {
        return placedItems;
    }

    // [저장용] 덮어쓰기 (InteriorManager에서 SaveAllFurniture 호출 시 사용)
    public void SaveRoomInterior(List<RoomInteriorPlaced> newList)
    {
        placedItems = newList;
    }

    // =================================================================
    // ★ [요청하신 기능] 그리드 변환 함수들 (Index <-> X, Y)
    // =================================================================
    
    // 1. (X, Y) 좌표 -> 그리드 번호(Index) 변환
    public int ToGridIndex(int x, int y)
    {
        return x + (y * mapWidth);
    }

    // 2. 그리드 번호(Index) -> (X, Y) 좌표 변환
    public Vector2Int ToGridCoords(int index)
    {
        int x = index % mapWidth;
        int y = index / mapWidth;
        return new Vector2Int(x, y);
    }
}