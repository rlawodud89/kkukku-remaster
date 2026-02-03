using UnityEngine;
using System.Collections.Generic;

public class WR_StorageListUI : MonoBehaviour
{
    [Header("연결 필요")]
    public Transform contentParent;   // Scroll View의 Content
    public GameObject boxButtonPrefab; // 아래 2번에서 만들 버튼 프리팹
    public RoomInteriorType targetType; // 지금 보고 있는 보관함 타입
    
    private void OnEnable() 
    {
        ShowStorageList();
    }
    
    public void ShowStorageList()
    {
        InteriorManager manager = FindObjectOfType<InteriorManager>();

    // [중요] 아직 매니저가 안 만들어졌거나 못 찾았으면 스톱! (오류 방지)
    if (manager == null)
    {
        Debug.LogWarning("InteriorManager를 찾을 수 없습니다. (아직 생성 전일 수 있음)");
        return;
    }

        // 1. 기존에 떠있던 버튼들 싹 지우기 (초기화)
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 2. 현재 방에 있는 '모든' 가구 리스트 가져오기
        List<RoomInteriorPlaced> allFurniture = manager.currentPlacedList;

        Debug.Log($"[StorageListUI] 방에 있는 가구 총 {allFurniture.Count}개 중, '{targetType}' 타입 보관함을 표시합니다.");
        
        int index = 1;  // display용 인덱스 번호

        // 3. 리스트 돌면서 '요청받은 타입(targetType)'과 똑같은 애들만 생성
        foreach (var furniture in allFurniture)
        {
            if (furniture.interiorType == targetType)
            {
                CreateButton(furniture.ID, index, targetType);
                index++;
            }
        }
    }

    // 버튼 실제 생성 함수
    void CreateButton(int dbID, int displayIndex, RoomInteriorType type)
    {
        GameObject btn = Instantiate(boxButtonPrefab, contentParent);
        
        // 버튼 스크립트에 정보 주입
        var script = btn.GetComponent<WR_StorageSelectButton>();
        if (script != null)
        {
            // "너는 이불함이고, ID는 105번이고, 화면엔 '1번함'이라고 표시해"
            script.Setup(dbID, displayIndex, type);
        }
    }
}