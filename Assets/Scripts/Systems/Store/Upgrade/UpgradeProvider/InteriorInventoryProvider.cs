using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class InteriorInventoryProvider : IUpgradeProvider
{
    public string levelName { get; private set; } = "인테리어 인벤토리 레벨";
    public int currentLevel { get; private set; }
    public int maxLevel { get; private set; } = 3;
    public bool isGold { get; private set; } = true;

    private Dictionary<int, int> upgradePrice = new();

    public InteriorInventoryProvider()
    {
        upgradePrice.Add(1, 8000);
        upgradePrice.Add(2, 10000);

        currentLevel = ServiceLocator.Get<GameData>().User.GetInteriorInventoryLevel().level;
    }

    public int GetUpgradePrice()
    {
        if (upgradePrice.ContainsKey(currentLevel)) return upgradePrice[currentLevel];
        return -1;
    }

    public void LevelUpgrade()
    {
        ServiceLocator.Get<GameData>().User.ChangeInteriorInventoryLevel(1);
        currentLevel++;
    }
}
