using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpgradeLoader : MonoBehaviour
{
    [SerializeField] private UpgradeType upgradeType;
    [SerializeField] private TMP_Text levelText;
    [SerializeField] private UpgradePopup popup;
    [SerializeField] private WarningUI warningUI;

    private IUpgradeProvider upgradeProvider;

    void Start()
    {
        upgradeProvider = UpgradeProviderFactory.Create(upgradeType);

        LevelTextUpdate();
    }

    public void OnUpgradeClick()
    {
        if (upgradeProvider.maxLevel == upgradeProvider.currentLevel)
        {
            warningUI.Show("더이상 업그레이드할 수 없습니다.");
            return;
        }

        popup.SetUpgrade(upgradeProvider);
        popup.gameObject.SetActive(true);
    }

    private void LevelTextUpdate()
    {
        if (upgradeProvider.maxLevel == upgradeProvider.currentLevel)
        {
            levelText.text = "max level";
        }
        else
        {
            levelText.text = $"lv {upgradeProvider.currentLevel} -> lv {upgradeProvider.currentLevel + 1}";
        }
    }
}
