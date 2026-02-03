using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;

public class ShopStorageClick : MonoBehaviour, IPointerClickHandler
{
    public ShopStoragePanel storagePanel;
    public int storageID; // 이불장의 고유 ID (인스펙터에서 설정)

    [Header("Sprites")]
    public Sprite emptySprite;  // 이불이 0개일 때 보여줄 빈 이불장 이미지
    public Sprite filledSprite; // 이불이 1개 이상일 때 보여줄 채워진 이불장 이미지

    private SpriteRenderer spriteRenderer;

    void Awake()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    void Start()
    {
        // 씬이 시작될 때 이불 개수를 확인하고 이미지를 변경합니다.
        UpdateSpriteState();
    }

    // 💡 핵심: 데이터에서 이불 개수를 확인하고 스프라이트를 바꾸는 함수
    public void UpdateSpriteState()
    {
        bool hasBlanket = ServiceLocator.Get<GameData>().ShopState.IsBlanketOnShopTable(storageID);

        // 2. 결과에 따라 스프라이트를 변경합니다.
        if (hasBlanket)
        {
            spriteRenderer.sprite = filledSprite; // 이불 있음
        }
        else
        {
            spriteRenderer.sprite = emptySprite; // 이불 없음 (품절)
        }
    }

    public void OnPointerClick(PointerEventData eventData)
    {
        if (EventSystem.current.IsPointerOverGameObject()) return;

        if (storagePanel != null)
        {
            storagePanel.OpenStorage(storageID);
        }
    }
}
