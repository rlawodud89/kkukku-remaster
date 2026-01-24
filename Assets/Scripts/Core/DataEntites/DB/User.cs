using SQLite4Unity3d;

public class User
{
    [PrimaryKey] public string shopName { get; set; }
    public int level { get; set; }
    public float energy { get; set; }
    public int gold { get; set; }
    public int moonrock { get; set; }
    public float playTime { get; set; }
    public string endScene { get; set; }
    public bool isOpen { get; set; }
    public int itemShopLevel { get; set; }
    public int interiorInventoryLevel { get; set; }
    public int shopLevel { get; set; }
    public float bgSound { get; set; }
    public float effectSound { get; set; }
}
