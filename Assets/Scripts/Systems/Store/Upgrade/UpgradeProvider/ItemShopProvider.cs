using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemShopProvider : IUpgradeProvider
{
    public string levelName { get; private set; } = "신성재료 레벨";
    public int currentLevel { get; private set; }
    public int maxLevel { get; private set; } = 3;
    public bool isGold { get; private set; } = false;

    private Dictionary<int, int> upgradePrice = new();

    public ItemShopProvider()
    {
        upgradePrice.Add(1, 2000);
        upgradePrice.Add(2, 5000);

        currentLevel = ServiceLocator.Get<GameData>().User.GetItemShopLevel();
    }

    public int GetUpgradePrice()
    {
        if (upgradePrice.ContainsKey(currentLevel)) return upgradePrice[currentLevel];
        return -1;
    }


    public void LevelUpgrade()
    {
        ServiceLocator.Get<GameData>().User.ChangeItemShopLevel(1);
        currentLevel++;

        UpgradeEvents.OnUpgradeLevelChanged?.Invoke(this);
    }
}
