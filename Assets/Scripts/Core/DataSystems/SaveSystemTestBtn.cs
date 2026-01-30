using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class SaveSystemTestBtn : MonoBehaviour
{
    [SerializeField] private TMP_Text TMPtext;

    private void Update()
    {
        TMPtext.text = "시간: " + ServiceLocator.Get<SaveService>().GetCurrentTimer();
    }

    public void OnClickTestBtn()
    {
        var userData = ServiceLocator.Get<GameData>().User.GetUserData();
        Debug.Log("사용자 이름: " + userData.shopName);
        Debug.Log("래벨: " + userData.level);
        Debug.Log("경험치: " + userData.energy);
    }
}
