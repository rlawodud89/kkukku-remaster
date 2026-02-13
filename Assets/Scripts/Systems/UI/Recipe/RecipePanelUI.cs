using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class RecipePanelUI : MonoBehaviour
{
    // Start is called before the first frame update
    void Start()
    {
        
    }

    // Update is called once per frame
    void Update()
    {
        
    }

    public TMP_Text blanketName;

    public UnityEngine.UI.Image blanketImage;
    
    public Transform itemPanel;
    public GameObject itemPrefab;

    public void Setup(BlanketItemSO blanket)
    {
        if (itemPanel != null)
        {
            foreach (Transform child in itemPanel)
            {
                Destroy(child.gameObject);
            }
        }
        
        // 안보이면 보이게
        if (this.gameObject.activeSelf==false)
        {
            this.gameObject.SetActive(true);
        }

        blanketName.text=blanket.itemName;

        blanketImage.sprite=blanket.image;
        
        List<RecipePair> recipePairs = blanket.recipe;
        
        foreach(var recipe in recipePairs)
        {
            GameObject item = Instantiate(itemPrefab,itemPanel);
            item.GetComponent<ItemSlotUI>().Setup(recipe);
        }
    }
}
