using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ShopExpansionProvider : IUpgradeProvider
{
    public string levelName { get; private set; } = "가게 확장 레벨";
    public int currentLevel { get; private set; }
    public int maxLevel { get; private set; } = 3;
    public bool isGold { get; private set; } = true;

    private Dictionary<int, int> upgradePrice = new();

    public ShopExpansionProvider()
    {
        upgradePrice.Add(1, 8000);
        upgradePrice.Add(2, 10000);

        currentLevel = ServiceLocator.Get<GameData>().User.GetShopLevel().level;
    }

    public int GetUpgradePrice()
    {
        if (upgradePrice.ContainsKey(currentLevel)) return upgradePrice[currentLevel];
        return -1;
    }

    public void LevelUpgrade()
    {
        ServiceLocator.Get<GameData>().User.ChangeShopLevel(1);
        currentLevel++;

        UpgradeEvents.OnUpgradeLevelChanged?.Invoke(this);
    }
}
