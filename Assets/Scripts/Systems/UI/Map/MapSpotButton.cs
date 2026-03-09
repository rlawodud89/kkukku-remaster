using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MapSpotButton : MonoBehaviour
{
    // 이동할 씬 이름
    public string sceneName;
    // 장소 이름
    public string locationName;

    // 버튼 클릭 시
    public void OnClickSpot()
    {
        UIManager.Instance.ShowConfirmPopup(
            $"{locationName}으로 이동하시겠습니까?", // 메시지
            () => { GoToScene(); }                // '네' 누르면 할 일
        );
    }

    // 씬으로 이동하는 함수 
    void GoToScene()
    {
        // 씬 이동
        SceneManager.LoadScene(sceneName);
    }
}
