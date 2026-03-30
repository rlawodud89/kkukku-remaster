using UnityEngine;

[CreateAssetMenu(menuName = "GameData/ShopLevel")]
public class ShopLevelSO : ScriptableObject
{
    public int level;
    public int width;
    public int height;
    public Vector3 startPos;
}