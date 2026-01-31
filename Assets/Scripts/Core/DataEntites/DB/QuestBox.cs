using SQLite4Unity3d;

public class QuestBox
{
    [PrimaryKey] public int questID { get; set; }
    public int progress { get; set; }
    public bool isComplete { get; set; }
    public bool isReward { get; set; }
}
