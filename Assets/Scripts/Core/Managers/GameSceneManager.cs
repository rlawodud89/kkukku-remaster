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
        if (PlayerPrefs.HasKey("StoreName"))
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
        }
    }
}
