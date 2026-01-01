using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using System;

public class ConfirmPopup : MonoBehaviour
{
    // 텍스트
    public TextMeshProUGUI messageText;

    // yes 선택 시 실행할 함수  
    private Action _onYesAction;

    // 팝업을 열어서 세틴
    public void Setup(string message, Action onYes)
    {
        messageText.text = message;
        _onYesAction = onYes; // 할 일을 기억해둠
    }

    // yes 버튼에 연결할 함수
    public void OnClickYes()
    {
        // 기억해둔 할 일이 있으면 실행한다.
        _onYesAction?.Invoke(); 
        
        Close(); // 팝업 닫기
    }

    // No 버튼에 연결할 함수
    public void OnClickNo()
    {
        Close(); // 그냥 닫기
    }

    private void Close()
    {
        Destroy(gameObject);
    }
}
