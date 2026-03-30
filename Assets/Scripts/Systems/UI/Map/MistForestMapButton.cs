using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MistForestMapButton : MonoBehaviour
{
    [SerializeField] private int endingLevel = 10;
    [SerializeField] private Button mapButton;

    // 이동할 씬 이름
    public string endingSceneName;
    public string mistForestName;
    // 장소 이름
    public string locationName;

    private Color originalButtonColor;
    void Awake()
    {
        originalButtonColor = mapButton.image.color;

        ColorBlock colors = mapButton.colors;
        colors.disabledColor = originalButtonColor * 0.7f;
        mapButton.colors = colors;
    }

    void OnEnable()
    {
        UpdateButtonState();
    }

    // 버튼 클릭 시
    public void OnClickSpot()
    {
        UIManager.Instance.ShowConfirmPopup(
            $"{locationName}으로 이동하시겠습니까?", // 메시지
            () => { GoToEnding(); }                // '네' 누르면 할 일
        );
    }


    private void GoToEnding()
    {
        if (ServiceLocator.Get<GameData>().User.GetUserData().level < endingLevel)
        {
            Debug.Log("엔딩 레벨 부족");
        }
        else if (!ServiceLocator.Get<GameData>().User.GetIsWatchEnding())
        {
            UIEventManager.HideMainUI();
            LoadingSceneManager.LoadScene(endingSceneName);
        }
        else
        {
            LoadingSceneManager.LoadScene(mistForestName);
        }
    }

    private void UpdateButtonState()
    {
        bool isLocked = ServiceLocator.Get<GameData>().User.GetUserData().level < endingLevel;

        mapButton.interactable = !isLocked;
    }

}