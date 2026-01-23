using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ShopStateAggregate : IAggregate
{
    private Dictionary<int, Dictionary<string, ShopTable>> shopTable;
    private Dictionary<int, WorkerState> workerState;

    private HashSet<(int tableID, string itemName)> insertedShopTable = new();
    private HashSet<(int tableID, string itemName)> updatedShopTable = new();
    private HashSet<(int tableID, string itemName)> deletedShopTable = new();

    private HashSet<int> insertedWorkerState = new();
    private HashSet<int> updatedWorkerState = new();
    private HashSet<int> deletedWorkerState = new();


    public bool IsDirty { get; private set; }

    private void MarkDirty()
    {
        IsDirty = true;
        ServiceLocator.Get<DirtyDataRegistry>().RegisterDirty(this);
    }

    public void ClearDirty()
    {
        IsDirty = false;

        insertedShopTable.Clear();
        updatedShopTable.Clear();
        deletedShopTable.Clear();

        insertedWorkerState.Clear();
        updatedWorkerState.Clear();
        deletedWorkerState.Clear();
    }

    public IEnumerable<SavePayload> ToSavePayloads()
    {
        if (!IsDirty)
            yield break;

        // 이불장 현황
        foreach (var ist in insertedShopTable)
        {
            ShopTable table = shopTable[ist.tableID][ist.itemName];

            yield return new SavePayload
            {
                Operation = SaveOperation.INSERT,
                Table = "ShopTable",
                Values = new Dictionary<string, object>
                {
                    { "tableID", table.tableID },
                    { "itemName", table.itemName },
                    { "count", table.count }
                }
            };
        }
        foreach (var ust in updatedShopTable)
        {
            ShopTable table = shopTable[ust.tableID][ust.itemName];

            yield return new SavePayload
            {
                Operation = SaveOperation.UPDATE,
                Table = "ShopTable",
                Values = new Dictionary<string, object>
                {
                    { "count", table.count }
                },
                Conditions = new Dictionary<string, object>
                {
                    { "tableID", table.tableID },
                    { "itemName", table.itemName }
                }
            };
        }
        foreach (var dst in deletedShopTable)
        {
            yield return new SavePayload
            {
                Operation = SaveOperation.DELETE,
                Table = "ShopTable",
                Conditions = new Dictionary<string, object>
                {
                    { "tableID", dst.tableID },
                    { "itemName", dst.itemName }
                }
            };
        }

        // 직원 상태
        foreach (var iws in insertedWorkerState)
        {
            WorkerState worker = workerState[iws];

            yield return new SavePayload
            {
                Operation = SaveOperation.INSERT,
                Table = "WorkerState",
                Values = new Dictionary<string, object>
                {
                    { "workerID", worker.workerID },
                    { "stamina", worker.stamina },
                    { "workingItem", worker.workingItem },
                    { "progress", worker.progress },
                    { "skill", worker.skill }
                }
            };
        }
        foreach (var uws in updatedWorkerState)
        {
            WorkerState worker = workerState[uws];

            yield return new SavePayload
            {
                Operation = SaveOperation.UPDATE,
                Table = "WorkerState",
                Values = new Dictionary<string, object>
                {
                    { "stamina", worker.stamina },
                    { "workingItem", worker.workingItem },
                    { "progress", worker.progress },
                    { "skill", worker.skill }
                },
                Conditions = new Dictionary<string, object>
                {
                    { "workerID", worker.workerID }
                }
            };
        }
        foreach (var dws in deletedWorkerState)
        {
            yield return new SavePayload
            {
                Operation = SaveOperation.DELETE,
                Table = "WorkerState",
                Conditions = new Dictionary<string, object>
                {
                    { "workerID", dws }
                }
            };
        }

    }

    public void LoadShopStateAggregate(IEnumerable<ShopTable> shopTable, IEnumerable<WorkerState> workerState)
    {
        this.shopTable = shopTable
        .GroupBy(st => st.tableID)
        .ToDictionary(
            g => g.Key,
            g => g.ToDictionary(st => st.itemName)
        );
        this.workerState = workerState.ToDictionary(ws => ws.workerID);
    }

    // === 게임 플레이 메서드 ===

}
