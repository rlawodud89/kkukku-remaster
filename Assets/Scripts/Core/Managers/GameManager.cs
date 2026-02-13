using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class GameManager : MonoBehaviour
{
    private static GameManager _instance;
    
    public static GameManager Instance
    {
        get
        {
            // 씬에 생성된 싱글톤이 없으면 자동 생성
            if (_instance == null)
            {
                var obj = new GameObject("GameManager");
                _instance = obj.AddComponent<GameManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as GameManager;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }
    }

    private int gold;
    private int moonRock;  
    private int level;
    private float energy;
    private int maxEnergy=200;

    public TMP_Text goldText;
    public TMP_Text moonRockText;
    public TMP_Text levelText;
    public UnityEngine.UI.Image energyBar;


    void Start()
    {
        LoadGameData();
    }

    void LoadGameData()
    {
        gold=ServiceLocator.Get<GameData>().User.GetCurrentGold();
        moonRock=ServiceLocator.Get<GameData>().User.GetCurrentMoonrock();

        var userData = ServiceLocator.Get<GameData>().User.GetUserData();
        level=userData.level;
        energy=userData.energy;
        UpdateUI();
    }

    void UpdateUI()
    {
        goldText.text=gold.ToString();
        moonRockText.text=moonRock.ToString();

        levelText.text=$"Lv {level.ToString()}";
        energyBar.fillAmount = energy / maxEnergy;
        Debug.Log($"포근에너지: {energy}");
    }

    public void ChangeGold(int amount)
    {
        gold+=amount;
        ServiceLocator.Get<GameData>().User.ChangeGold(amount);
        UpdateUI();
    }

    public void ChangeMoonRock(int amount)
    {
        moonRock+=amount;
        ServiceLocator.Get<GameData>().User.ChangeMoonrock(amount);
        UpdateUI();
    }

    public void ChangeEnergy(int amount)
    {
        energy+=amount;
        ServiceLocator.Get<GameData>().User.SetUserData("", level, energy);
        UpdateUI();

        if (maxEnergy == energy)
        {
            ChangeLevel();
        }
    }

    public void ChangeLevel()
    {
        level+=1;
        ServiceLocator.Get<GameData>().User.SetUserData("", level, energy);
        UpdateUI();
    }

}
