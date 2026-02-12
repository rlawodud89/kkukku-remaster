using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.UI;

public class LetterManager : MonoBehaviour
{

    public List<LetterDataSO> allLetterDatas; // 모든 편지 원본 데이터 (SO)
    List<int> myLetterIDs= new List<int>();
    public Transform contentPanel;           // 편지 슬롯이 생성될 부모 패널
    public GameObject letterButtonPrefab;

    public GameObject letterContentPanel; // 편지지 패널
    private static LetterManager _instance;
    
    public static LetterManager Instance
    {
        get
        {
            // 씬에 생성된 싱글톤이 없으면 자동 생성
            if (_instance == null)
            {
                var obj = new GameObject("GameManager");
                _instance = obj.AddComponent<LetterManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as LetterManager;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        allLetterDatas=Resources.LoadAll<LetterDataSO>("ScriptableObjects/Letter").ToList();


        
    }

    private void Start()
    {
        // 테스트용
        //GiveLetter(2);
        LoadLettersFromDB();
    }

    

    // 편지 부여
    public void GiveLetter(int letterID)
    {
        // 이미 리스트에 있다면 추가하지 않음
        if (myLetterIDs.Contains(letterID)) return;
        myLetterIDs.Add(letterID);

        List<int> list = new List<int> { letterID };
        ServiceLocator.Get<GameData>().Quest.AddLetters(list);

        UpdateUI();
    }

    public void UpdateUI()
    {
        if (contentPanel == null) return;

        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        //myLetterIDs = ServiceLocator.Get<GameData>().Quest.GetCurrentLetters();

        foreach (int id in myLetterIDs)
        {
            // 원본 데이터 찾기
            LetterDataSO data = allLetterDatas.FirstOrDefault(x => x.letterID == id);

            if (data != null)
            {
                // UI 슬롯 생성
                GameObject slot = Instantiate(letterButtonPrefab, contentPanel);
                // 슬롯 스크립트에 데이터 전달 (Setup 함수 필요)
                LetterSlotUI letterSlotUIScript=slot.GetComponent<LetterSlotUI>();
                letterSlotUIScript.Setup(data);

                Button btn = slot.GetComponent<Button>();
                btn.onClick.AddListener(() => OpenLetter(data));
            }
        }
    }

    public void LoadLettersFromDB()
    {
        // 기존 UI와 데이터 리스트 초기화
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        myLetterIDs.Clear();

        // DB에서 현재 보유 중인 편지 ID 리스트 가져오기
        myLetterIDs = ServiceLocator.Get<GameData>().Quest.GetCurrentLetters();

        if (myLetterIDs == null || myLetterIDs.Count == 0)
        {
            Debug.Log("보유 중인 편지가 없습니다.");
            return;
        }

        UpdateUI();
        Debug.Log($"{myLetterIDs.Count}개의 편지를 성공적으로 불러왔습니다.");
    }

    public void OpenLetter(LetterDataSO letter)
    {
        letterContentPanel.SetActive(true);

        letterContentPanel.GetComponent<LetterContentUI>().Setup(letter);
    }
}
