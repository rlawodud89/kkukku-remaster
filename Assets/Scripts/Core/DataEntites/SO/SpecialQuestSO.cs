using UnityEngine;

[CreateAssetMenu(fileName = "SpecialQuest", menuName = "QuestSO/SpecialQuestSO")]
public class SpecialQuestSO : ScriptableObject
{
    public string questName;
    public string customerName;
    public string description;
    public MaterialType needType;
    public string hint;
    public string successComment;
    public int rewardGold;
    public int rewardMoonrock;
    public float rewardEnergy;
    public string rewardLetterName;
}
