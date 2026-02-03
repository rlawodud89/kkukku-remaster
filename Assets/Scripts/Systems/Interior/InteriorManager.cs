using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI; // 버튼 색이나 텍스트 변경용
using System.Linq;

public class InteriorManager : MonoBehaviour
{
    // 어디서든 InteriorManager.Instance로 접근할 수 있게 함 (싱글톤)
    public static InteriorManager Instance;

    public bool IsEditMode { get; private set; } = false;

    [Header("UI Reference")]
    [SerializeField] private Button editModeButton;
    [SerializeField] public Grid mainGrid;
    [SerializeField] private Sprite editModeOnSprite;
    [SerializeField] private Sprite editModeOffSprite;
    [SerializeField] private TextMeshProUGUI buttonText; // 버튼 안의 텍스트 (옵션)


    // 가구 데이터
    public List<RoomInteriorPlaced> currentPlacedList = new List<RoomInteriorPlaced>();

    private void Awake()
    {
        // 싱글톤 설정
        if (Instance == null) Instance = this;
        else Destroy(gameObject);
    }


    // 재고함이 몇 개인지 세는 메서드
    public int GetCountByType(RoomInteriorType targetType)
    {
        // 리스트를 싹 훑어서 타입이 같은 놈들의 숫자를 셈
        // (SQL의 Select Count와 똑같지만, 메모리에서 하니까 엄청 빠름)
        return currentPlacedList.Count(x => x.interiorType == targetType);
    }


    // 이 함수를 버튼에 연결하세요
    public void ToggleEditMode()
    {
        IsEditMode = !IsEditMode; // 켜져있으면 끄고, 꺼져있으면 켬
        
        UpdateUI();
    }

    private void UpdateUI()
    {
        if (IsEditMode)
        {
            Debug.Log("가구 배치 모드: ON");
            if (buttonText) buttonText.text = "가구 배치 모드";
            if (editModeButton && editModeOnSprite) editModeButton.image.sprite = editModeOnSprite;
        }
        else
        {
            Debug.Log("가구 배치 모드: OFF");
            if (buttonText) buttonText.text = "가구 배치 모드 종료";
            if (editModeButton && editModeOffSprite) editModeButton.image.sprite = editModeOffSprite;
        }
    }
}