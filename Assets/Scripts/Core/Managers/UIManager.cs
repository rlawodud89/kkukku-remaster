using System.Collections;
using System.Collections.Generic;
using UnityEngine;

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
}
