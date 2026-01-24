using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class QuestAggregate : IAggregate
{
    // === 런타임 데이터 ===

    private Dictionary<string, QuestBox> questBox = new();
    private Dictionary<string, SpecialQuestBox> specialQuestBox = new();
    private List<LetterBox> letterBox = new();

    // === 변경 사항 저장소 ===

    private Dictionary<string, SaveOperation> questChanges = new();
    private Dictionary<string, SaveOperation> specialQuestChanges = new();
    private HashSet<string> insertedLetter = new();


    // === 저장 시스템 사용 메서드 ===

    public bool IsDirty { get; private set; }

    private void MarkDirty()
    {
        IsDirty = true;
        ServiceLocator.Get<DirtyDataRegistry>().RegisterDirty(this);
    }

    public void ClearDirty()
    {
        IsDirty = false;

        questChanges.Clear();
        specialQuestChanges.Clear();
        insertedLetter.Clear();
    }

    public IEnumerable<SavePayload> ToSavePayloads()
    {
        if (!IsDirty)
            yield break;

        // 퀘스트 변경 사항
        foreach (var (questName, change) in questChanges)
        {
            switch (change)
            {
                case SaveOperation.INSERT:
                    QuestBox insertQuest = questBox[questName];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.INSERT,
                        Table = "QuestBox",
                        Values = new Dictionary<string, object>
                        {
                            { "questName", insertQuest.questName},
                            { "progress", insertQuest.progress },
                            { "isComplete", insertQuest.isComplete }
                        }
                    };

                    break;

                case SaveOperation.UPDATE:
                    QuestBox updateQuest = questBox[questName];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.UPDATE,
                        Table = "QuestBox",
                        Values = new Dictionary<string, object>
                        {
                            { "progress", updateQuest.progress },
                            { "isComplete", updateQuest.isComplete }
                        },
                        Conditions = new Dictionary<string, object>
                        {
                            { "questName", updateQuest.questName }
                        }
                    };

                    break;

                case SaveOperation.DELETE:
                    yield return new SavePayload
                    {
                        Operation = SaveOperation.DELETE,
                        Table = "QuestBox",
                        Conditions = new Dictionary<string, object>
                        {
                            { "questName", questName }
                        }
                    };

                    break;
            }
        }

        // 특별 퀘스트 변경 사항
        foreach (var (questName, change) in specialQuestChanges)
        {
            switch (change)
            {
                case SaveOperation.INSERT:
                    SpecialQuestBox insertSpecialQuest = specialQuestBox[questName];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.INSERT,
                        Table = "SpecialQuestBox",
                        Values = new Dictionary<string, object>
                        {
                            { "questName", insertSpecialQuest.questName },
                            { "isComplete", insertSpecialQuest.isComplete },
                            { "failCount", insertSpecialQuest.failCount }
                        }
                    };

                    break;

                case SaveOperation.UPDATE:
                    SpecialQuestBox updateSpecialQuest = specialQuestBox[questName];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.UPDATE,
                        Table = "SpecialQuestBox",
                        Values = new Dictionary<string, object>
                        {
                            { "isComplete", updateSpecialQuest.isComplete },
                            { "failCount", updateSpecialQuest.failCount }
                        },
                        Conditions = new Dictionary<string, object>
                        {
                            { "questName", updateSpecialQuest.questName }
                        }
                    };

                    break;

                case SaveOperation.DELETE:
                    yield return new SavePayload
                    {
                        Operation = SaveOperation.DELETE,
                        Table = "SpecialQuestBox",
                        Conditions = new Dictionary<string, object>
                        {
                            { "questName", questName }
                        }
                    };

                    break;
            }
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

    }

    public void LoadQuestAggregate(IEnumerable<QuestBox> questBox, IEnumerable<SpecialQuestBox> specialQuestBox, IEnumerable<LetterBox> letterBox)
    {
        this.questBox = questBox.ToDictionary(qb => qb.questName);
        this.specialQuestBox = specialQuestBox.ToDictionary(sqb => sqb.questName);
        this.letterBox = letterBox.ToList();
    }

    private void MergeChange<TKey>(Dictionary<TKey, SaveOperation> changes, TKey key, SaveOperation newOp)
    {
        if (!changes.TryGetValue(key, out var oldOp))
        {
            changes[key] = newOp;
            return;
        }

        switch (oldOp, newOp)
        {
            case (SaveOperation.INSERT, SaveOperation.UPDATE):
                // INSERT 유지
                break;

            case (SaveOperation.INSERT, SaveOperation.DELETE):
                // 생성했다가 삭제 → 아무 일도 없었던 것
                changes.Remove(key);
                break;

            case (SaveOperation.UPDATE, SaveOperation.UPDATE):
                // UPDATE 유지
                break;

            case (SaveOperation.UPDATE, SaveOperation.DELETE):
                changes[key] = SaveOperation.DELETE;
                break;

            case (SaveOperation.DELETE, SaveOperation.INSERT):
                // 삭제 후 다시 추가 → UPDATE로 취급
                changes[key] = SaveOperation.UPDATE;
                break;

            default:
                changes[key] = newOp;
                break;
        }
    }

    // === 게임 플레이 메서드 ===
}
