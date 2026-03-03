using UnityEngine;

public class MainUIListener : MonoBehaviour
{
    public static MainUIListener Instance; // 중복 방지용

    [Header("껐다 켤 실제 UI 부모 객체")]
    public GameObject uiPanel;

    private void Awake()
    {
        // 씬이 이동해도 파괴되지 않게 막아주는 유저님의 아이디어 적용!
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 나 자신을 파괴하지 마라!
        }
        else
        {
            // 씬을 이동했는데 이미 똑같은 수신기가 있다면? 나는 미련 없이 파괴! (중복 방지)
            Destroy(gameObject);
        }
    }

    private void OnEnable()
    {
        UIEventManager.OnMainUIStateChanged += SetUIVisible; // 구독
    }

    private void OnDisable()
    {
        UIEventManager.OnMainUIStateChanged -= SetUIVisible; // 구독 취소
    }

    // 방송국에서 날아온 bool 값(isVisible)에 따라 껐다 켜기만 하면 끝!
    private void SetUIVisible(bool isVisible)
    {
        if (uiPanel != null)
        {
            uiPanel.SetActive(isVisible);
        }
    }
}