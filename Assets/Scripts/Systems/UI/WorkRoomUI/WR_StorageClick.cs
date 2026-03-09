using UnityEngine;
using UnityEngine.EventSystems; // UI 클릭 방지용

public class WR_StorageClick : MonoBehaviour, IPointerClickHandler
{
    [Header("Data")]
    public int myStorageID; // InteriorManager가 생성 시 주입해줌

    public RoomInteriorType myType;
    [Header("Settings")]
    // 이 가구가 이불장인지 간식창고인지 프리팹 단계에서 설정
    [SerializeField] private StorageUIController.StorageType myStorageType;
    [SerializeField] private StorageUIController storageUIController;


    private void Start()
    {
        // 만약 인스펙터 연결이 안 되어 있다면, 게임 시작 시 자동으로 찾는다.
        if (storageUIController == null)
        {
            // 씬에 있는 StorageUIController 컴포넌트를 가진 녀석을 찾아 내 변수에 넣음
            storageUIController = FindObjectOfType<StorageUIController>();
        }
    }
    
    
    public void OnPointerClick(PointerEventData eventData)
    {
        if (StorageUIController.Instance.IsPopupOpen) return;
        // 3. 편집 모드 체크
        if (InteriorManager.Instance != null && InteriorManager.Instance.IsEditMode) 
        {
            Debug.Log("[Click Test] 현재 편집 모드이므로 팝업을 열지 않습니다.");
            return;
        }

        if (storageUIController == null)
        {
            // 씬에 있는 StorageUIController 컴포넌트를 가진 녀석을 찾아 내 변수에 넣음
            storageUIController = FindObjectOfType<StorageUIController>();
        }
        // 4. UI 연결 상태 확인
        if (storageUIController != null)
        {
            Debug.Log($"[Click Test] {myStorageType} 팝업 오픈 요청 보냄 (ID: {myStorageID})");
            storageUIController.OpenPopup(myStorageID, myStorageType);
        }
        else
        {
            // 이 로그가 뜬다면 인스펙터에서 popupUI 칸이 비어있는 겁니다.
            Debug.LogError($"[Click Test] {gameObject.name}에 popupUI가 연결되지 않았습니다! 인스펙터를 확인하세요.");
        }
    }

    public void OnclickExitBtn()
    {
        if (storageUIController != null)
        {
            storageUIController.CloseAllPanels();
        }
    }
}