using UnityEngine;
using UnityEngine.UI;
using TMPro;

// 이 스크립트를 '기록 이력 프리팹' 최상단에 붙여주세요!
public class RecipeHistorySlotUI : MonoBehaviour
{
    public Image[] iconImage;
    public TextMeshProUGUI[] qtyText;
    public Image BlanketImage;
    public TextMeshProUGUI successText;

    // 외부에서 데이터(이미지, 이름 등)를 쏙 넣어주면 UI를 바꿔주는 함수
    public void SetHistoryData(Sprite[] icon,int[] num, string itemName)
    {
        for (int i = 0; i < iconImage.Length; i++)
        {
            if (i < icon.Length)
                iconImage[i].sprite = icon[i];
            if (i < num.Length)
                qtyText[i].text = num[i].ToString();
        }

        var itemSO =ServiceLocator.Get<GameData>().Inventory.GetBlanketItemSO(itemName);
        BlanketImage.sprite  = itemSO.image;
        if (itemName == "엉성한 이불")
        {
            successText.text = "실패";
            successText.color = Color.red;
        }
        else
        {
            successText.text = "성공";
            successText.color = Color.green;
        }
    }
}