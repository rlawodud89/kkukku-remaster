using SQLite4Unity3d;

public class ShopInteriorPlaced
{
    [PrimaryKey] public int gridNumber { get; set; }
    public string itemName { get; set; }
    public ShopInteriorType interiorType { get; set; }
    public int ID { get; set; }
}
