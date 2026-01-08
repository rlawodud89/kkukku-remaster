using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using Microsoft.Unity.VisualStudio.Editor;
using UnityEditor.Experimental.GraphView;
using UnityEngine.UI;

public class QuestSlotUI : MonoBehaviour
{
    public  TMP_Text questTitle;   // 퀘스트 제목
    //public TMP_Text questDescription;   // 퀘스트 설명
    public UnityEngine.UI.Image questIcon;   // 퀘스트 아이콘
    public TMP_Text progressText;   // 진행도

    // 데이터를 받아서 화면을 갱신하는 함수
    public void Setup(Quest quest)
    {
        // 텍스트 설정
        questTitle.text=quest.data.title;
        
        // 아이콘 설정
        questIcon.sprite=quest.data.icon;

        // 진행도 설정
        if (quest.data.goalCount == 1)
        {
            progressText.text=" ";
        }
        else
        {
            progressText.text = $"{quest.currentCount} / {quest.data.goalCount}";
        }

        // 퀘스트 완료 시 색바꾸고 완료 표시
        if (quest.isCompleted)
        {
            questTitle.color=Color.green;
            progressText.text="완료!";
        }
        else
        {
            questTitle.color=Color.black;
        }
    }
}
