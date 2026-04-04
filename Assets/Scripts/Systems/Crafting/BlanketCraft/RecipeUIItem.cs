using UnityEngine;
using TMPro;
using UnityEngine.UI;

public class RecipeUIItem : MonoBehaviour
{
    [Header("UI 연결")]
    public TextMeshProUGUI nameText;       // 이불 이름
    public Image iconImage;                // 이불 아이콘 (있으면 연결)
    
    private BlanketItemSO myData;
    
    
    public void SetData(BlanketItemSO data)
    {
        myData = data; // 데이터 저장

        nameText.text = data.itemName;
        if (data.image != null) iconImage.sprite = data.image;
    }

    public void OnClick()
    {
        BlanketCraftController.Instance.ApplyRecipeToSlots(myData);

        TutorialEventBus.Raise(TutorialID.SelectWorkerRecipe);
    }

}