using UnityEngine;

[CreateAssetMenu(fileName = "SpecialQuest", menuName = "QuestSO/SpecialQuestSO")]
public class SpecialQuestSO : ScriptableObject
{
    public string questName;
    public int npcID;
    public string description;
    public DecoMaterialType needType;
    public string hint;
    public string successComment;
    public int rewardGold;
    public int rewardMoonrock;
    public float rewardEnergy;
    public string rewardLetterName;
}
