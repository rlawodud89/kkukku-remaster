using SQLite4Unity3d;

public class TileInteriorInventory
{
    [PrimaryKey] public string itemName { get; set; }
    public TileInteriorType tileType {get; set;}
}
