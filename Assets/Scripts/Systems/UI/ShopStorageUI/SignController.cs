using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SignController : MonoBehaviour
{
    public Button signButton;
    public SignFlipper signFlipper;
    private void OnEnable() {
        // GameManager의 이벤트에 내 함수를 등록
        GameManager.OnPhaseChangedEvent += SignControll;
    }

    private void OnDisable() {
        // 메모리 누수 방지를 위해 해제
        GameManager.OnPhaseChangedEvent -= SignControll;
    }

    void SignControll(DayPhase phase)
    {
        switch (phase)
        {
            case DayPhase.Night:
                // 1. 밤에는 간판 버튼 비활성화 (시각적 비활성화용)
                if (signButton != null) signButton.interactable = false;

                // 2. 가게가 열려있다면 강제로 닫기
                if (ShopManager.Instance.isStoreOpen && signFlipper != null)
                {
                    signFlipper.ForceClose();
                }
                break;

            default: // Morning, Day, Evening (아침, 낮, 저녁)
                // 3. 아침이 되면 다시 간판 버튼 활성화
                if (signButton != null) signButton.interactable = true;
                break;
        }
    }

}
