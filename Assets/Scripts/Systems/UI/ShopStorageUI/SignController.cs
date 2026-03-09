using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class SignController : MonoBehaviour
{

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
            case DayPhase.Morning:
                this.gameObject.GetComponent<Button>().interactable=true;
                break;
            case DayPhase.Night:
                this.gameObject.GetComponent<Button>().interactable=true;
                break;
            default:
                this.gameObject.GetComponent<Button>().interactable=false;
                break;
        }
    }
    
}
