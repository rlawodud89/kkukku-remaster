using SQLite4Unity3d;

public class WorkerState
{
    [PrimaryKey]
    public int workerID { get; set; }

    public int stamina { get; set; }
    public string workingItem { get; set; }
    public float progress { get; set; }
    public float skill { get; set; }
}
