using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using System.Linq;

public class QuestManager : MonoBehaviour
{
    private static QuestManager _instance;

    public List<QuestDataSO> allQuestDatas;  // 모든 퀘스트 SO
    public List<Quest> myActiveQuests;   // 플레이어가 진행 중인 퀘스트들

    public GameObject questButtonPrefab;
    public Transform contentPanel;  // questButton 생성될 위치
    
    public static QuestManager Instance
    {
        get
        {
            // 씬에 생성된 싱글톤이 없으면 자동 생성
            if (_instance == null)
            {
                var obj = new GameObject("QuestManager");
                _instance = obj.AddComponent<QuestManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as QuestManager;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }

        // Quest 폴더 안에 있는 모든 QuestDataSO 타입의 파일 불러오기
        // Resources 밑으로 정확한 경로적어야 함
        allQuestDatas=Resources.LoadAll<QuestDataSO>("ScriptableObjects/Quest").ToList();
        Debug.Log($"총 {allQuestDatas.Count}개의 퀘스트를 불러왔습니다.");

        

        // 테스트용
        GenerateDailyQuests(0);
        UpdateUI();
    }

    // 플레이어에게 퀘스트 부여하는 함수 (아침마다 호출)
    public void GenerateDailyQuests(int playerLevel)
    {
        // 어제 퀘스트 초기화
        myActiveQuests.Clear();

        // 레벨 필터링 (내 레벨 ~ 내 레벨 -2)
        var candidates = allQuestDatas
            .Where(x => x.requiredLevel <= playerLevel && x.requiredLevel >= playerLevel - 2)
            .ToList();

        // 퀘스트가 너무 적으면 전체에서 뽑기 (에러 방지용)
        if (candidates.Count < 3) candidates = allQuestDatas.ToList();

        // 랜덤으로 3개 뽑기
        for (int i = 0; i < 3; i++)
        {
            if(candidates.Count==0) break;

            int randIdx = Random.Range(0, candidates.Count);

            // 원본 퀘스트 데이터를 참고하여 새 Quest 객체를 하나 만들어 넣기
            myActiveQuests.Add(new Quest(candidates[randIdx]));

            Debug.Log($"오늘의 일일 퀘스트 {i+1}: {candidates[randIdx].questID}");

            // 중복 방지
            candidates.RemoveAt(randIdx);
        }
    }

    // UI 갱신하는 함수
    public void UpdateUI()
    {
        if(contentPanel==null)  return;

        // 기존에 있던 것들 다 지우기 
        foreach (Transform child in contentPanel)
        {
            Destroy(child.gameObject);
        }

        // 새로운 퀘스트들 생성
        foreach(Quest quest in myActiveQuests)
        {
            // 프리팹 생성
            GameObject newButton = Instantiate(questButtonPrefab,contentPanel);

            // 스크립트 가져와서 Setup() 함수 호출
            QuestSlotUI questSlotUIScricpt =newButton.GetComponent<QuestSlotUI>();
            questSlotUIScricpt.Setup(quest);
        }
    }
}
