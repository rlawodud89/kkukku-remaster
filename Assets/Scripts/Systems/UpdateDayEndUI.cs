using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class UpdateDayEndUI : MonoBehaviour
{
    public TMP_Text goldCountText;
    public TMP_Text moonnrockCountCountText;


    // Start is called before the first frame update
    void Start()
    {

    }

    // Update is called once per frame
    void Update()
    {

    }

    public void Setup(int goldCount, int moonnrockCount)
    {
        goldCountText.text = goldCount.ToString();
        moonnrockCountCountText.text = moonnrockCount.ToString();
    }

    // 확인 버튼 눌렀을 때
    public void ClickOKButton()
    {
        this.gameObject.SetActive(false);

        GameManager.Instance.SetGameTime(6, 0);
        ServiceLocator.Get<GameData>().User.ResetTodayGoldMoonrock();
        ServiceLocator.Get<GameData>().Store.ResetAllStoreItemList();
    }
}
