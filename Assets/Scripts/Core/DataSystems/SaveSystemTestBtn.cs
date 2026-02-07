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
        var allRecipes = ServiceLocator.Get<GameData>().BlanketCraft.GetAllRecipes();
    }
}
