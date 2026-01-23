using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestAggregate : IAggregate
{
    private Dictionary<string, QuestBox> questBox = new();
    private Dictionary<string, SpecialQuestBox> specialQuestBox = new();
    private List<LetterBox> letterBox = new();


    private HashSet<string> insertedQuest = new();
    private HashSet<string> updatedQuest = new();
    private HashSet<string> deletedQuest = new();

    private HashSet<string> insertedSpeicalQuest = new();
    private HashSet<string> updatedSpecialQuest = new();
    private HashSet<string> deletedSpecialQuest = new();

    private HashSet<string> insertedLetter = new();


    public bool IsDirty { get; private set; }

    private void MarkDirty()
    {
        IsDirty = true;
        ServiceLocator.Get<DirtyDataRegistry>().RegisterDirty(this);
    }

    public void ClearDirty() => IsDirty = false;

    public IEnumerable<SavePayload> ToSavePayloads()
    {
        if (!IsDirty)
            yield break;

        // 퀘스트 변경 사항
        foreach (var iq in insertedQuest)
        {
            QuestBox quest = questBox[iq];

            yield return new SavePayload
            {
                Operation = SaveOperation.INSERT,
                Table = "QuestBox",
                Values = new Dictionary<string, object>
                {
                    { "questName", quest.questName},
                    { "progress", quest.progress },
                    { "isComplete", quest.isComplete }
                }
            };
        }
        foreach (var up in updatedQuest)
        {
            QuestBox quest = questBox[up];

            yield return new SavePayload
            {
                Operation = SaveOperation.UPDATE,
                Table = "QuestBox",
                Values = new Dictionary<string, object>
                {
                    { "progress", quest.progress },
                    { "isComplete", quest.isComplete }
                },
                Conditions = new Dictionary<string, object>
                {
                    { "questName", quest.questName }
                }
            };
        }
        foreach (var dq in deletedQuest)
        {
            QuestBox quest = questBox[dq];

            yield return new SavePayload
            {
                Operation = SaveOperation.DELETE,
                Table = "QuestBox",
                Conditions = new Dictionary<string, object>
                {
                    { "questName", quest.questName }
                }
            };
        }

        // 특별 퀘스트 변경 사항
        foreach (var iq in insertedSpeicalQuest)
        {
            SpecialQuestBox specialQuest = specialQuestBox[iq];

            yield return new SavePayload
            {
                Operation = SaveOperation.INSERT,
                Table = "SpecialQuestBox",
                Values = new Dictionary<string, object>
                {
                    { "questName", specialQuest.questName },
                    { "isComplete", specialQuest.isComplete },
                    { "failCount", specialQuest.failCount }
                }
            };
        }
        foreach (var up in updatedSpecialQuest)
        {
            SpecialQuestBox specialQuest = specialQuestBox[up];

            yield return new SavePayload
            {
                Operation = SaveOperation.UPDATE,
                Table = "SpecialQuestBox",
                Values = new Dictionary<string, object>
                {
                    { "isComplete", specialQuest.isComplete },
                    { "failCount", specialQuest.failCount }
                },
                Conditions = new Dictionary<string, object>
                {
                    { "questName", specialQuest.questName }
                }
            };
        }
        foreach (var dq in deletedSpecialQuest)
        {
            SpecialQuestBox specialQuest = specialQuestBox[dq];

            yield return new SavePayload
            {
                Operation = SaveOperation.DELETE,
                Table = "SpecialQuestBox",
                Conditions = new Dictionary<string, object>
                {
                    { "questName", specialQuest.questName }
                }
            };
        }

        // 편지 변경 사항
        foreach (var il in insertedLetter)
        {
            yield return new SavePayload
            {
                Operation = SaveOperation.INSERT,
                Table = "LetterBox",
                Values = new Dictionary<string, object>
                {
                    { "letterName", il }
                }
            };
        }

        insertedQuest.Clear();
        updatedQuest.Clear();
        deletedQuest.Clear();
        insertedSpeicalQuest.Clear();
        updatedSpecialQuest.Clear();
        deletedSpecialQuest.Clear();
        insertedLetter.Clear();
    }

    public void LoadQuestAggregate(IEnumerable<QuestBox> questBox, IEnumerable<SpecialQuestBox> specialQuestBox, IEnumerable<LetterBox> letterBox)
    {
        this.questBox = questBox.ToDictionary(qb => qb.questName);
        this.specialQuestBox = specialQuestBox.ToDictionary(sqb => sqb.questName);
        this.letterBox = letterBox.ToList();
    }


    // === 게임 플레이 메서드 ===
}
