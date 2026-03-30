using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "GameData/StoreItemList")]
public class StoreItemListSO : ScriptableObject
{
    public StoreType storeType;

    public List<MaterialItemSO> materialItems;
    public List<RoomInteriorItemSO> workerItems;
    public List<ToolItemSO> toolItmes;
}