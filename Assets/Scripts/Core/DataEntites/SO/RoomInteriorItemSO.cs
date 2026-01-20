using UnityEngine;

[CreateAssetMenu(fileName = "RoomInteriorItem", menuName = "InteriorItemSO/RoomInteriorItemSO")]
public class RoomInteriorItemSO : ScriptableObject
{
    public string itemName;

    // public RoomInteriorType roomInteriorType

    public Sprite image;

    public GameObject leftPrefab;
    public GameObject rightPrefab;

    public int price;

    [Header("재고함인 경우, 슬롯 개수")]
    public int slotCount;

    [Header("직원인 경우, 최대 스테미나와 작업시간")]
    public int maxStamina;
    public int workingTime;
}