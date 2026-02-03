using SQLite4Unity3d;

public class WorkerState
{
    [PrimaryKey] public int workerID { get; set; }
    public int stamina { get; set; } = 0;
    public string workingItem { get; set; } = null;
    public float progress { get; set; } = 0;
    public float skill { get; set; } = 0;
}
