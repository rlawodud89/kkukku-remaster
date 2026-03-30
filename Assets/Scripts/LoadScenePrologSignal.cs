using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadSceneOnPrologSignal : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void LoadNextScene()
    {
        //SceneManager.LoadScene(nextSceneName);
        ServiceLocator.Get<GameData>().User.SetStartState(StartStateType.TUTORIAL);
        GameSceneManager.Instance.AfterProlog();
    }
}
