using SQLite4Unity3d;

public class ToolInventory
{
    [PrimaryKey] public string toolName { get; set; }
    public ToolType toolType { get; set; }
}
