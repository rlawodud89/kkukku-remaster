using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Quest", menuName = "Quest/Quest Data")]
public class QuestDataSO : ScriptableObject
{
    [Header("기본 정보")]
    public int questID;             // 퀘스트 고유 번호 
    public string title;            // 퀘스트 제목
    [TextArea] public string description; // 설명 
    public Sprite icon;             // UI에 띄울 아이콘

    [Header("난이도 설정")]
    public int requiredLevel;       // 이 퀘스트가 등장할 최소 레벨
    
    [Header("목표 설정")]
    public QuestType type;          // 퀘스트 유형
    public int goalCount;           // 목표 수치

    [Header("보상")]
    public int rewardGold;          // 보상 골드
    public int rewardMoonRock;           // 보상 월석
    public int rewardEnergy;            // 보상 포근 에너지
}
