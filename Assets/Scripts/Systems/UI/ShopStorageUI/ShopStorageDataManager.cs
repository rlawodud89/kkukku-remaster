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
    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 가게 그리드 설정 불러오기
        pathfinding.totalGridHeight = 6;
        pathfinding.totalGridWidth = 10;

        //0. 현재 가게의 인테리어 정보 파악 및 불러오기. 이불장에 이불장 본인 id 적어두기.(shopStorageClick) => 인테리어 스크립트 따로 만들어서 해야할듯.



        if (interiorData != null)
        {
            // 1. 가구 위치를 바탕으로 이동 불가능한 타일들을 미리 계산합니다.
            pathfinding.BuildObstacleMap(interiorData);
        }

        //1. 이불장 id로 이불 이름, 이불 개수 리스트 가져오기.
        tableClasses = ServiceLocator.Get<GameData>().ShopState.GetCurrentShopTables();

        //2. 작업실에 존재하는 모든 재고함 id 및 count/max 값 가져오기.
        storageClasses = ServiceLocator.Get<GameData>().Inventory.GetCurrentRoomBlanketBoxData();
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