using UnityEngine;

[CreateAssetMenu(fileName = "MaterialItem", menuName = "ItemSO/MaterialItemSO")]
public class MaterialItemSO : ScriptableObject
{
    public string itemName;
    public MaterialType materialType;
    public Sprite image;
    public int level;
    public int price;

    [Header("꾸미기 재료인 경우, 속성 선택")]
    public DecoMaterialType decoMaterialType;
}
