using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopStorageDataManager : MonoBehaviour
{
    // Start is called before the first frame update
    public static ShopStorageDataManager Instance { get; private set; }
    TableClass[] tableClasses;
    public StorageClass[] storageClasses { get; set; }

    private void Awake()
    {
        if (Instance == null) Instance = this;
        else Destroy(gameObject);

        // 매번 가게 돌아올때마다 처음부터 모두 가져오면 부하가 심할것
        // 꺼지지 않는 씬에 데이터 올려두고, 바뀌는 것들만 체크하는게 나을 것 같음.

        //0. 현재 가게의 인테리어 정보 파악 및 이불장에 이불장 본인 id 적어두기.(shopStorageClick)
        

        //1. 이불장 id로 이불 이름, 이불 개수 리스트 가져오기.
        //2. 작업실에 존재하는 모든 재고함 id 및 count/max 값 가져오기.
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
            table.count[itemIndex] += changeAmount;
            // 데베 또한 업뎃
        }
    }

    public void UpdateStorageData(int storageID, int changeAmount)
    {
        var storage = System.Array.Find(storageClasses, s => s.storageID == storageID);
        if (storage != null)
        {
            storage.count += changeAmount;
            //데베 또한 업뎃
        }
    }

}

public class TableClass
{
    public int tableID {  get; set; }
    public string[] itemName { get; set; }
    public int[] count { get; set; }
    public Image[] itemImage { get; set; }
}

public class StorageClass
{
    public int storageID { get; set; }
    public int count {  set; get; }
    public int max {  set; get; }
}