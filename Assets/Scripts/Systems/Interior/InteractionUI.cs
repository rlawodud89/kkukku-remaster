using System.Data.Common;
using UnityEngine;
using UnityEngine.UI;

public class InteractionUI : MonoBehaviour
{
    public static InteractionUI Instance;

    [Header("UI Elements")]
    public GameObject menuPanel; // 버튼들이 담긴 패널
    public Button btnStore;      // 보관 버튼
    public Button btnConfirm;    // 체크(확인) 버튼

    private FurnitureMobileDrag targetFurniture; // 지금 메뉴가 열린 가구
    private Camera mainCam;

    private void Awake()
    {
        Instance = this;
        mainCam = Camera.main;
        
        btnStore.onClick.AddListener(OnClickStore);
        btnConfirm.onClick.AddListener(OnClickConfirm);
        
        HideMenu();
    }

    private void Update()
    {
        // 메뉴가 켜져 있으면 가구 따라다니기
        if (menuPanel.activeSelf && targetFurniture != null)
        {
            Vector3 screenPos = mainCam.WorldToScreenPoint(targetFurniture.transform.position);
            // 가구 머리 위로 살짝 띄우기 (Y축 +50 정도)
            menuPanel.transform.position = screenPos + new Vector3(0, 100, 0); 
        }
    }

    public void OpenMenu(FurnitureMobileDrag furniture)
    {
        targetFurniture = furniture;
        menuPanel.SetActive(true);
    }

    public void HideMenu()
    {
        menuPanel.SetActive(false);
        targetFurniture = null; 
    }

    public void OnClickStore()
    {
        if (targetFurniture != null)
        {
            if (RoomInteriorManager.Instance)
             {
            RoomInteriorManager.Instance.StoreSelectedFurniture();
             }

            if (ShopInteriorManager.Instance)
            {
                ShopInteriorManager.Instance.StoreSelectedFurniture();
            }

            // 2. 내 타겟 비우고 UI 메뉴 닫기
            targetFurniture = null;
            HideMenu();
        }
    }

    // [체크 버튼] 클릭 시
    public void OnClickConfirm()
    {
        if (targetFurniture != null)
        {
            if (RoomInteriorManager.Instance)
            {
            RoomInteriorManager.Instance.ConfirmFurnitureMove(); 
            }
            
            if (ShopInteriorManager.Instance)
            {
                ShopInteriorManager.Instance.ConfirmFurnitureMove(); 
            }
            // 2. 내 타겟 비우고 UI 메뉴 닫기
            targetFurniture = null;
            HideMenu();
        }
    }
}