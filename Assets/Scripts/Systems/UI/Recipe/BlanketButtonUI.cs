using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class BlanketButtonUI : MonoBehaviour
{
    //public string recipeID;

    public TMP_Text blanketName;
    public GameObject lockImage;

    public void Setup()
    {
        Debug.Log($"{blanketName.text} 버튼의 Setup() 실행됨!");
        lockImage.SetActive(false);
        this.GetComponent<Button>().interactable=true;
    }
}
