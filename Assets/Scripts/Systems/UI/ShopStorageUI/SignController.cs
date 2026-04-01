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
                // 1. 밤에는 클릭(터치) 방지를 위해 버튼 비활성화
                this.gameObject.GetComponent<Button>().interactable = false;

                // 2. 밤이 되었을 때 자동으로 간판을 돌려서 닫는 연출 실행
                SignFlipper flipper = this.gameObject.GetComponent<SignFlipper>();
                if (flipper != null)
                {
                    flipper.AutoClose();
                }
                break;

            default: // Morning, Day, Evening (아침, 낮, 저녁)
                // 3. 밤이 아니면 다시 간판 버튼 활성화
                this.gameObject.GetComponent<Button>().interactable = true;
                break;
        }
    }

}
