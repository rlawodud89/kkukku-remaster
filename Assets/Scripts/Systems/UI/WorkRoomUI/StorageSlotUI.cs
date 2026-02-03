using UnityEngine;
using UnityEngine.UI;
using TMPro; // TextMeshPro를 쓴다면

public class StorageSlotUI : MonoBehaviour
{
    [SerializeField] private TextMeshProUGUI nameText;  // 아이템 이름 표시
    [SerializeField] private TextMeshProUGUI countText; // 개수 표시
    [SerializeField] private Image iconImage;           // (선택) 아이콘

    public void SetData(int count)
    {
        countText.text = $"x {count}";
        
        // 만약 이름으로 이미지를 찾아야 한다면 여기서 처리
        // iconImage.sprite = ResourceManager.LoadSprite(name); 
    }
}