using SQLite4Unity3d;

public class RoomInteriorInventory
{
    [PrimaryKey] public string itemName { get; set; }
    public RoomInteriorType roomInteriorType { get; set; }
    public int count { get; set; } = 0;
}
