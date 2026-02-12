using Microsoft.Unity.VisualStudio.Editor;
using System.Collections;
using System.Collections.Generic;
using TMPro;
using Unity.VisualScripting;
using UnityEngine;

public class LetterContentUI : MonoBehaviour
{
    public TMP_Text letterContentText;
    public UnityEngine.UI.Image letterImage;  

    public void Setup(LetterDataSO letter)
    {
        letterContentText.text=letter.content;
        letterImage.sprite=letter.senderProfile;
    }
}
