using UnityEngine;

[CreateAssetMenu(fileName = "Quest", menuName = "QuestSO/QuestSO")]
public class QuestSO : ScriptableObject
{
    public string questName;
    public QuestType questType;
    public string description;
    public int level;
    public int completeState;
    public int rewardGold;
    public int rewardMoonrock;
    public float rewardEnergy;
}
