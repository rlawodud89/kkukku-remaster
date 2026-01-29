using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class QuestSlotUI : MonoBehaviour
{
    [Header("UI Components")]
    public UnityEngine.UI.Image questIcon;   // 퀘스트 아이콘
    public  TMP_Text questTitle;   // 퀘스트 제목
    public TMP_Text progressText;   // 진행도

    //public UnityEngine.UI.Image reward1Image;   // 퀘스트 보상
    //public GameObject reward2;

    public TMP_Text completeButton;  // 보상받기/진행중 버튼


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

        // 보상이미지, 갯수 설정


        // 퀘스트 완료 여부 표시
        if (quest.isCompleted)
        {
            completeButton.text="보상받기";
        }
        else
        {
            completeButton.text="진행중";
        }
    }
}
