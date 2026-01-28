using UnityEngine;

[CreateAssetMenu(fileName = "MaterialItem", menuName = "ItemSO/MaterialItemSO")]
public class MaterialItemSO : ScriptableObject
{
    public string itemName;

    public Sprite image;

    public int level;
    public int price;
    // public MaterialType materialType
}
