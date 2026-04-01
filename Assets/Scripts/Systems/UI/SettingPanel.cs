using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class SettingPanel : MonoBehaviour
{
    public void OnClickPrologButton()
    {
        LoadingSceneManager.LoadScene("Prolog");
    }

    public void OnClickSaveButton()
    {
        ServiceLocator.Get<SaveService>().SaveNow();
    }

    public void OnExitButton()
    {
        Application.Quit();
    }
}
