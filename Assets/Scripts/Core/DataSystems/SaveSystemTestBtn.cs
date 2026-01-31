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
        ServiceLocator.Get<GameData>().Quest.AddQuest(1);
        ServiceLocator.Get<GameData>().Quest.SaveQuest(1, 1, true, true);
    }
}
