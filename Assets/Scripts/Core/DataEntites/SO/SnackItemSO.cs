using UnityEngine;

[CreateAssetMenu(fileName = "SnackItem", menuName = "ItemSO/SnackItemSO")]
public class SnackItemSO : ScriptableObject
{
    public string itemName;
    public Sprite image;
    public int level;
    [Header("직원에게 먹였을 때, 증가하는 스태미나량")]
    public int stamina;
}
