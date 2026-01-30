using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Tycoon/Item")]
public class ItemData : ScriptableObject
{
    public int itemID;
    public string itemName;
    public int price;
}