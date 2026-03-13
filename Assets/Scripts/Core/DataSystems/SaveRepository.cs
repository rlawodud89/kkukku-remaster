using SQLite4Unity3d;
using System;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;
using UnityEngine.AddressableAssets;

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
    private Dictionary<string, SpecialQuestSO> specialQuestSOs;
    private Dictionary<string, NPCDataSO> customerSOs;


    public SaveRepository(SQLiteConnection connection)
    {
        this.connection = connection;

        // MakeDefaultDB();

        shopInteriorSOs = Addressables.LoadAssetsAsync<ShopInteriorItemSO>("shopInterior", null)
                .WaitForCompletion()
                .ToDictionary(i => i.itemName);
        roomInteriorSOs = Addressables.LoadAssetsAsync<RoomInteriorItemSO>("roomInterior", null)
                .WaitForCompletion()
                .ToDictionary(i => i.itemName);
        tileInteriorSOs = Addressables.LoadAssetsAsync<TileInteriorItemSO>("tileInterior", null)
                .WaitForCompletion()
                .ToDictionary(i => i.itemName);
        materialSOs = Addressables.LoadAssetsAsync<MaterialItemSO>("material", null)
                .WaitForCompletion()
                .ToDictionary(i => i.itemName);
        snackSOs = Addressables.LoadAssetsAsync<SnackItemSO>("snack", null)
                .WaitForCompletion()
                .ToDictionary(i => i.itemName);
        blanketSOs = Addressables.LoadAssetsAsync<BlanketItemSO>("blanket", null)
                .WaitForCompletion()
                .ToDictionary(i => i.itemName);
        toolSOs = Addressables.LoadAssetsAsync<ToolItemSO>("tool", null)
                .WaitForCompletion()
                .ToDictionary(i => i.itemName);
        specialQuestSOs = Addressables.LoadAssetsAsync<SpecialQuestSO>("specialQuest", null)
                .WaitForCompletion()
                .ToDictionary(i => i.questName);
        customerSOs = Addressables.LoadAssetsAsync<NPCDataSO>("customer", null)
                .WaitForCompletion()
                .ToDictionary(i => i.npcID);

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
            LoadBlanketCraftAggregate(),
            LoadStoreAggregate()
        );
    }


    // === SavePayload -> DB 적용 ===

    private void ExecuteInsert(SavePayload payload)
    {
        if (payload.Values == null || payload.Values.Count == 0)
        {
            throw new Exception($"INSERT without values is not allowed: {payload.Table}");
        }

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
        if (payload.Values == null || payload.Values.Count == 0)
        {
            throw new Exception($"UPDATE without SET values is not allowed: {payload.Table}");
        }

        if (payload.Conditions == null || payload.Conditions.Count == 0)
        {
            throw new Exception($"UPDATE without WHERE is not allowed: {payload.Table}");
        }

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
        // 전체 삭제
        if (payload.Conditions == null || payload.Conditions.Count == 0)
        {
            string sql = $"DELETE FROM {payload.Table}";
            connection.Execute(sql);
            return;
        }

        // 조건부 삭제
        var whereKeys = payload.Conditions.Keys.ToList();

        var whereClause = string.Join(" AND ",
            whereKeys.Select(k => $"{k} = ?"));

        string sqlWithWhere =
            $"DELETE FROM {payload.Table} WHERE {whereClause}";

        var values = whereKeys
            .Select(k => payload.Conditions[k])
            .ToArray();

        connection.Execute(sqlWithWhere, values);
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
            specialQuestSOs, customerSOs);

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
        List<BlanketRecipe> blanketRecipe = connection.Table<BlanketRecipe>().ToList();
        List<BlanketRecord> blanketRecord = connection.Table<BlanketRecord>().ToList();

        var aggregate = new BlanketCraftAggregate();
        aggregate.LoadBlanketCraftAggregate(blanketRecipe, blanketRecord, materialSOs, blanketSOs);

        return aggregate;
    }

    private StoreAggregate LoadStoreAggregate()
    {
        List<StoreItemList> storeItemList = connection.Table<StoreItemList>().ToList();

        var aggreagte = new StoreAggregate();
        aggreagte.LoadStoreAggregate(storeItemList, shopInteriorSOs, roomInteriorSOs, tileInteriorSOs);

        return aggreagte;
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
        connection.CreateTable<StoreItemList>();

        // User
        User user = new User();
        user.shopName = "";
        user.level = 1;
        user.energy = 0;
        user.gold = 100;
        user.moonrock = 100;
        user.playTime = 0;
        user.endScene = "BlanketShop";
        user.isOpen = false;
        user.itemShopLevel = 1;
        user.interiorInventoryLevel = 1;
        user.shopLevel = 1;
        user.bgmVol = 50;
        user.sfxVol = 50;
        user.startState = StartStateType.PROLOG;
        user.isWatchEnding = false;
        connection.Insert(user);

        // TileInteriorInventory
        TileInteriorInventory floorTile = new TileInteriorInventory();
        floorTile.itemName = "기본바닥타일";
        floorTile.tileType = TileInteriorType.FLOOR;
        connection.Insert(floorTile);

        TileInteriorInventory wallTile = new TileInteriorInventory();
        wallTile.itemName = "기본벽타일";
        wallTile.tileType = TileInteriorType.WALL;
        connection.Insert(wallTile);

        // ShopInteriorPlaced

        // RoomInteriorPlaced

        // TileInteriorPlaced
        TileInteriorPlaced shopfloor = new TileInteriorPlaced();
        shopfloor.tilePosition = TilePositionType.SHOP_FLOOR;
        shopfloor.itemName = "기본바닥타일";
        connection.Insert(shopfloor);

        TileInteriorPlaced shopwall = new TileInteriorPlaced();
        shopwall.tilePosition = TilePositionType.SHOP_WALL;
        shopwall.itemName = "기본벽타일";
        connection.Insert(shopwall);

        TileInteriorPlaced roomfloor = new TileInteriorPlaced();
        roomfloor.tilePosition = TilePositionType.ROOM_FLOOR;
        roomfloor.itemName = "기본바닥타일";
        connection.Insert(roomfloor);

        TileInteriorPlaced roomwall = new TileInteriorPlaced();
        roomwall.tilePosition = TilePositionType.ROOM_WALL;
        roomwall.itemName = "기본벽타일";
        connection.Insert(roomwall);

        // MaterialInventory

        // BlanketRecipe
        BlanketRecipe recipe = new BlanketRecipe();
        recipe.itemName = "기본이불";
        connection.Insert(recipe);

        // WorkerState

        // QuestBox

        // ToolInventory
        ToolInventory gatheringtool = new ToolInventory();
        gatheringtool.toolName = "기본채집망";
        gatheringtool.toolType = ToolType.GATHERING;
        connection.Insert(gatheringtool);

        ToolInventory fishingtool = new ToolInventory();
        fishingtool.toolName = "기본낚시대";
        fishingtool.toolType = ToolType.FISHING;
        connection.Insert(fishingtool);

        // ToolUsed
        ToolUsed gatheringUsed = new ToolUsed();
        gatheringUsed.toolType = ToolType.GATHERING;
        gatheringUsed.toolName = "기본채집망";
        connection.Insert(gatheringUsed);

        ToolUsed fishingUsed = new ToolUsed();
        fishingUsed.toolType = ToolType.FISHING;
        fishingUsed.toolName = "기본낚시대";
        connection.Insert(fishingUsed);

        // StoreItemList

    }

}
