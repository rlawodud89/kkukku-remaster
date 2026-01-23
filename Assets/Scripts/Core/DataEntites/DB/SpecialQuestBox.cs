using SQLite4Unity3d;

public class SpecialQuestBox
{
    [PrimaryKey] public string questName { get; set; }
    public bool isComplete { get; set; }
    public int failCount { get; set; }
}
