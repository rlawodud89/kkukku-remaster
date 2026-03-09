using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class CaughtFishSlotUI : MonoBehaviour
{
    public Image iconImage;
    public TextMeshProUGUI countText;

    // 처음 생성될 때 이미지와 수량 세팅
    public void Setup(Sprite icon, int count)
    {
        if (iconImage != null) iconImage.sprite = icon;
        UpdateCount(count);
    }

    // 이미 있는 생선일 때 수량만 갱신
    public void UpdateCount(int count)
    {
        if (countText != null) countText.text = count.ToString();
    }
}