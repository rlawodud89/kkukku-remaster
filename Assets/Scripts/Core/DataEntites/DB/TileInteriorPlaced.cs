using SQLite4Unity3d;

public class TileInteriorPlaced
{
    [PrimaryKey] public TilePositionType tilePosition { get; set; }
    public string itemName { get; set; }
}
