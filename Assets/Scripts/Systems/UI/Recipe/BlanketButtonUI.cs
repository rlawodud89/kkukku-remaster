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
        lockImage.SetActive(false);
        this.GetComponent<Button>().interactable=true;
    }
}
