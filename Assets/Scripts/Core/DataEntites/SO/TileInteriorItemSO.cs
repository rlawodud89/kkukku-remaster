using UnityEngine;

[CreateAssetMenu(fileName = "TileInteriorItem", menuName = "InteriorItemSO/TileInteriorItemSO")]
public class TileInteriorItemSO : ScriptableObject
{
    public string itemName;
    public TileInteriorType tileType;
    public Sprite image;
    public int price;
}
