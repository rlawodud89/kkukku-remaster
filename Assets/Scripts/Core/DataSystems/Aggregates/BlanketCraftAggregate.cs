using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.Profiling;

public class BlanketCraftAggregate : IAggregate
{
    // === 런타임 데이터 ===

    private List<BlanketRecipe> blanketRecipe;
    private Queue<BlanketRecord> blanketRecord;

    // === SO 데이터 ===

    private Dictionary<string, MaterialItemSO> materialSOs;
    private Dictionary<string, BlanketItemSO> blanketSOs;

    // === 변경 사항 저장소 ===

    private HashSet<string> insertedBlanketRecipe = new();

    // === 기타 데이터 ===
    public const int maxRecordCount = 5;
    private bool isRecordDirty = false;

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

        insertedBlanketRecipe.Clear();
        isRecordDirty = false;
    }

    public IEnumerable<SavePayload> ToSavePayloads()
    {
        if (!IsDirty)
            yield break;

        // 보유 이불 레시피
        foreach (var itemName in insertedBlanketRecipe)
        {
            yield return new SavePayload
            {
                Operation = SaveOperation.INSERT,
                Table = "BlanketRecipe",
                Values = new Dictionary<string, object>()
                {
                    { "itemName", itemName }
                }
            };
        }

        // 이불 레시피 제작 기록
        if (isRecordDirty)
        {
            yield return new SavePayload
            {
                Operation = SaveOperation.DELETE,
                Table = "BlanketRecord"
            };

            foreach (var record in blanketRecord)
            {
                yield return new SavePayload
                {
                    Operation = SaveOperation.INSERT,
                    Table = "BlanketRecord",
                    Values = new Dictionary<string, object>
                    {
                        { "item1Name", record.item1Name },
                        { "item1Count", record.item1Count },
                        { "item2Name", record.item2Name },
                        { "item2Count", record.item2Count },
                        { "item3Name", record.item3Name },
                        { "item3Count", record.item3Count },
                        { "item4Name", record.item4Name },
                        { "item4Count", record.item4Count },
                        { "makedRecipeName", record.makedRecipeName },
                        { "createdAt", record.createdAt },
                    }
                };
            }

        }

    }

    public void LoadBlanketCraftAggregate(IEnumerable<BlanketRecipe> blanketRecipe, IEnumerable<BlanketRecord> blanketRecord,
        Dictionary<string, MaterialItemSO> materialSOs, Dictionary<string, BlanketItemSO> blanketSOs)
    {
        this.blanketRecipe = blanketRecipe.ToList();
        this.blanketRecord = new Queue<BlanketRecord>(blanketRecord.OrderBy(i => i.createdAt));

        this.materialSOs = materialSOs;
        this.blanketSOs = blanketSOs;
    }


    // === 게임 플레이 메서드 ===

    public void AddBlanketRecipes(IEnumerable<string> blanketNameList)
    {
        foreach (string recipeName in blanketNameList)
        {
            BlanketRecipe newRecipe = new BlanketRecipe();
            newRecipe.itemName = recipeName;
            blanketRecipe.Add(newRecipe);

            insertedBlanketRecipe.Add(recipeName);
        }

        MarkDirty();
    }

    public List<BlanketItemSO> GetCurrentRecipes()
    {
        List<BlanketItemSO> blanketList = new List<BlanketItemSO>();

        foreach (BlanketRecipe recipe in blanketRecipe)
        {
            blanketList.Add(blanketSOs[recipe.itemName]);
        }

        return blanketList;
    }

    public List<BlanketItemSO> GetAllRecipes()
    {
        return blanketSOs.Values.ToList();
    }

    public List<BlanketRecord> GetBlanketCraftRecordOldest()
    {
        return blanketRecord.ToList(); // 오래된 순

    }

    public List<BlanketRecord> GetBlanketCraftRecordLatest()
    {
        return blanketRecord.Reverse().ToList(); // 최신 순
    }

    public void AddBlanketCraftRecord(string item1Name, int item1Count, string item2Name, int item2Count,
        string item3Name, int item3Count, string item4Name, int item4Count, string makedRecipeName)
    {
        blanketRecord.Enqueue(new BlanketRecord
        {
            item1Name = item1Name,
            item1Count = item1Count,
            item2Name = item2Name,
            item2Count = item2Count,
            item3Name = item3Name,
            item3Count = item3Count,
            item4Name = item4Name,
            item4Count = item4Count,
            makedRecipeName = makedRecipeName,
            createdAt = DateTimeOffset.UtcNow.ToUnixTimeMilliseconds()
        });

        while (blanketRecord.Count > maxRecordCount)
            blanketRecord.Dequeue();

        isRecordDirty = true;
        MarkDirty();
    }
}
