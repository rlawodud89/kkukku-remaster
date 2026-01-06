using UnityEngine;

[CreateAssetMenu(fileName = "NewItem", menuName = "Tycoon/Item")]
public class ItemData : ScriptableObject
{
    public int itemID;
    public string itemName;
    public int price;
    public Sprite itemSprite; // NPC가 들고 있을 이미지
}