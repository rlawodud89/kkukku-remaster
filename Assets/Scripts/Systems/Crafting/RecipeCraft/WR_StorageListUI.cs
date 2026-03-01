using UnityEngine;
using UnityEngine.UI;
using System.Collections;
using System.Collections.Generic;

public class WR_StorageListUI : MonoBehaviour
{
    [Header("연결 필수")]
    public Transform contentParent; 
    public GameObject boxButtonPrefab; 

    [Header("설정")]
    public RoomInteriorType targetType; 

    private void OnEnable()
    {
        // 타이밍 이슈 방지를 위해 코루틴 사용
        StartCoroutine(RefreshRoutine());
    }

    IEnumerator RefreshRoutine()
    {
        // 1. 혹시 모를 매니저 초기화 대기 (한 프레임 쉼)
        yield return null;

        RefreshStorageList();
    }

    public void RefreshStorageList()
    {
        // 1. 매니저 찾기
        RoomInteriorManager manager = FindObjectOfType<RoomInteriorManager>();
        if (manager == null)
        {
            Debug.LogError("[StorageList] InteriorManager를 찾을 수 없습니다!");
            return;
        }

        if (manager.currentPlacedList == null || manager.currentPlacedList.Count == 0)
        {
            Debug.LogWarning($"[StorageList] 매니저의 가구 리스트가 비어있습니다. (Count: 0)");
            // 리스트가 비어있어도 기존 버튼은 지워야 하므로 아래 진행
        }

        // 2. 기존 버튼 삭제
        foreach (Transform child in contentParent)
        {
            Destroy(child.gameObject);
        }

        // 3. 버튼 생성
        int count = 0;
        foreach (var furniture in manager.currentPlacedList)
        {
            // 디버그: 타입 비교 로그
            // Debug.Log($"검사 중: {furniture.interiorType} vs 목표: {targetType}");

            if (furniture.interiorType == targetType)
            {
                CreateButton(furniture.ID, count + 1, targetType);
                count++;
            }
        }

        Debug.Log($"[StorageList] 총 {count}개의 버튼을 생성했습니다.");

        // 4. 레이아웃 갱신 (두 번 호출하여 확실하게 처리)
        StartCoroutine(ForceLayoutRebuild());
    }

    IEnumerator ForceLayoutRebuild()
    {
        // 프레임 끝까지 대기 (Destroy가 완료되도록)
        yield return new WaitForEndOfFrame();
        
        if (contentParent != null)
        {
            LayoutRebuilder.ForceRebuildLayoutImmediate(contentParent.GetComponent<RectTransform>());
        }
    }

    void CreateButton(int dbID, int displayIndex, RoomInteriorType type)
    {
        if (boxButtonPrefab == null) return;

        GameObject btn = Instantiate(boxButtonPrefab, contentParent);
        
        // UI 스케일/위치 초기화
        btn.transform.localScale = Vector3.one;
        btn.transform.localPosition = Vector3.zero;

        var script = btn.GetComponent<WR_StorageSelectButton>();
        if (script != null)
        {
            script.Setup(dbID, displayIndex, type);
        }
        else
        {
            Debug.LogError($"[StorageList] 프리팹에 'WR_StorageSelectButton' 스크립트가 없습니다!");
        }
    }
}