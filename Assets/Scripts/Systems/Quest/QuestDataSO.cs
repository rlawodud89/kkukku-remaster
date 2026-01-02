using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest/Quest Data")]
public class QuestDataSO : ScriptableObject
{
    [Header("기본 정보")]
    public int id;              // 퀘스트 ID (고유 번호)
    public string questTitle;   // 퀘스트 제목
    [TextArea] 
    public string description;  // 퀘스트 설명

    [Header("목표")]
    public List<QuestGoalData> goals; // 목표 리스트 (여러 개일 수 있음)

    [Header("보상")]
    public int rewardGold;      // 보상 골드
    public int rewardExp;       // 보상 경험치
    // public ItemSO rewardItem; // 나중에 아이템 SO가 생기면 추가
}
