using JetBrains.Annotations;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

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
    private static int maxRecordCount = 5;


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
        yield return new SavePayload
        {
            Operation = SaveOperation.DELETE,
            Table = "BlanketRecord"
        };
        foreach (var br in blanketRecord)
        {
            yield return new SavePayload
            {
                Operation = SaveOperation.INSERT,
                Table = "BlanketRecord",
                Values = new Dictionary<string, object>()
                {
                    { "yarnName", br.yarnName },
                    { "cottonName", br.cottonName },
                    { "moonpieceName", br.moonpieceName },
                    { "decoName", br.decoName },
                    { "makedRecipeName", br.makedRecipeName },
                }
            };
        }

    }

    public void LoadBlanketCraftAggregate(IEnumerable<BlanketRecipe> blanketRecipe, IEnumerable<BlanketRecord> blanketRecord,
        Dictionary<string, MaterialItemSO> materialSOs, Dictionary<string, BlanketItemSO> blanketSOs)
    {
        this.blanketRecipe = blanketRecipe.ToList();
        this.blanketRecord = new Queue<BlanketRecord>(blanketRecord);

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
}
