using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "New Letter", menuName = "Letter/Letter Data")]
public class LetterDataSO : ScriptableObject
{
    [Header("Basic Info")]
    //public string letterID;        // 고유 ID 
    public string senderName;      // 보낸 주민 이름
    public string title;           // 편지 제목
    [TextArea(5, 10)]
    public string content;         // 편지 본문

    [Header("Visuals")]
    public Sprite senderProfile;   // 주민 얼굴 아이콘
    //public Sprite letterPaperBG;   // 편지지 배경 이미지
}
