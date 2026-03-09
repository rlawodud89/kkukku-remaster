using UnityEngine;

public class CanvasManager : MonoBehaviour
{
    [Header("연결할 캔버스들")]
    public GameObject canvasToShow; // 켜고 싶은 캔버스
    public GameObject canvasToHide; // 끄고 싶은 캔버스
    public bool MainUI;

    // 버튼의 OnClick 이벤트에 연결할 함수입니다.
    // 외부에서 버튼이 실행할 수 있도록 반드시 'public'을 붙여야 해요.
    public void SwitchCanvas()
    {
        if (MainUI)
        {
            UIEventManager.ShowMainUI();
        }
        else
        {
            UIEventManager.HideMainUI();
        }

        // 켤 캔버스가 지정되어 있다면 켭니다 (true)
        if (canvasToShow != null)
        {
            canvasToShow.SetActive(true);
        }

        // 끌 캔버스가 지정되어 있다면 끕니다 (false)
        if (canvasToHide != null)
        {
            canvasToHide.SetActive(false);
        }
    }
}
