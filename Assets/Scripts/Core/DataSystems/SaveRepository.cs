using SQLite4Unity3d;
using System.Collections.Generic;
using System.Linq;
using UnityEngine.UIElements;

public class SaveRepository
{
    private SQLiteConnection connection;

    public SaveRepository(SQLiteConnection connection)
    {
        this.connection = connection;
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
        //connection.CreateTable<User>();
        //User user = new User();
        //user.shopName = "shopName";
        //user.gold = 100;
        //connection.Insert(user);

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
        aggregate.LoadUserAggregate(user, toolUsed);

        return aggregate;
    }

    private InventoryAggregate LoadInventoryAggregate()
    {

    }

    private InteriorAggregate LoadInteriorAggregate()
    {
        List<ShopInteriorPlaced> shopPlaced = connection.Table<ShopInteriorPlaced>().ToList();
        List<RoomInteriorPlaced> roomPlaced = connection.Table<RoomInteriorPlaced>().ToList();
        List<TileInteriorPlaced> tilePlaced = connection.Table<TileInteriorPlaced>().ToList();

        var aggregate = new InteriorAggregate();
        aggregate.LoadInteriorAggregate(shopPlaced, roomPlaced, tilePlaced);

        return aggregate;
    }

    private QuestAggregate LoadQuestAggregate()
    {
        List<QuestBox> questBox = connection.Table<QuestBox>().ToList();
        List<SpecialQuestBox> specialQuestBox = connection.Table<SpecialQuestBox>().ToList();
        List<LetterBox> letterBox = connection.Table<LetterBox>().ToList();

        var aggregate = new QuestAggregate();
        aggregate.LoadQuestAggregate(questBox, specialQuestBox, letterBox);

        return aggregate;
    }

    private ShopStateAggregate LoadShopStateAggregate()
    {
        List<ShopTable> shopTable = connection.Table<ShopTable>().ToList();
        List<WorkerState> workerState = connection.Table<WorkerState>().ToList();

        var aggregate = new ShopStateAggregate();
        aggregate.LoadShopStateAggregate(shopTable, workerState);

        return aggregate;
    }

    private BlanketCraftAggregate LoadBlanketCraftAggregate()
    {
        List<BlanketRecord> blanketRecord = connection.Table<BlanketRecord>().ToList();
        List<BlanketRecipe> blanketRecipe = connection.Table<BlanketRecipe>().ToList();

        var aggregate = new BlanketCraftAggregate();
        aggregate.LoadBlanketCraftAggregate(blanketRecipe, blanketRecord);

        return aggregate;
    }
}
