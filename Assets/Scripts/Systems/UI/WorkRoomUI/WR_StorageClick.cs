using UnityEngine;
using UnityEngine.EventSystems; // UI 클릭 방지용

public class WR_StorageClick : MonoBehaviour
{
    [Header("Data")]
    public int myStorageID; // InteriorManager가 생성 시 주입해줌

    public RoomInteriorType myType;
    [Header("Settings")]
    // 이 가구가 이불장인지 간식창고인지 프리팹 단계에서 설정
    [SerializeField] private StoragePopupUI.StorageType myStorageType;
    [SerializeField] private StoragePopupUI storagePopup;

private void OnMouseDown()
    {
        // 1. 클릭 감지 시작
        Debug.Log($"[Click Test] {gameObject.name} 클릭됨!");

        // 2. UI 위 클릭인지 체크
        //if (EventSystem.current.IsPointerOverGameObject()) 
        //{
        //    Debug.LogWarning("[Click Test] UI가 앞에 있어서 클릭이 무시되었습니다.");
        //    return;
        //}

        // 3. 편집 모드 체크
        if (InteriorManager.Instance != null && InteriorManager.Instance.IsEditMode) 
        {
            Debug.Log("[Click Test] 현재 편집 모드이므로 팝업을 열지 않습니다.");
            return;
        }

        // 4. UI 연결 상태 확인
        if (storagePopup != null)
        {
            Debug.Log($"[Click Test] {myStorageType} 팝업 오픈 요청 보냄 (ID: {myStorageID})");
            storagePopup.OpenPopup(myStorageID, myStorageType);
        }
        else
        {
            // 이 로그가 뜬다면 인스펙터에서 popupUI 칸이 비어있는 겁니다.
            Debug.LogError($"[Click Test] {gameObject.name}에 popupUI가 연결되지 않았습니다! 인스펙터를 확인하세요.");
        }
    }
}