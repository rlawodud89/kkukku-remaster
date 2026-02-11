using SQLite4Unity3d;

public class StoreItemList
{
    [PrimaryKey] public string itemName { get; set; }
    public StoreType storeType { get; set; }
}
