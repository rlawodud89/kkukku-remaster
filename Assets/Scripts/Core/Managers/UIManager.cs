using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class UIManager : MonoBehaviour
{
    private static UIManager _instance;
    
    public static UIManager Instance
    {
        get
        {
            // 씬에 생성된 싱글톤이 없으면 자동 생성
            if (_instance == null)
            {
                var obj = new GameObject("UIManager");
                _instance = obj.AddComponent<UIManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as UIManager;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }


    // 메뉴 팝업 여는 함수
    public void OpenPopup(GameObject popupPrefab)
    {
        if (popupPrefab == null) return;
        popupPrefab.SetActive(true);
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    // 씬이 로드될 때 실행되는 함수
    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        // 특정 씬에서 캔버스 숨기기 
        /*
        if (_canvasComponent == null) _canvasComponent = GetComponent<Canvas>();
        if (scene.name==) _canvasComponent.enabled = false;
        else _canvasComponent.enabled = true;*/

        // 씬 이동할 때 열려있는 팝업 다 닫기
        // CloseAllPopups(); 
    }


    [Header("확인 팝업 프리팹")]
    public GameObject confirmPopupPrefab;

    public void ShowConfirmPopup(string msg, System.Action onYes)
    {
        // 팝업 생성
        GameObject go = Instantiate(confirmPopupPrefab, transform);
        
        // 팝업 세팅 (글자랑 할 일 넘겨주기)
        ConfirmPopup popup = go.GetComponent<ConfirmPopup>();
        popup.Setup(msg, onYes);
    }
}
