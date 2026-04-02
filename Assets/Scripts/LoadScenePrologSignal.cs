using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
        if (ServiceLocator.Get<GameData>().User.GetStartState() == StartStateType.PROLOG)
            ServiceLocator.Get<GameData>().User.SetStartState(StartStateType.TUTORIAL);

        GameSceneManager.Instance.AfterProlog();
    }
}
