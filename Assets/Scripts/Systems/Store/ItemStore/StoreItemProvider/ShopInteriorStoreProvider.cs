using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopInteriorStoreProvider : IStoreItemProvider
{
    public bool isCountable { get; private set; } = true;
    public bool isGold { get; private set; } = true;

    public List<(string itemName, Sprite itemSprite, int price)> LoadItemData()
    {
        List<(string itemName, Sprite itemSprite, int price)> data = new();

        // TODO: DB에서 오늘 판매하는 아이템 리스트 받아오기

        return data;
    }

    public bool AddItem(string itemName, int count)
    {
        // 인테리어 재고 확인 후 추가
        // 남는 재고함이 없는 경우, false return

        return ServiceLocator.Get<GameData>().Inventory.AddShopInteriorItem(itemName, count);
    }

    public string GetDescription(string itemName)
    {
        ShopInteriorItemSO item = ServiceLocator.Get<GameData>().Inventory.GetShopInteriorItemSO(itemName);

        switch (item.shopInteriorType)
        {
            case ShopInteriorType.TABLE:
                return $"최대 저장량: {item.slotCount}";
            case ShopInteriorType.CASHER:
                return "손님이 이불을 사는 계산대";
            case ShopInteriorType.INTERIOR:
                return "단순 인테리어 아이템";
            default:
                return "";
        }
    }
}
