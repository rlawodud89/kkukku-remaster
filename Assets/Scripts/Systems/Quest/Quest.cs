using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class Quest : MonoBehaviour
{
    public QuestDataSO data;  // 원본 SO데이터
    public int currentCount;  // 현재 달성 수치
    public bool isCompleted;   // 완료 여부
    public bool isRewarded;    // 보상 수령 여부

    // 생성자 (데이터를 받아서 초기화)
    public Quest(QuestDataSO data)
    {
        this.data = data;
        this.currentCount = 0;
        this.isCompleted = false;
        this.isRewarded = false;
    }


    // 진행도 증가 함수
    public void AddProgress(int amount)
    {
        if(isCompleted) return;

        currentCount+=amount;
        if (currentCount >= data.goalCount)
        {
            currentCount=data.goalCount;
            isCompleted=true;

            // 나중에: 퀘스트 완료 알림 호출
        }
    }
}
