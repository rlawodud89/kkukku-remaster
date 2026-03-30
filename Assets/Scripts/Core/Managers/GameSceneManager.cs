using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class GameSceneManager : MonoBehaviour
{
    private static GameSceneManager _instance;

    public static GameSceneManager Instance
    {
        get
        {
            // 씬에 생성된 싱글톤이 없으면 자동 생성
            if (_instance == null)
            {
                var obj = new GameObject("GameSceneManager");
                _instance = obj.AddComponent<GameSceneManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as GameSceneManager;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public GameObject storeNamePanel;

    public void OnStartButtonClick()
    {
        switch (ServiceLocator.Get<GameData>().User.GetStartState())
        {
            case StartStateType.PROLOG:
                ServiceLocator.Get<SaveService>().SetAutoSave(false);
                SceneManager.LoadScene("Prolog");
                break;

            case StartStateType.TUTORIAL:
                SceneManager.LoadScene("BlanketShop");
                ServiceLocator.Get<SaveService>().SetAutoSave(false);
                if (TutorialLoader.Instance != null) TutorialLoader.Instance.TutorialStart();
                break;

            case StartStateType.GAME:
                ServiceLocator.Get<SaveService>().SetAutoSave(true);
                SceneManager.LoadScene("BlanketShop");
                break;
        }

        //if (PlayerPrefs.HasKey("StoreName"))
        //{
        //    Debug.Log("기존 유저입니다. 게임 씬으로 이동합니다.");
        //    SceneManager.LoadScene("BlanketShop");
        //}
        //else
        //{
        //    // 데이터가 없다면 처음인 유저 -> 이름 설정 패널 띄우기
        //    Debug.Log("처음 온 유저입니다. 이름 설정창을 띄웁니다.");

        //    SceneManager.LoadScene("Prolog");
        //    //storeNamePanel.SetActive(true);
        //    //SceneManager.LoadScene("BlanketShop");
        //}
    }

    public void AfterProlog()
    {
        //if (PlayerPrefs.HasKey("StoreName"))
        if (ServiceLocator.Get<GameData>().User.GetStartState() == StartStateType.GAME)
        {
            Debug.Log("기존 유저입니다. 게임 씬으로 이동합니다.");
            SceneManager.LoadScene("BlanketShop");
        }
        else
        {
            // 데이터가 없다면 처음인 유저 -> 이름 설정 패널 띄우기
            Debug.Log("처음 온 유저입니다. 이름 설정창을 띄웁니다.");

            storeNamePanel.SetActive(true);
            SceneManager.LoadScene("BlanketShop");

            // 시간을 아침으로 설정
            GameManager.Instance.SetGameTime(6,0);
        }
    }
}
