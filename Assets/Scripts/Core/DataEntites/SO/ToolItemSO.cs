using UnityEngine;

[CreateAssetMenu(fileName = "ToolItem", menuName = "ItemSO/ToolItemSO")]
public class ToolItemSO : ScriptableObject
{
    public string itemName;
    public ToolType toolType;
    public Sprite image;
    public int price;

    [Header("채집 도구인 경우, 채집 제한시간")]
    public float timeLimit;
}
