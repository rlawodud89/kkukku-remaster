using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.Tilemaps;

public class ShopInteriorManager : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }
}

public class FurnitureItem
{
    public string itemName;       // 이름
    public Sprite itemImage;      // 이미지
    public Vector2Int gridSize;   // 그리드 개수 (예: 가로 2칸, 세로 1칸을 차지한다면 X:2, Y:1)
    public int quantity;          // 보유 개수
    public GameObject prefab;
}

public class TileItem
{
    public string itemName;       // 이름
    public Sprite itemImage;      // 이미지
    public TileBase tileBase;
}
public class WallpaperItem
{
    public string itemName;       // 이름
    public Sprite itemImage;      // 이미지
    public TileBase tileBase;
}
