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

    [Header("Quest Type Icons")]
    public Sprite collectBlanketIcon;
    public Sprite fishingIcon;
    public Sprite gatheringIcon;
    public Sprite visitLocationIcon;
    public Sprite talkIcon;

   

    [Header("Reward UI")]
    public UnityEngine.UI.Image reward1Icon;
    public TMP_Text reward1Amount;
    public UnityEngine.UI.Image reward2Icon;
    public TMP_Text reward2Amount;

    [Header("Reward Sprites")]
    public Sprite goldSprite;
    public Sprite moonRockSprite;
    public Sprite energySprite;

    public TMP_Text completeButton;  // 보상받기/진행중 버튼

    private Quest targetQuest; // 현재 슬롯이 표시하고 있는 퀘스트 참조


    // 데이터를 받아서 화면을 갱신하는 함수
    public void Setup(Quest quest)
    {
        targetQuest = quest; // 퀘스트 참조 저장

        // 텍스트 설정
        questTitle.text=quest.data.title;
        
        // 아이콘 설정
        questIcon.sprite=GetQuestTypeIcon(quest.data.type);

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
        UpdateRewardUI(reward1Icon, reward1Amount, quest.data.reward1);
        UpdateRewardUI(reward2Icon, reward2Amount, quest.data.reward2);
        

        // 퀘스트 완료 여부 표시
        if (quest.isCompleted)
        {
            completeButton.text="보상받기";
        }
        else
        {
            completeButton.text="진행중";
        }

        RefreshUI();
    }

    private void UpdateRewardUI(UnityEngine.UI.Image iconImage, TMP_Text amountText, Reward reward)
    {
        amountText.text = reward.amount.ToString();
        iconImage.sprite = GetRewardSprite(reward.type);
    }

    // RewardType에 따라 적절한 스프라이트 반환
    private Sprite GetRewardSprite(RewardType type)
    {
        switch (type)
        {
            case RewardType.Gold: return goldSprite;
            case RewardType.MoonRock: return moonRockSprite;
            case RewardType.Energy: return energySprite;
            default: return null;
        }
    }

    private Sprite GetQuestTypeIcon(QuestType type)
    {
        switch (type)
        {
            case QuestType.CollectBlanket: return collectBlanketIcon;
            case QuestType.Fishing: return fishingIcon;
            case QuestType.Gathering: return gatheringIcon;
            case QuestType.TalkToNPC: return talkIcon;
            case QuestType.VisitLocation:   return visitLocationIcon;
            default: return null;
        }
    }

    // 보상 버튼 클릭헸을 때 실행될 함수
    public void OnRewardButtonClick()
    {
        if (targetQuest != null && targetQuest.isCompleted && !targetQuest.isRewarded)
        {
            targetQuest.ReceiveReward(); // 데이터 갱신
            RefreshUI(); // UI 다시 그리기
        }
    }

    // 상태에 따라 버튼 텍스트와 상호작용 여부를 업데이트
    private void RefreshUI()
    {
        if (targetQuest.isRewarded)
        {
            completeButton.GetComponentInParent<Button>().interactable = false;
        }
        else if (targetQuest.isCompleted)
        {
            completeButton.text = "보상받기";
        }
        else
        {
            completeButton.text = "진행중";
        }
    }
}
