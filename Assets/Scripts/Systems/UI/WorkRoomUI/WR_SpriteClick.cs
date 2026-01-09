using UnityEngine;

public class WR_SpriteClick : MonoBehaviour
{
    [SerializeField] private GameObject targetPanel;
    private SpriteRenderer spriteRenderer;

    private void Start()
    {
        spriteRenderer = GetComponent<SpriteRenderer>();
    }

    private void OnMouseDown()
    {
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
