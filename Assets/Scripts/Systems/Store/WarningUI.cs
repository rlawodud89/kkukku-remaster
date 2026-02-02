using System.Collections;
using UnityEngine;
using TMPro;

public class WarningUI : MonoBehaviour
{
    public TextMeshProUGUI warningText;
    public CanvasGroup canvasGroup;
    public float stayTime = 4f;
    public float fadeOutTime = 0.5f;

    Coroutine hideCoroutine;

    private void Awake()
    {
        canvasGroup.alpha = 0f;
    }

    public void Show(string message)
    {
        warningText.text = message;
        canvasGroup.alpha = 1f;

        if (hideCoroutine != null)
            StopCoroutine(hideCoroutine);

        hideCoroutine = StartCoroutine(HideAfterDelay());
    }

    IEnumerator HideAfterDelay()
    {
        // 유지
        yield return new WaitForSeconds(stayTime);

        // 페이드아웃
        float t = 0f;
        while (t < fadeOutTime)
        {
            t += Time.deltaTime;
            canvasGroup.alpha = Mathf.Lerp(1f, 0f, t / fadeOutTime);
            yield return null;
        }

        canvasGroup.alpha = 0f;
        hideCoroutine = null;
    }
}

