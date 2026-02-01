using SQLite4Unity3d;

public class RoomInteriorPlaced
{
    [PrimaryKey] public int gridNumber { get; set; }
    public string itemName { get; set; }
    public RoomInteriorType interiorType { get; set; }
    public int ID { get; set; }
}
