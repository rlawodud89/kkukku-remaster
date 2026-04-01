using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.SceneManagement;

public class StoreName : MonoBehaviour
{
    public TMP_InputField nameInputField;
    public ProfileManager profileManager;

    public void SaveAndStart()
    {
        // 입력창에 적힌 텍스트 가져옴
        string inputName = nameInputField.text;

        if (!string.IsNullOrEmpty(inputName))
        {
            // PlayerPrefs에 "StoreName"이라는 이름으로 입력값을 저장
            //PlayerPrefs.SetString("StoreName", inputName);
            //PlayerPrefs.Save();

            //Debug.Log("저장된 가게 이름: " + PlayerPrefs.GetString("StoreName"));

            ServiceLocator.Get<GameData>().User.SetShopName(inputName);

            this.gameObject.SetActive(false);

            if (TutorialLoader.Instance != null) TutorialLoader.Instance.TutorialStart();

            ServiceLocator.Get<SaveService>().SaveNow();

            profileManager.SetName();
        }
        else
        {
            Debug.Log("이름을 입력해 주세요!");
        }
    }
}
