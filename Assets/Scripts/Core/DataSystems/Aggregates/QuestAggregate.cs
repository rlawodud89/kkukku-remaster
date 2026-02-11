using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class QuestAggregate : IAggregate
{
    // === 런타임 데이터 ===

    private Dictionary<int, QuestBox> questBox;
    private Dictionary<string, SpecialQuestBox> specialQuestBox;
    private List<LetterBox> letterBox;

    // === SO 데이터 ===

    private Dictionary<string, SpecialQuestSO> specialQuestSOs;
    private Dictionary<string, NPCDataSO> customerSOs;

    // === 변경 사항 저장소 ===

    private Dictionary<int, SaveOperation> questChanges = new();
    private Dictionary<string, SaveOperation> specialQuestChanges = new();
    private HashSet<int> insertedLetter = new();


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
        foreach (var (questID, change) in questChanges)
        {
            switch (change)
            {
                case SaveOperation.INSERT:
                    QuestBox insertQuest = questBox[questID];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.INSERT,
                        Table = "QuestBox",
                        Values = new Dictionary<string, object>
                        {
                            { "questID", insertQuest.questID},
                            { "progress", insertQuest.progress },
                            { "isComplete", insertQuest.isComplete },
                            { "isReward", insertQuest.isReward }
                        }
                    };

                    break;

                case SaveOperation.UPDATE:
                    QuestBox updateQuest = questBox[questID];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.UPDATE,
                        Table = "QuestBox",
                        Values = new Dictionary<string, object>
                        {
                            { "progress", updateQuest.progress },
                            { "isComplete", updateQuest.isComplete },
                            { "isReward", updateQuest.isReward }
                        },
                        Conditions = new Dictionary<string, object>
                        {
                            { "questID" , updateQuest.questID }
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
                            { "questID", questID }
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
        foreach (var letterID in insertedLetter)
        {
            yield return new SavePayload
            {
                Operation = SaveOperation.INSERT,
                Table = "LetterBox",
                Values = new Dictionary<string, object>
                {
                    { "letterID", letterID }
                }
            };
        }

    }

    public void LoadQuestAggregate(IEnumerable<QuestBox> questBox, IEnumerable<SpecialQuestBox> specialQuestBox,
        IEnumerable<LetterBox> letterBox, Dictionary<string, SpecialQuestSO> specialQuestSOs, Dictionary<string, NPCDataSO> customerSOs)
    {
        this.questBox = questBox.ToDictionary(qb => qb.questID);
        this.specialQuestBox = specialQuestBox.ToDictionary(sqb => sqb.questName);
        this.letterBox = letterBox.ToList();

        this.specialQuestSOs = specialQuestSOs;
        this.customerSOs = customerSOs;
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

    public List<int> GetCurrentLetters()
    {
        List<int> letterList = new List<int>();
        foreach (LetterBox letter in letterBox)
        {
            letterList.Add(letter.letterID);
        }

        return letterList;
    }

    public void AddLetters(List<int> letterIDlist)
    {
        foreach (int letterID in letterIDlist)
        {
            LetterBox letter = new LetterBox();
            letter.letterID = letterID;
            letterBox.Add(letter);

            insertedLetter.Add(letterID);
        }

        MarkDirty();
    }

    public List<QuestBox> GetCurrentQuests()
    {
        return questBox.Values.ToList();
    }

    public bool AddQuest(int questID)
    {
        if (questBox.ContainsKey(questID)) return false;

        questBox.Add(questID, new QuestBox
        {
            questID = questID,
            progress = 0,
            isComplete = false,
            isReward = false
        });

        MergeChange(questChanges,
            questID,
            SaveOperation.INSERT);

        MarkDirty();

        return true;
    }

    public bool SaveQuest(int questID, int progress, bool isComplete, bool isReward)
    {
        if (!questBox.ContainsKey(questID)) return false;

        questBox[questID].progress = progress;
        questBox[questID].isComplete = isComplete;
        questBox[questID].isReward = isReward;

        MergeChange(questChanges,
            questID,
            SaveOperation.UPDATE);

        MarkDirty();

        return true;
    }

    public bool RemoveQuest(int questID)
    {
        if (!questBox.ContainsKey(questID)) return false;

        questBox.Remove(questID);

        MergeChange(questChanges,
            questID,
            SaveOperation.DELETE);

        MarkDirty();

        return true;
    }

    public SpecialQuestSO GetSpeicalQuestSO(string questName)
    {
        if (specialQuestSOs.TryGetValue(questName, out var specialQuestSO)) return specialQuestSO;
        else return null;
    }

    public NPCDataSO GetSpeicalQuestNPCData(string questName)
    {
        if (specialQuestSOs.TryGetValue(questName, out var specialQuestSO))
            return customerSOs[specialQuestSO.npcID];

        else return null;
    }

    public int GetNPCSpecialQuestState(string npcID)
    {
        foreach (var (questName, specialQuest) in specialQuestBox)
        {
            if (specialQuestSOs[questName].npcID == npcID)
            {
                if (specialQuest.isComplete) return 2;
                else return 1;
            }
        }

        return 0;
    }

    public List<SpecialQuestBox> GetCurrentSpecialQuests()
    {
        return specialQuestBox.Values.ToList();
    }

    public bool AddSpeicalQuest(string questName)
    {
        if (specialQuestBox.ContainsKey(questName)) return false;

        specialQuestBox.Add(questName, new SpecialQuestBox
        {
            questName = questName,
            isComplete = false,
            failCount = 0
        });

        MergeChange(specialQuestChanges,
            questName,
            SaveOperation.INSERT);

        MarkDirty();

        return true;
    }

    public bool SaveSpecialQuest(string questName, bool isComplete, int failCount)
    {
        if (!specialQuestBox.ContainsKey(questName)) return false;

        specialQuestBox[questName].isComplete = isComplete;
        specialQuestBox[questName].failCount = failCount;

        MergeChange(specialQuestChanges,
            questName,
            SaveOperation.UPDATE);

        MarkDirty();

        return true;
    }

    public bool RemoveSpecialQuest(string questName)
    {
        if (!specialQuestBox.ContainsKey(questName)) return false;

        specialQuestBox.Remove(questName);

        MergeChange(specialQuestChanges,
            questName,
            SaveOperation.DELETE);

        MarkDirty();

        return true;
    }


}
