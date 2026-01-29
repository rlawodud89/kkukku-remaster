using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(fileName = "BlanketItem", menuName = "ItemSO/BlanketItemSO")]
public class BlanketItemSO : ScriptableObject
{
    public string itemName;
    public Sprite image;
    public int level;
    public int price;
    public List<RecipePair> recipe;
}

[System.Serializable]
public class RecipePair
{
    public string itemName;
    public int count;
}
