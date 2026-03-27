using UnityEngine;
using UnityEngine.Tilemaps;

// DB에 저장하기 좋게 X, Y를 묶어둔 데이터 클래스 (JSON 변환용)
[System.Serializable]
public class GridOriginData
{
    public int startX;
    public int startY;
}

public class GridOriginManager : MonoBehaviour
{
    [Header("타일맵 및 마커 연결")]
    public Tilemap targetTilemap;   // 바닥 타일맵 연결
    public Transform topLeftMarker; // 왼쪽 위에 올려둔 빈 오브젝트 연결

    [Header("확인용 (코드에서 쓰는 진짜 좌표)")]
    public Vector3Int currentOrigin;

    // ==============================================================
    // 1. 에디터에서 우클릭으로 실행! (마커 위치 추출 후 DB 저장)
    // ==============================================================
    [ContextMenu("1. 마커 위치 추출해서 DB에 저장하기")]
    void Start()
    {
        if (targetTilemap == null || topLeftMarker == null)
        {
            Debug.LogError("🚨 타일맵이나 마커가 연결되지 않았습니다!");
            return;
        }

        // 1) 마커의 월드 좌표를 타일맵 기준 그리드(Cell) 좌표로 변환
        Vector3Int cellPos = targetTilemap.WorldToCell(topLeftMarker.position);
        currentOrigin = cellPos;

        Debug.Log($"📍 [좌표 추출 성공] 타일 X: {cellPos.x}, Y: {cellPos.y}");
    }
}