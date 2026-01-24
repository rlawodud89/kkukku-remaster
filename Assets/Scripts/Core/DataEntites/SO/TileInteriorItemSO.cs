using UnityEngine;

[CreateAssetMenu(fileName = "TileInteriorItem", menuName = "InteriorItemSO/TileInteriorItemSO")]
public class TileInteriorItemSO : ScriptableObject
{
    public string itemName;

    // public TileType tileType

    public Sprite image;

    public int price;
}
