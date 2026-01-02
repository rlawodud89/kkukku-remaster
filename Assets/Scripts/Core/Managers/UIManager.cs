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

    
    private Dictionary<string, GameObject> _popupList = new Dictionary<string, GameObject>();

    public void OpenPopup(GameObject popupPrefab)
    {
        if (popupPrefab == null) return;

        string popupName=popupPrefab.name;

        // 이미 생성되어있으면 켜주기만 함
        if(_popupList.ContainsKey(popupName) && _popupList[popupName] != null)
        {
            GameObject existingPopup = _popupList[popupName];
            existingPopup.SetActive(true);
            existingPopup.transform.SetAsLastSibling(); // 맨앞으로
        }
        // 없으면 생성
        else
        {
            GameObject newPopup=Instantiate(popupPrefab,transform);
            newPopup.name=popupName;
            _popupList[popupName]=newPopup;
        }
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
        CloseAllPopups(); 
    }

    public void CloseAllPopups()
    {
        // 딕셔너리에 기록된 모든 창을 검사
        foreach (var popup in _popupList.Values)
        {
            // 팝업이 실제로 존재한다면
            if (popup != null)
            {
                Destroy(popup); 
            }
        }

        // 딕셔너리를 깨끗하게 비움
        _popupList.Clear();
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
