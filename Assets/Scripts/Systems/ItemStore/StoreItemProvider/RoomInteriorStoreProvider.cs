using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomInteriorStoreProvider : IStoreItemProvider
{
    public bool isCountable { get; private set; } = true;
    public bool isGold { get; private set; } = true;

    public List<(string itemName, Sprite itemSprite, int price)> LoadData()
    {
        List<(string itemName, Sprite itemSprite, int price)> data = new();



        return data;
    }
}
