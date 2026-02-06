using UnityEngine;

[CreateAssetMenu(fileName = "ToolItem", menuName = "ItemSO/ToolItemSO")]
public class ToolItemSO : ScriptableObject
{
    public string itemName;
    public ToolType toolType;
    public Sprite image;
    public int price;

    [Header("채집 도구인 경우, 클릭 필요 횟수")]
    public int needClickCount;
}
