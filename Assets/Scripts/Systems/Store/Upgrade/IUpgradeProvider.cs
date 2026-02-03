using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IUpgradeProvider
{
    public string levelName { get; }
    public int currentLevel { get; }
    public int maxLevel { get; }
    public bool isGold { get; }

    public int GetUpgradePrice();
    public void LevelUpgrade();
}