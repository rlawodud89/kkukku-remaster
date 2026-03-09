using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopStorageDataManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static ShopStorageDataManager Instance { get; private set; }
    public List<TableClass> tableClasses;
    public List <StorageClass> storageClasses { get; set; }
    public ShopInteriorData interiorData { get; set; }

    public Pathfinding pathfinding;
    public CashierManager cashierManager;
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 가게 그리드 설정 불러오기
        pathfinding.totalGridHeight = 6;
        pathfinding.totalGridWidth = 10;
        pathfinding.CalculateGridOrigin();

        interiorData = new ShopInteriorData();
        interiorData.Table = new List<Interiorinfo>();
        interiorData.Interior = new List<Interiorinfo>();


        /*
        int ID = ServiceLocator.Get<GameData>().Interior.AddShopInterior(0, "기본 벽장");
        ID = ServiceLocator.Get<GameData>().Interior.AddShopInterior(3, "기본 벽장");
        ID = ServiceLocator.Get<GameData>().Interior.AddShopInterior(31, "기본 진열장");
        ID = ServiceLocator.Get<GameData>().Interior.AddShopInterior(34, "기본 진열장");
        ID = ServiceLocator.Get<GameData>().Interior.AddShopInterior(6, "다람쥐 캐셔");
        */

        /*
        ServiceLocator.Get<GameData>().ShopState.AdjustShopTableBlanketCount(1, "자수 꽃무늬 이불", 4);
        ServiceLocator.Get<GameData>().ShopState.AdjustShopTableBlanketCount(2, "자수 꽃무늬 이불", 5);
        ServiceLocator.Get<GameData>().ShopState.AdjustShopTableBlanketCount(3, "자수 꽃무늬 이불", 6);
        ServiceLocator.Get<GameData>().ShopState.AdjustShopTableBlanketCount(4, "자수 꽃무늬 이불", 7);

        ServiceLocator.Get<GameData>().ShopState.AdjustShopTableBlanketCount(1, "살구빛 이불", 4);
        ServiceLocator.Get<GameData>().ShopState.AdjustShopTableBlanketCount(2, "살구빛 이불", 5);
        ServiceLocator.Get<GameData>().ShopState.AdjustShopTableBlanketCount(3, "살구빛 이불", 6);
        ServiceLocator.Get<GameData>().ShopState.AdjustShopTableBlanketCount(4, "살구빛 이불", 7);
        */


        // pathfinding 할당 확인
        if (pathfinding == null) pathfinding = FindObjectOfType<Pathfinding>();

        //0. 현재 가게의 인테리어 정보 파악 및 불러오기. 이불장에 이불장 본인 id 적어두기.(shopStorageClick) => 인테리어 스크립트 따로 만들어서 해야할듯.
        LoadInteriorData();

        if (interiorData != null)
        {
            // 1. 가구 위치를 바탕으로 이동 불가능한 타일들을 미리 계산합니다.
            pathfinding.BuildObstacleMap(interiorData);
        }

        //1. 이불장 id로 이불 이름, 이불 개수 리스트 가져오기.
        tableClasses = ServiceLocator.Get<GameData>().ShopState.GetCurrentShopTables();

        Debug.Log($"<color=yellow>[테이블 데이터 확인]</color> 총 {tableClasses.Count}개의 이불장 데이터를 가져왔습니다.");

        foreach (var table in tableClasses)
        {
            // 각 이불장 안에 든 아이템 이름들과 개수를 문자열로 합칩니다.
            string itemDetails = "";
            for (int i = 0; i < table.itemName.Count; i++)
            {
                itemDetails += $"[{table.itemName[i]}: {table.count[i]}개] ";
            }

            Debug.Log($"<b>[이불장 ID: {table.tableID}]</b> 내용물: {itemDetails}");

            if (table.itemName.Count == 0 || !table.count.Exists(c => c > 0))
            {
                Debug.LogWarning($"<color=orange>[경고]</color> ID {table.tableID}번 이불장은 현재 비어있습니다.");
            }
        }

        //2. 작업실에 존재하는 모든 재고함 id 및 count/max 값 가져오기.
        storageClasses = ServiceLocator.Get<GameData>().Inventory.GetCurrentRoomBlanketBoxData();

        cashierManager.cashierPosIndex = interiorData.Casher.placement;
        cashierManager.cashierWidth = interiorData.Casher.Width;
    }

    private void LoadInteriorData()
    {

        var placedItems = ServiceLocator.Get<GameData>().Interior.GetCurrentShopInterior();

        Debug.Log($"<color=yellow>[데이터 로드 시작]</color> 총 {placedItems.Count}개의 데이터를 검사합니다.");

        foreach (var placed in placedItems)
        {
            Debug.Log($"<color=white>[DB 데이터 확인]</color> 가져온 이름: <b>{placed.itemName}</b>, 그리드번호: {placed.gridNumber}");

            var so = ServiceLocator.Get<GameData>().Inventory.GetShopInteriorItemSO(placed.itemName);
            if (so == null)
            {
                Debug.LogError($"<color=red>[데이터 오류]</color> '{placed.itemName}'에 해당하는 ShopInteriorItemSO를 찾을 수 없습니다! (아이템 이름 오타 확인 필요)");
                continue;
            }

            Interiorinfo info = new Interiorinfo();
            info.placement = placed.gridNumber;
            info.prefab = so.prefab;
            info.Width = so.itemWidth;
            info.Height = so.itemHeight;
            info.ID = placed.ID; // DB에서 가져온 고유 ID

            if (so.shopInteriorType == ShopInteriorType.TABLE)
            {
                interiorData.Table.Add(info);
            }
            else if (so.shopInteriorType == ShopInteriorType.CASHER)
            {
                interiorData.Casher = info;
            }
            else
            {
                interiorData.Interior.Add(info);
            }
        }

        Debug.Log($"<color=cyan>[데이터 로드 결과]</color> 가구 총 개수: {interiorData.Table.Count + interiorData.Interior.Count + (interiorData.Casher != null ? 1 : 0)}");
        Debug.Log($" - 테이블(Table): {interiorData.Table.Count}개");
        Debug.Log($" - 일반가구(Interior): {interiorData.Interior.Count}개");
        Debug.Log($" - 계산대(Casher): {(interiorData.Casher != null ? "있음" : "없음")}");
    }

    public bool GetTableClass(int tableID, out TableClass result)
    {
        foreach (TableClass b in tableClasses)
        {
            if (b.tableID == tableID)
            {
                result = b;
                return true;
            }
        }

        result = null;
        Debug.LogWarning($"ID {tableID}를 찾지 못했습니다.");
        return false;
    }

    public void UpdateTableData(int tableID, int itemIndex, int changeAmount)
    {
        if (GetTableClass(tableID, out var table))
        {
            ServiceLocator.Get<GameData>().ShopState.AdjustShopTableBlanketCount(tableID, table.itemName[itemIndex], changeAmount);
            table.count[itemIndex] += changeAmount;
        }
    }

    public void UpdateStorageData(int storageID, int changeAmount)
    {
        var storage = storageClasses.Find(s => s.storageID == storageID);
        if (storage != null)
        {
            storage.count += changeAmount;
        }
    }

}

public class TableClass
{
    public int tableID { get; set; }
    public List<string> itemName { get; set; } = new();
    public List<int> count { get; set; } = new();
    public List<Image> itemImage { get; set; } = new();
}

public class StorageClass
{
    public int storageID { get; set; }
    public int count { set; get; }
    public int max { set; get; }
}

public class ShopInteriorData
{
    public Interiorinfo Casher {  get; set; }
    public List<Interiorinfo> Interior {  get; set; }
    public List<Interiorinfo> Table { get; set; }
}

public class Interiorinfo
{
    public int placement { get; set; }
    public GameObject prefab {  get; set; }
    public int Width {  get; set; }
    public int Height { get; set; }

    public int ID {  get; set; } // 이불장의 경우 ID도 포함시켜 주세요
}