using UnityEngine;
using TMPro;

public class WR_StorageSelectButton : MonoBehaviour
{
    private int myBoxID;           // 보관함의 고유 ID (예: 105)
    private RoomInteriorType myType; // 이불함인지 재료함인지 구분

    public TextMeshProUGUI titleText; // "1번 보관함" 텍스트

    // 초기화 함수
    public void Setup(int id, int index, RoomInteriorType type)
    {
        myBoxID = id;
        myType = type;
        titleText.text = $"{index}번 보관함";
    }

    // 버튼 클릭 시 (Inspector 연결)
    public void OnClick()
    {
        // 방어 코드: 매니저가 없으면 중단
        if (StorageUIController.Instance == null)
        {
            Debug.LogError("[WR_StorageSelectButton] StorageUIController 인스턴스가 없습니다!");
            return;
        }

        Debug.Log($"[{myType}] {myBoxID}번 함을 선택했습니다.");

        // ★ 핵심 변경 사항: 복잡한 Load 함수 대신 OpenPopup 하나만 호출합니다.
        // 내 타입(RoomInteriorType)에 맞춰서 UI 타입(StorageType)을 결정합니다.

        switch (myType)
        {
            case RoomInteriorType.BLANKET_BOX:
                StorageUIController.Instance.OpenPopup(myBoxID, StorageUIController.StorageType.Blanket);
                break;

            case RoomInteriorType.MATERIAL_BOX:
                StorageUIController.Instance.OpenPopup(myBoxID, StorageUIController.StorageType.CraftBox);
                break;
                
            // 필요하다면 Snack 등 다른 케이스 추가
        }
    }
}