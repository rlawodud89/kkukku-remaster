using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class EndingBtn : MonoBehaviour
{
    public void OnClickEndingBtn()
    {
        UIEventManager.HideMainUI();
        LoadingSceneManager.LoadScene("Ending");
    }

}
