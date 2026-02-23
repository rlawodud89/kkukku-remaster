using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MoveToWorkRoom : MonoBehaviour
{
    public void MoveScene()
    {
        LoadingSceneManager.LoadScene("WorkRoom");
    }
}
