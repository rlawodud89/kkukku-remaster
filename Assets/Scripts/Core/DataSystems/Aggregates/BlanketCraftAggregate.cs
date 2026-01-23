using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class BlanketCraftAggregate : IAggregate
{
    private List<BlanketRecipe> blanketRecipe;
    private Queue<BlanketRecord> blanketRecord;


    private HashSet<string> insertedBlanketRecipe = new();
    private static int maxRecordCount = 5;


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
        foreach (var ibr in insertedBlanketRecipe)
        {
            yield return new SavePayload
            {
                Operation = SaveOperation.INSERT,
                Table = "BlanketRecipe",
                Values = new Dictionary<string, object>()
                {
                    { "itemName", ibr }
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

    public void LoadBlanketCraftAggregate(IEnumerable<BlanketRecipe> blanketRecipe, IEnumerable<BlanketRecord> blanketRecord)
    {
        this.blanketRecipe = blanketRecipe.ToList();
        this.blanketRecord = new Queue<BlanketRecord>(blanketRecord);
    }


    // === 게임 플레이 메서드 ===
}
