using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TileInteriorStoreProvider : IStoreItemProvider
{
    public bool isCountable { get; private set; } = false;
    public bool isGold { get; private set; } = true;

    public List<(string itemName, Sprite itemSprite, int price)> LoadItemData()
    {
        List<(string itemName, Sprite itemSprite, int price)> data = new();

        // TODO: DB에서 오늘 판매하는 아이템 리스트 받아오기

        return data;
    }

    public bool BuyItem(string itemName, int price, int count)
    {
        // TODO: 이미 존재하는 경우 return false

        return true;
    }
}
