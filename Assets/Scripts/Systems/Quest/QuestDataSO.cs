using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public struct Reward
{
    public RewardType type; // 보상 종류 (Enum)
    public int amount;      // 보상 양
}

public enum RewardType
{
    Gold,
    MoonRock,
    Energy
}


// 퀘스트 종류 (수집, 대화, 방문 등)
public enum QuestType
{
    CollectBlanket, // 아이템 수집 
    Fishing,   // 낚시
    Gathering,   // 채집
    VisitLocation,   // 장소 방문 
    TalkToNPC     // NPC 대화
}


[CreateAssetMenu(fileName = "New Quest", menuName = "Quest/Quest Data")]
public class QuestDataSO : ScriptableObject
{
    [Header("기본 정보")]
    public int questID;             // 퀘스트 고유 번호 
    public string title;            // 퀘스트 제목
    //[TextArea] public string description; // 설명 
    //public Sprite icon;             // UI에 띄울 아이콘

    [Header("난이도 설정")]
    public int requiredLevel;       // 이 퀘스트가 등장할 최소 레벨
    
    [Header("목표 설정")]
    public QuestType type;          // 퀘스트 유형
    public int goalCount;           // 목표 수치

    [Header("보상")]
    public Reward reward1;
    public Reward reward2;
}
