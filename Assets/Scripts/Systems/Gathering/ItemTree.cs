using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ItemTree : MonoBehaviour
{
    [SerializeField] private List<ItemFruit> fruits;

    void Start()
    {
        GatheringManager manager = GatheringManager.Instance;
        manager.RegisterItemTree(this);
    }

    public void ResetItemTree()
    {
        foreach (var fruit in fruits)
        {
            fruit.ResetItemFruit();
        }
    }
}
