using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;


public class ShopStateAggregate : IAggregate
{
    // === 런타임 데이터 ===

    private Dictionary<int, Dictionary<string, ShopTable>> shopTable;
    private Dictionary<int, WorkerState> workerState;

    // === SO 데이터 ===

    private Dictionary<string, BlanketItemSO> blanketSOs;
    private Dictionary<string, NPCDataSO> customerSOs;

    // === 변경 사항 저장소 ===

    private Dictionary<(int tableID, string itemName), SaveOperation> shopTableChanges = new();
    private Dictionary<int, SaveOperation> workerStateChanges = new();


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

        shopTableChanges.Clear();
        workerStateChanges.Clear();
    }

    public IEnumerable<SavePayload> ToSavePayloads()
    {
        if (!IsDirty)
            yield break;

        // 이불장 현황
        foreach (var ((tableID, itemName), change) in shopTableChanges)
        {
            switch (change)
            {
                case SaveOperation.INSERT:
                    ShopTable insertTable = shopTable[tableID][itemName];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.INSERT,
                        Table = "ShopTable",
                        Values = new Dictionary<string, object>
                        {
                            { "tableID", insertTable.tableID },
                            { "itemName", insertTable.itemName },
                            { "count", insertTable.count }
                        }
                    };
                    break;

                case SaveOperation.UPDATE:
                    ShopTable updateTable = shopTable[tableID][itemName];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.UPDATE,
                        Table = "ShopTable",
                        Values = new Dictionary<string, object>
                        {
                            { "count", updateTable.count }
                        },
                        Conditions = new Dictionary<string, object>
                        {
                            { "tableID", updateTable.tableID },
                            { "itemName", updateTable.itemName }
                        }
                    };

                    break;

                case SaveOperation.DELETE:
                    yield return new SavePayload
                    {
                        Operation = SaveOperation.DELETE,
                        Table = "ShopTable",
                        Conditions = new Dictionary<string, object>
                        {
                            { "tableID", tableID },
                            { "itemName", itemName }
                        }
                    };

                    break;
            }
        }

        // 직원 상태
        foreach (var (workerID, change) in workerStateChanges)
        {
            switch (change)
            {
                case SaveOperation.INSERT:
                    WorkerState insertWorker = workerState[workerID];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.INSERT,
                        Table = "WorkerState",
                        Values = new Dictionary<string, object>
                        {
                            { "workerID", insertWorker.workerID },
                            { "stamina", insertWorker.stamina },
                            { "workingItem", insertWorker.workingItem },
                            { "progress", insertWorker.progress },
                            { "skill", insertWorker.skill }
                        }
                    };

                    break;

                case SaveOperation.UPDATE:
                    WorkerState updateWorker = workerState[workerID];

                    yield return new SavePayload
                    {
                        Operation = SaveOperation.UPDATE,
                        Table = "WorkerState",
                        Values = new Dictionary<string, object>
                        {
                            { "stamina", updateWorker.stamina },
                            { "workingItem", updateWorker.workingItem },
                            { "progress", updateWorker.progress },
                            { "skill", updateWorker.skill }
                        },
                        Conditions = new Dictionary<string, object>
                        {
                            { "workerID", updateWorker.workerID }
                        }
                    };

                    break;

                case SaveOperation.DELETE:
                    yield return new SavePayload
                    {
                        Operation = SaveOperation.DELETE,
                        Table = "WorkerState",
                        Conditions = new Dictionary<string, object>
                        {
                            { "workerID", workerID }
                        }
                    };

                    break;
            }
        }
    }

    public void LoadShopStateAggregate(IEnumerable<ShopTable> shopTable, IEnumerable<WorkerState> workerState,
        Dictionary<string, BlanketItemSO> blanketSOs, Dictionary<string, NPCDataSO> customerSOs)
    {
        this.shopTable = shopTable
        .GroupBy(st => st.tableID)
        .ToDictionary(
            g => g.Key,
            g => g.ToDictionary(st => st.itemName)
        );
        this.workerState = workerState.ToDictionary(ws => ws.workerID);

        this.blanketSOs = blanketSOs;
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

}
