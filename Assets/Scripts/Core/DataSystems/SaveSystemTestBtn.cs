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
        List<string> list = new List<string>();
        list.Add("기본이불");
        ServiceLocator.Get<GameData>().BlanketCraft.AddBlanketRecipes(list);

        foreach (var i in ServiceLocator.Get<GameData>().BlanketCraft.GetCurrentRecipes())
        {
            Debug.Log(i.itemName);
        }
    }
}
