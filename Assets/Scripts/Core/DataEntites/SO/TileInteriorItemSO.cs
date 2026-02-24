using UnityEngine;
using UnityEngine.Tilemaps;

[CreateAssetMenu(fileName = "TileInteriorItem", menuName = "InteriorItemSO/TileInteriorItemSO")]
public class TileInteriorItemSO : ScriptableObject
{
    public string itemName;
    public TileInteriorType tileType;
    public Sprite image;
    public int price;

    [Header("타일베이스 필요없는 경우에는 그냥 비워둠")]
    public TileBase tileBase;       // 기본 타일베이스
    public TileBase topTileBase;    // 위 타일베이스
    public TileBase bottomTileBase; // 아래 타일베이스
}
