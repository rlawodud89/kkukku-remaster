using SQLite4Unity3d;

public class QuestBox
{
    [PrimaryKey] public string questName { get; set; }
    public int progress { get; set; }
    public bool isComplete { get; set; }
}
