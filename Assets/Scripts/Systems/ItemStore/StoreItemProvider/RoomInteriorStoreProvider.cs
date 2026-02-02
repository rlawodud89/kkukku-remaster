using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class RoomInteriorStoreProvider : IStoreItemProvider
{
    public bool isCountable { get; private set; } = true;
    public bool isGold { get; private set; } = true;

    public List<(string itemName, Sprite itemSprite, int price)> LoadItemData()
    {
        List<(string itemName, Sprite itemSprite, int price)> data = new();

        // TODO: DB에서 오늘 판매하는 아이템 리스트 받아오기

        return data;
    }

    public bool BuyItem(string itemName, int price, int count)
    {
        // TODO: 인테리어 재고 확인 후 추가
        // 남는 재고함이 없는 경우, false return

        return true;
    }
}
