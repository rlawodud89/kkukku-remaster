using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameData
{
    public UserAggregate User { get; private set; }
    public InventoryAggregate Inventory { get; private set; }
    public InteriorAggregate Interior { get; private set; }
    public QuestAggregate Quest { get; private set; }
    public ShopStateAggregate ShopState { get; private set; }
    public BlanketCraftAggregate BlanketCraft { get; private set; }


    public GameData(UserAggregate User, InventoryAggregate Inventory, InteriorAggregate Interior,
        QuestAggregate Quest, ShopStateAggregate ShopState, BlanketCraftAggregate BlanketCraft)
    {
        this.User = User;
        this.Inventory = Inventory;
        this.Interior = Interior;
        this.Quest = Quest;
        this.ShopState = ShopState;
        this.BlanketCraft = BlanketCraft;
    }
}
