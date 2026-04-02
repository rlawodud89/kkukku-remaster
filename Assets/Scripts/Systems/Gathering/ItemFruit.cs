using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ItemFruit : MonoBehaviour
{
    [Header("UI 요소")]
    [SerializeField] private RectTransform ItemFruitPanel;
    [SerializeField] private Button itemBtn;
    [SerializeField] private TMP_Text countText;

    private int maxClickCount = 5;
    private int currentClickCount = 0;
    private SnackItemSO snackItem;

    [Header("연출")]
    [SerializeField] private float scaleStep = 0.06f;
    [SerializeField] private float punchAmount = 0.08f;
    [SerializeField] private float completePopMultiplier = 1.3f;

    private Vector3 originScale;
    private CanvasGroup canvasGroup;
    private Coroutine animCoroutine;
    private bool isInteractable = true;


    void Awake()
    {
        originScale = ItemFruitPanel.localScale;

        canvasGroup = GetComponent<CanvasGroup>();
        if (canvasGroup == null)
            canvasGroup = gameObject.AddComponent<CanvasGroup>();
    }



    void Start()
    {
        maxClickCount = ServiceLocator.Get<GameData>().User.GetCurrentUsedTool(ToolType.GATHERING).needClickCount;
        countText.text = (maxClickCount - currentClickCount).ToString();
    }


    public void OnClickItemFruit()
    {
        if (!isInteractable)
            return;

        currentClickCount++;

        // 탭 반응
        Grow();
        Punch();

        countText.text = (maxClickCount - currentClickCount).ToString();

        if (currentClickCount >= maxClickCount)
        {
            SetInteractable(false);
            animCoroutine = StartCoroutine(CompleteGathering());
        }
    }


    public void ResetItemFruit(Vector2 newPos)
    {
        int snackLevel = Get_RandomLevel();
        do
        {
            snackItem = ServiceLocator.Get<GameData>().Inventory.GetRandomSnackItemSO();
        } while (snackItem.level == snackLevel);

        itemBtn.image.sprite = snackItem.image;

        currentClickCount = 0;
        countText.text = maxClickCount.ToString();

        ItemFruitPanel.localScale = originScale;
        canvasGroup.alpha = 1f;

        SetInteractable(true);

        ItemFruitPanel.anchoredPosition = newPos;
        gameObject.SetActive(true);
    }


    public void SetMaxClickCount(int maxClickCount)
    {
        this.maxClickCount = maxClickCount;
        countText.text = (maxClickCount - currentClickCount).ToString();
    }

    public Rect GetFruitPanelRectAtPosition(Vector2 pos)
    {
        Vector2 size = ItemFruitPanel.rect.size;

        return new Rect(
            pos - size / 2,
            size
        );
    }

    public Rect GetFruitPanelRect()
    {
        return ItemFruitPanel.rect;
    }

    private int Get_RandomLevel()
    {
        // 높은 레벨이 덜 선택되도록 가중치 설정 
        int weight1 = 60;
        int weight2 = 30;
        int weight3 = 10;
        int totalWeight = weight1 + weight2 + weight3;
        int rand = UnityEngine.Random.Range(1, totalWeight + 1);
        if (rand <= weight1) return 1;
        else if (rand <= weight1 + weight2) return 2;
        else return 3;
    }

    private void Grow()
    {
        ItemFruitPanel.localScale += Vector3.one * scaleStep;
    }

    private void Punch()
    {
        if (animCoroutine != null)
            StopCoroutine(animCoroutine);

        animCoroutine = StartCoroutine(PunchRoutine());
    }

    IEnumerator PunchRoutine()
    {
        Vector3 baseScale = ItemFruitPanel.localScale;
        Vector3 target = baseScale + Vector3.one * punchAmount;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.08f;
            ItemFruitPanel.localScale = Vector3.Lerp(baseScale, target, t);
            yield return null;
        }

        ItemFruitPanel.localScale = baseScale;
    }

    IEnumerator CompleteGathering()
    {
        // 흔들림
        yield return StartCoroutine(Shake());

        // Pop
        Vector3 startScale = ItemFruitPanel.localScale;
        Vector3 popScale = startScale * completePopMultiplier;

        float t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.15f;
            ItemFruitPanel.localScale = Vector3.Lerp(startScale, popScale, t);
            yield return null;
        }

        // 인벤토리 추가
        GatheringManager.Instance.AddSnackToInventory(snackItem);

        // Fade out
        t = 0f;
        while (t < 1f)
        {
            t += Time.deltaTime / 0.2f;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t);
            yield return null;
        }

        gameObject.SetActive(false);
    }

    IEnumerator Shake()
    {
        Vector3 origin = ItemFruitPanel.anchoredPosition;
        float time = 0.15f;
        float strength = 12f;

        float t = 0f;
        while (t < time)
        {
            t += Time.deltaTime;
            ItemFruitPanel.anchoredPosition =
                origin + (Vector3)Random.insideUnitCircle * strength;
            yield return null;
        }

        ItemFruitPanel.anchoredPosition = origin;
    }

    private void SetInteractable(bool value)
    {
        isInteractable = value;

        canvasGroup.blocksRaycasts = value;
        canvasGroup.interactable = value;
    }
}
