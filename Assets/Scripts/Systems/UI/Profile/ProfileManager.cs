using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ProfileManager : MonoBehaviour
{
    public TMP_Text nameText;

    public UnityEngine.UI.Image energyBar;
    public float maxEnergy = 100f;
    private float currentEnergy = 0f;


    void Start()
    {
        SetName();
    }

    // 이불가게 이름 설정
    public void SetName()
    {
        string savedName = ServiceLocator.Get<GameData>().User.GetUserData().shopName;
        nameText.text = $"{savedName}의 이불가게";
    }

    // 포근에너지 설정
    public void AddEnergy(float amount)
    {
        currentEnergy += amount;

        energyBar.fillAmount = currentEnergy / maxEnergy;
    }
}
