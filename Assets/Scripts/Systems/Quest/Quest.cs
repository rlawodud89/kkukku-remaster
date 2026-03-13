using System.Collections;
using System.Collections.Generic;
using UnityEngine;

[System.Serializable]
public class Quest 
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

    // 보상 수령 함수
    public void ReceiveReward()
    {
        // 완료되었고, 아직 보상을 받지 않은 경우에만 실행
        if (isCompleted && !isRewarded)
        {
            isRewarded = true;

            Reward[] rewards = { data.reward1, data.reward2 };

            foreach (var reward in rewards)
            {
                // 보상 양이 0보다 클 때만 지급
                if (reward.amount <= 0) continue;

                ApplyReward(reward.type, reward.amount);
            }

            Debug.Log($"<color=yellow>[Quest]</color> {data.title} 모든 보상 수령 완료!");
            
            // 실제 보상 지급 로직
        }
    }

    // 실제 보상을 종류별로 분류해서 지급하는 헬퍼 함수
    private void ApplyReward(RewardType type, int amount)
    {
        switch (type)
        {
            case RewardType.Gold:
                GameManager.Instance.ChangeGold(amount);
                Debug.Log($"금화 {amount} 지급");
                break;

            case RewardType.MoonRock:
                GameManager.Instance.ChangeMoonRock(amount);
                Debug.Log($"월석 {amount} 지급");
                break;

            case RewardType.Energy:
                GameManager.Instance.ChangeEnergy(amount);
                Debug.Log($"에너지 {amount} 지급");
                break;

            default:
                Debug.LogWarning($"{type}은(는) 정의되지 않은 보상 타입입니다.");
                break;
        }
    }
}
