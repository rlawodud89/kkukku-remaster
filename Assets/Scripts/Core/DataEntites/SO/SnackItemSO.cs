using UnityEngine;

[CreateAssetMenu(fileName = "SnackItem", menuName = "ItemSO/SnackItemSO")]
public class SnackItemSO : ScriptableObject
{
    public string itemName;

    public Sprite image;

    public int level;
    public int stamina;
}
