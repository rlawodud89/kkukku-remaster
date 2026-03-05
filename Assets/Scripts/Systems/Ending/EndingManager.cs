using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Playables;
using UnityEngine.SceneManagement;

public class EndingManager : MonoBehaviour
{
    [SerializeField] PlayableDirector director;

    void Start()
    {
        director.Play();
    }

    public void OnEndingFinished()
    {
        SceneManager.LoadScene("MistForest");
    }
}
