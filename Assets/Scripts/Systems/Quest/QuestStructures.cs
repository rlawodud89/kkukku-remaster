using System;

// 퀘스트 종류 (수집, 대화, 방문 등)
public enum QuestType
{
    CollectItem, // 아이템 수집 (예: 원두 10개)
    VisitLocation,   // 장소 방문 (예: 공원 가기)
    Talk     // NPC 대화 (예: 단골손님 대화)
}


// 퀘스트 목표 데이터 
[Serializable]
public class QuestGoalData
{
    public QuestType type;      // 목표 타입
    public string targetName;   // 대상 이름 (예: "CoffeeBean", "Park")
    public int requiredAmount;  // 필요 개수 (예: 10)
}