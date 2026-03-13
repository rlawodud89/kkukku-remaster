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
        
        //GenerateDailyQuests(0);
        LoadQuestsFromDB();

        var targetQuest = myActiveQuests.Find(x => x.data.questID == 1);
        
    }

    void OnEnable()
    {
        GameManager.OnPhaseChangedEvent += HandlePhaseChanged;
    }

    void OnDisable()
    {
        GameManager.OnPhaseChangedEvent -= HandlePhaseChanged;
    }

    // 아침 되면 퀘스트 리셋
    private void HandlePhaseChanged(DayPhase phase)
    {
        if (phase == DayPhase.Morning)
        {
            GenerateDailyQuests(0);
            //LoadQuestsFromDB();
        }
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

            // DB에 저장
            bool isAdded = ServiceLocator.Get<GameData>().Quest.AddQuest(candidates[randIdx].questID);

            Debug.Log($"오늘의 일일 퀘스트 {i+1}: {candidates[randIdx].questID}");

            // 중복 방지
            candidates.RemoveAt(randIdx);
        }

        UpdateUI();
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

    public void LoadQuestsFromDB()
    {
        // 기존 리스트 초기화
        myActiveQuests.Clear();

        List<QuestBox> questDataList = ServiceLocator.Get<GameData>().Quest.GetCurrentQuests();

        if (questDataList == null || questDataList.Count == 0)
        {
            Debug.Log("저장된 퀘스트 데이터가 없습니다.");
            return;
        }

        foreach (QuestBox data in questDataList)
        {
            // 전체 원본 데이터(allQuestDatas)에서 ID가 일치하는 SO 찾기
            QuestDataSO originalSO = allQuestDatas.FirstOrDefault(x => x.questID == data.questID);

            if (originalSO != null)
            {
                // 찾은 SO를 바탕으로 새로운 Quest 인스턴스 생성
                Quest loadedQuest = new Quest(originalSO);

                // DB에서 받아온 현재 진행 상황 덮어씌우기
                loadedQuest.currentCount = data.progress;
                loadedQuest.isCompleted = data.isComplete;
                loadedQuest.isRewarded = data.isReward;

                // 리스트에 추가
                myActiveQuests.Add(loadedQuest);
            }
        }

        UpdateUI();
        Debug.Log($"{myActiveQuests.Count}개의 퀘스트를 DB에서 성공적으로 불러왔습니다.");
    }

    // 퀘스트 진행도 올리는 함수
    public void UpdateQuestProgressByID(int targetQuestID, int amount = 1)
    {
        // 진행 중인 퀘스트 리스트에서 ID가 일치하는 퀘스트를 찾음
        Quest targetQuest = myActiveQuests.Find(q => q.data.questID == targetQuestID);

        if (targetQuest != null)
        {
            // 이미 완료된 퀘스트라면 무시
            if (targetQuest.isCompleted)
            {
                Debug.Log($"[QuestManager] {targetQuestID}는 이미 완료된 퀘스트입니다.");
                return;
            }

            // 진행도 증가 및 완료 체크 
            targetQuest.AddProgress(amount);

            // 4. DB에 저장 (ID 기반으로 업데이트)
            ServiceLocator.Get<GameData>().Quest.SaveQuest(targetQuestID, targetQuest.currentCount, targetQuest.isCompleted, targetQuest.isRewarded);

            // UI 갱신
            UpdateUI();
            
            Debug.Log($"<color=green>[QuestManager]</color> {targetQuestID} 업데이트 완료! 현재: {targetQuest.currentCount}");
        }
        else
        {
            Debug.LogWarning($"[QuestManager] ID가 {targetQuestID}인 진행 중인 퀘스트를 찾을 수 없습니다.");
        }
    }

    // SO 데이터를 직접 넣어서 찾는 방식
    public void UpdateQuestProgress(QuestDataSO targetSO)
    {
        UpdateQuestProgressByID(targetSO.questID, 1);
    }

    // 퀘스트 보상 수령 함수
    public void CompleteQuest(int targetQuestID)
    {
        var targetQuest = myActiveQuests.Find(x => x.data.questID == targetQuestID);
        if (targetQuest != null)
        {
            if(targetQuest.isCompleted && !targetQuest.isRewarded)
            {
                targetQuest.ReceiveReward();

                // DB 저장
                ServiceLocator.Get<GameData>().Quest.SaveQuest(targetQuestID, targetQuest.currentCount, targetQuest.isCompleted, targetQuest.isRewarded);

                Debug.Log($"<color=yellow>[QuestManager]</color> {targetQuestID} 보상 처리 완료.");
            }
            else
            {
                Debug.LogWarning($"[QuestManager] {targetQuestID}는 아직 완료되지 않았거나 이미 보상을 받았습니다.");
            }
        }
        else
        {
            Debug.LogError($"[QuestManager] ID가 {targetQuestID}인 퀘스트를 찾을 수 없습니다.");
        }
    }
}
