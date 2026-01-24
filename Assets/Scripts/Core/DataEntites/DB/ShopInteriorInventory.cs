using SQLite4Unity3d;

public class ShopInteriorInventory
{
    [PrimaryKey] public string itemName { get; set; }
    public ShopInteriorType shopInteriorType { get; set; }
    public int count { get; set; }
}
