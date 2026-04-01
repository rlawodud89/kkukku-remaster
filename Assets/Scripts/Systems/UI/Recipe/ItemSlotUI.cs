using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class ItemSlotUI : MonoBehaviour
{

    public TMP_Text itemName;
    public UnityEngine.UI.Image itemImage;
    public TMP_Text itemCount;

    [Header("Item Sprites")]
    public Sprite cotton0;  //운무솜
    public Sprite cotton1;  //햇빛운무솜
    public Sprite cotton2;  //천공운무솜
    public Sprite yarn0;  // 꿈실
    public Sprite yarn1;  // 별빛꿈실
    public Sprite yarn2;  // 은하꿈실
    public Sprite moonpiece0;    // 달조각    
    public Sprite moonpiece1;    // 은빛달조각
    public Sprite moonpiece2;    // 천야달조각
    public Sprite deco0;   // 부드러운 실크 매듭
    public Sprite deco1;   // 앙증맞은 체크 리본
    public Sprite deco2;   // 투명하고 빛나는 물방울
    public Sprite deco3;  // 할머니의 꽃바늘

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

        switch (recipe.itemName)
        {
            case "운무솜":
                itemImage.sprite=cotton0;
                break;
            case "햇빛운무솜":
                itemImage.sprite=cotton1;
                break;
            case "천공운무솜":
                itemImage.sprite=cotton2;
                break;
            case "꿈실":
                itemImage.sprite=yarn0;
                break;
            case "별빛꿈실":
                itemImage.sprite=yarn1;
                break;
            case "은하꿈실":
                itemImage.sprite=yarn2;
                break;
            case "달조각":
                itemImage.sprite=moonpiece0;
                break;
            case "은빛달조각":
                itemImage.sprite=moonpiece1;
                break;
            case "천야달조각":
                itemImage.sprite=moonpiece2;
                break;
            case "부드러운 실크 매듭":
                itemImage.sprite=deco0;
                break;
            case "앙증맞은 체크 리본":
                itemImage.sprite=deco1;
                break;
            case "투명하고 빛나는 물방울":
                itemImage.sprite=deco2;
                break;
            case "할머니의 꽃바늘":
                itemImage.sprite=deco3;
                break;
            default:
                break;
        }

    }
}
