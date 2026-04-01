using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemSlotUI : MonoBehaviour
{

    public TMP_Text itemName;
    public UnityEngine.UI.Image itemImage;
    public TMP_Text itemCount;

    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public void Setup(RecipePair recipe)
    {
        itemName.text=recipe.itemName;
        itemCount.text="X"+recipe.count.ToString();
        
    }
}
