using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpgradePopup : MonoBehaviour
{
    [Header("UI 요소")]
    public TMP_Text InfoText;
    public WarningUI warningUI;

    private IUpgradeProvider upgradeProvider;

    public void SetUpgrade(IUpgradeProvider upgradeProvider)
    {
        this.upgradeProvider = upgradeProvider;

        string moneyText = upgradeProvider.isGold ? "재화" : "월석";

        InfoText.text = $"{upgradeProvider.levelName}을 {moneyText} {upgradeProvider.GetUpgradePrice()}개로" +
            $"\n업그레이드하시겠습니까?";
    }

    public void OnClickYesBtn()
    {
        if (upgradeProvider.isGold)
        {
            if (upgradeProvider.GetUpgradePrice() > ServiceLocator.Get<GameData>().User.GetCurrentGold())
            {
                warningUI.Show("재화가 부족합니다.");
                gameObject.SetActive(false);
                return;
            }

            GameManager.Instance.ChangeGold(-upgradeProvider.GetUpgradePrice());
        }
        else
        {
            if (upgradeProvider.GetUpgradePrice() > ServiceLocator.Get<GameData>().User.GetCurrentMoonrock())
            {
                warningUI.Show("월석이 부족합니다.");
                gameObject.SetActive(false);
                return;
            }

            GameManager.Instance.ChangeMoonRock(-upgradeProvider.GetUpgradePrice());
        }

        upgradeProvider.LevelUpgrade();
        gameObject.SetActive(false);
    }

    public void OnClickNoBtn()
    {
        gameObject.SetActive(false);
    }
}
