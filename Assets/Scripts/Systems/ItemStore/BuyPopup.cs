using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class BuyPopup : MonoBehaviour
{
    public TMP_Text InfoText;

    void Start()
    {
        
    }

    public void OnClickYesBtn()
    {

    }

    public void OnClickNoBtn()
    {
        gameObject.SetActive(false);
    }
}
