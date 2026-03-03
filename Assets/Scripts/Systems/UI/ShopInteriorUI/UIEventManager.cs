using System;
using UnityEngine;

public static class UIEventManager
{
    // bool 값을 전달하는 이벤트 (false면 숨겨라, true면 보여라)
    public static event Action<bool> OnMainUIStateChanged;

    // 현재 열려있는 팝업 창의 개수
    private static int openWindowCount = 0;

    // "메인 UI 숨겨!" 방송 쏘기
    public static void HideMainUI()
    {
        OnMainUIStateChanged?.Invoke(false);
    }

    // "메인 UI 보여!" 방송 쏘기
    public static void ShowMainUI()
    {
        OnMainUIStateChanged?.Invoke(true);
    }
}