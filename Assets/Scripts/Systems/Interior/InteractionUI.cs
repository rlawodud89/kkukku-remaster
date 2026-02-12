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
        // targetFurniture = null; // 여기서 널 처리하면 드래그 후 다시 열 때 문제될 수 있음
    }

    // [보관 버튼] 클릭 시
    public void OnClickStore()
    {
        if (targetFurniture != null)
        {
            targetFurniture.StoreInInventory(); // 가구한테 "너 들어가" 명령
            targetFurniture = null;
        }
    }

    // [체크 버튼] 클릭 시
    public void OnClickConfirm()
    {
        if (targetFurniture != null)
        {
            // 매니저를 통해 선택 해제 (확정)
            InteriorManager.Instance.DeselectCurrent(); 
            HideMenu();
        }
    }
}