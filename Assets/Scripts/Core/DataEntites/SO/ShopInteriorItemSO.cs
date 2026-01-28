using UnityEngine;

[CreateAssetMenu(fileName = "ShopInteriorItem", menuName = "InteriorItemSO/ShopInteriorItemSO")]
public class ShopInteriorItemSO : ScriptableObject
{
    public string itemName;
    public ShopInteriorType shopInteriorType;
    public Sprite image;
    public GameObject prefab;
    public int price;

    [Header("이불장인 경우, 슬롯 개수")]
    public int slotCount;
}
