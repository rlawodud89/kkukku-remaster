using SQLite4Unity3d;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;
using UnityEngine.ResourceManagement.AsyncOperations;

public class SaveRepository
{
    private SQLiteConnection connection;

    private Dictionary<string, ShopInteriorItemSO> shopInteriorSOs;
    private Dictionary<string, RoomInteriorItemSO> roomInteriorSOs;
    private Dictionary<string, TileInteriorItemSO> tileInteriorSOs;
    private Dictionary<string, MaterialItemSO> materialSOs;
    private Dictionary<string, SnackItemSO> snackSOs;
    private Dictionary<string, BlanketItemSO> blanketSOs;
    private Dictionary<string, ToolItemSO> toolSOs;
    private Dictionary<string, QuestSO> questSOs;
    private Dictionary<string, SpecialQuestSO> specialQuestSOs;
    private Dictionary<string, LetterSO> letterSOs;
    private Dictionary<string, NPCDataSO> customerSOs;


    public SaveRepository(SQLiteConnection connection)
    {
        this.connection = connection;

        //shopInteriorSOs = Addressables.LoadAssetsAsync<ShopInteriorItemSO>("shopInterior", null)
        //        .WaitForCompletion()
        //        .ToDictionary(i => i.itemName);
        //roomInteriorSOs = Addressables.LoadAssetsAsync<RoomInteriorItemSO>("roomInterior", null)
        //        .WaitForCompletion()
        //        .ToDictionary(i => i.itemName);
        //tileInteriorSOs = Addressables.LoadAssetsAsync<TileInteriorItemSO>("tileInterior", null)
        //        .WaitForCompletion()
        //        .ToDictionary(i => i.itemName);

        materialSOs = Addressables.LoadAssetsAsync<MaterialItemSO>("material", null)
                .WaitForCompletion()
                .ToDictionary(i => i.itemName);
        //snackSOs = Addressables.LoadAssetsAsync<SnackItemSO>("snack", null)
        //        .WaitForCompletion()
        //        .ToDictionary(i => i.itemName);
        //blanketSOs = Addressables.LoadAssetsAsync<BlanketItemSO>("material", null)
        //        .WaitForCompletion()
        //        .ToDictionary(i => i.itemName);
        //toolSOs = Addressables.LoadAssetsAsync<ToolItemSO>("tool", null)
        //        .WaitForCompletion()
        //        .ToDictionary(i => i.itemName);

        //questSOs = Addressables.LoadAssetsAsync<QuestSO>("quest", null)
        //        .WaitForCompletion()
        //        .ToDictionary(i => i.questName);
        //specialQuestSOs = Addressables.LoadAssetsAsync<SpecialQuestSO>("specialQuest", null)
        //        .WaitForCompletion()
        //        .ToDictionary(i => i.questName);
        //letterSOs = Addressables.LoadAssetsAsync<LetterSO>("letter", null)
        //        .WaitForCompletion()
        //        .ToDictionary(i => i.letterName);

        //customerSOs = Addressables.LoadAssetsAsync<NPCDataSO>("customer", null)
        //        .WaitForCompletion()
        //        .ToDictionary(i => i.npcID);
    }

    public void BeginTransaction() => connection.BeginTransaction();
    public void Commit() => connection.Commit();
    public void Rollback() => connection.Rollback();


    public void Save(IAggregate aggregate)
    {
        foreach (var data in aggregate.ToSavePayloads())
        {
            switch (data.Operation)
            {
                case SaveOperation.INSERT:
                    ExecuteInsert(data);
                    break;

                case SaveOperation.UPDATE:
                    ExecuteUpdate(data);
                    break;

                case SaveOperation.DELETE:
                    ExecuteDelete(data);
                    break;
            }
        }

        aggregate.ClearDirty();
    }

    public GameData LoadAll()
    {
        return new GameData(
            LoadUserAggregate(),
            LoadInventoryAggregate(),
            LoadInteriorAggregate(),
            LoadQuestAggregate(),
            LoadShopStateAggregate(),
            LoadBlanketCraftAggregate()
        );
    }


    // === SavePayload -> DB 적용 ===

    private void ExecuteInsert(SavePayload payload)
    {
        var columns = payload.Values.Keys.ToList();
        var parameters = string.Join(", ", columns.Select(_ => "?"));

        string sql =
            $"INSERT INTO {payload.Table} ({string.Join(", ", columns)}) " +
            $"VALUES ({parameters})";

        var values = columns
            .Select(k => payload.Values[k])
            .ToArray();

        connection.Execute(sql, values);
    }


    private void ExecuteUpdate(SavePayload payload)
    {
        var setKeys = payload.Values.Keys.ToList();
        var whereKeys = payload.Conditions.Keys.ToList();

        var setClause = string.Join(", ",
            setKeys.Select(k => $"{k} = ?"));

        var whereClause = string.Join(" AND ",
            whereKeys.Select(k => $"{k} = ?"));

        string sql =
            $"UPDATE {payload.Table} SET {setClause} WHERE {whereClause}";

        var values = setKeys
            .Select(k => payload.Values[k])
            .Concat(whereKeys.Select(k => payload.Conditions[k]))
            .ToArray();

        connection.Execute(sql, values);
    }

    private void ExecuteDelete(SavePayload payload)
    {
        var whereKeys = payload.Conditions.Keys.ToList();

        var whereClause = string.Join(" AND ",
            whereKeys.Select(k => $"{k} = ?"));

        string sql =
            $"DELETE FROM {payload.Table} WHERE {whereClause}";

        var values = whereKeys
            .Select(k => payload.Conditions[k])
            .ToArray();

        connection.Execute(sql, values);
    }


    // === 첫 로딩 시 SELECT 메서드 ===

    private UserAggregate LoadUserAggregate()
    {
        User user = connection.Table<User>().FirstOrDefault();
        List<ToolUsed> toolUsed = connection.Table<ToolUsed>().ToList();

        var aggregate = new UserAggregate();
        aggregate.LoadUserAggregate(user, toolUsed, toolSOs);

        return aggregate;
    }

    private InventoryAggregate LoadInventoryAggregate()
    {
        List<ShopInteriorInventory> shopInteriorInventory = connection.Table<ShopInteriorInventory>().ToList();
        List<RoomInteriorInventory> roomInteriorInventory = connection.Table<RoomInteriorInventory>().ToList();
        List<TileInteriorInventory> tileInventory = connection.Table<TileInteriorInventory>().ToList();
        List<MaterialInventory> materialInventory = connection.Table<MaterialInventory>().ToList();
        List<SnackInventory> snackInventory = connection.Table<SnackInventory>().ToList();
        List<BlanketInventory> blanketInventory = connection.Table<BlanketInventory>().ToList();
        List<ToolInventory> toolInventory = connection.Table<ToolInventory>().ToList();

        var aggregate = new InventoryAggregate();
        aggregate.LoadInventoryAggregate(shopInteriorInventory, roomInteriorInventory, tileInventory,
            materialInventory, snackInventory, blanketInventory, toolInventory,
            shopInteriorSOs, roomInteriorSOs, tileInteriorSOs, materialSOs, snackSOs, blanketSOs, toolSOs);

        return aggregate;
    }

    private InteriorAggregate LoadInteriorAggregate()
    {
        List<ShopInteriorPlaced> shopPlaced = connection.Table<ShopInteriorPlaced>().ToList();
        List<RoomInteriorPlaced> roomPlaced = connection.Table<RoomInteriorPlaced>().ToList();
        List<TileInteriorPlaced> tilePlaced = connection.Table<TileInteriorPlaced>().ToList();

        var aggregate = new InteriorAggregate();
        aggregate.LoadInteriorAggregate(shopPlaced, roomPlaced, tilePlaced,
            shopInteriorSOs, roomInteriorSOs, tileInteriorSOs);

        return aggregate;
    }

    private QuestAggregate LoadQuestAggregate()
    {
        List<QuestBox> questBox = connection.Table<QuestBox>().ToList();
        List<SpecialQuestBox> specialQuestBox = connection.Table<SpecialQuestBox>().ToList();
        List<LetterBox> letterBox = connection.Table<LetterBox>().ToList();

        var aggregate = new QuestAggregate();
        aggregate.LoadQuestAggregate(questBox, specialQuestBox, letterBox,
            questSOs, specialQuestSOs, letterSOs);

        return aggregate;
    }

    private ShopStateAggregate LoadShopStateAggregate()
    {
        List<ShopTable> shopTable = connection.Table<ShopTable>().ToList();
        List<WorkerState> workerState = connection.Table<WorkerState>().ToList();

        var aggregate = new ShopStateAggregate();
        aggregate.LoadShopStateAggregate(shopTable, workerState, blanketSOs, customerSOs);

        return aggregate;
    }

    private BlanketCraftAggregate LoadBlanketCraftAggregate()
    {
        List<BlanketRecord> blanketRecord = connection.Table<BlanketRecord>().ToList();
        List<BlanketRecipe> blanketRecipe = connection.Table<BlanketRecipe>().ToList();

        var aggregate = new BlanketCraftAggregate();
        aggregate.LoadBlanketCraftAggregate(blanketRecipe, blanketRecord, materialSOs, blanketSOs);

        return aggregate;
    }


    // === 새로운 유저의 데이터 만드는 메서드 (게임 최초 실행 시 실행) ===

    public void MakeDefaultDB()
    {
        connection.CreateTable<User>();
        connection.CreateTable<ShopInteriorInventory>();
        connection.CreateTable<RoomInteriorInventory>();
        connection.CreateTable<TileInteriorInventory>();
        connection.CreateTable<ShopInteriorPlaced>();
        connection.CreateTable<RoomInteriorPlaced>();
        connection.CreateTable<TileInteriorPlaced>();
        connection.CreateTable<MaterialInventory>();
        connection.CreateTable<SnackInventory>();
        connection.CreateTable<BlanketInventory>();
        connection.CreateTable<BlanketRecipe>();
        connection.CreateTable<BlanketRecord>();
        connection.CreateTable<ShopTable>();
        connection.CreateTable<WorkerState>();
        connection.CreateTable<QuestBox>();
        connection.CreateTable<SpecialQuestBox>();
        connection.CreateTable<LetterBox>();
        connection.CreateTable<ToolInventory>();
        connection.CreateTable<ToolUsed>();

        // User

        // TileInteriorInventory

        // ShopInteriorPlaced

        // RoomInteriorPlaced

        // TileInteriorPlaced

        // MaterialInventory

        // BlanketRecipe

        // WorkerState

        // QuestBox

        // ToolInventory

        // ToolUsed
    }
}
