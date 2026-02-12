using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ProfileManager : MonoBehaviour
{
    public TMP_Text nameText;

    void Start()
    {
        SetName();
    }

    public void SetName()
    {
        string savedName=PlayerPrefs.GetString("StoreName","꾸꾸");
        nameText.text=$"{savedName}의 이불가게";
    }
}
