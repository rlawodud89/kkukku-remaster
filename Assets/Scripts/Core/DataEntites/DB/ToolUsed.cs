using SQLite4Unity3d;

public class ToolUsed
{
    [PrimaryKey] public ToolType toolType { get; set; }
    public string toolName { get; set; }
}
