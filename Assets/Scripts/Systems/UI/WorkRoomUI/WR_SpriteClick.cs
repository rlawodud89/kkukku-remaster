using UnityEngine;

public class WR_SpriteClick : MonoBehaviour
{
    private GameObject targetPanel;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();

        FindPanelByTag();
    }

private void FindPanelByTag()
    {
        string panelName = "";

        switch (gameObject.tag)
        {
            case "PersonalCraftBox": panelName = "PersonalCraftBoxPanel"; break;
            case "SnackBox": panelName = "SnackBoxPanel"; break;
            case "FoxEmployee": panelName = "EmployeePanel"; break;
            case "CatEmployee": panelName = "EmployeePanel"; break;
            case "LeopardEmployee": panelName = "EmployeePanel"; break;
        }

        if (string.IsNullOrEmpty(panelName)) return;

        // 1. UI들이 모여있는 부모 캔버스나 매니저를 먼저 찾습니다. (이건 켜져 있어야 함)
        GameObject canvas = GameObject.Find("Canvas"); // 혹은 "UICanvas" 등 실제 이름

        if (canvas != null)
        {
            // 2. transform.Find는 꺼져있는 자식 오브젝트도 찾을 수 있습니다.
            Transform foundTransform = canvas.transform.Find(panelName);
            
            if (foundTransform != null)
            {
                targetPanel = foundTransform.gameObject;
            }
            else
            {
                Debug.LogError($"Canvas 안에서 '{panelName}'을 찾을 수 없습니다.");
            }
        }
    }


    private void OnMouseDown()
    {
        if (InteriorManager.Instance != null && InteriorManager.Instance.IsEditMode)
        {
            return; // 아래 코드를 실행하지 않고 여기서 끝냅니다.
        }

        // 스프라이트 클릭 시 패널 활성화
        if (targetPanel != null)
        {
            targetPanel.SetActive(true);
        }
    }

    public void SetTargetPanel(GameObject panelObject)
    {
        targetPanel = panelObject;
    }
}
