using UnityEngine;
using TMPro;

public class WR_StorageSelectButton : MonoBehaviour
{
    private int myBoxID;           // DB상의 고유 ID (예: 105)
    private RoomInteriorType myType; // 내가 이불함인지 재료함인지

    public TextMeshProUGUI titleText; // "1번 보관함" 텍스트

    // 1번 스크립트가 정보를 넣어주는 곳
    public void Setup(int id, int index, RoomInteriorType type)
    {
        myBoxID = id;
        myType = type;
        titleText.text = $"{index}번 보관함";
    }

    // 버튼 클릭 시 (Inspector의 Button OnClick에 연결)
    public void OnClick()
    {
        Debug.Log($"[{myType}] {myBoxID}번 함을 선택했습니다.");

        // ★ 여기서 '타입'에 따라 서로 다른 내용물 창을 엽니다!
        switch (myType)
        {
            case RoomInteriorType.BLANKET_BOX:
                // "이불 내용물 보여주는 UI야, 105번 열어라"
                //BlanketUIManager.Instance.OpenPanel(myBoxID);
                break;

            case RoomInteriorType.MATERIAL_BOX:
                // "재료 내용물 보여주는 UI야, 105번 열어라"
                //MaterialUIManager.Instance.OpenPanel(myBoxID);
                break;
        }
    }
}