using System.Collections.Generic;
using UnityEngine;

public interface IStoreItemProvider
{
    public bool isCountable { get; }
    public bool isGold { get; }

    public List<(string itemName, Sprite itemSprite, int price)> LoadItemData();
    public bool AddItem(string itemName, int count);
    public string GetDescription(string itemName);
}