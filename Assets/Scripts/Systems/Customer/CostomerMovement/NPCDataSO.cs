using UnityEngine;

[CreateAssetMenu(fileName = "NPCData", menuName = "Tycoon/NPC Data")]
public class NPCDataSO : ScriptableObject
{
    public string npcID;        // 고유 ID (예: "Bat_Writer")
    public string[] smallTalks; // 이 캐릭터 전용 대사들

    [Header("Quest Settings")]
    public int questProgress;         // 이 NPC와 연결된 퀘스트 번호
}
