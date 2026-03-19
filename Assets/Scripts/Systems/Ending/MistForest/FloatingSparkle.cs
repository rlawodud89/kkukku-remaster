using UnityEngine;
using UnityEngine.UI;

[RequireComponent(typeof(RectTransform))]
[RequireComponent(typeof(Image))]
public class FloatingSparkle : MonoBehaviour
{
    RectTransform rect;
    Image img;

    Vector2 startPos;
    Vector2 randomOffset;

    float moveSpeed;
    float alphaSpeed;
    float scaleSpeed;

    float alphaOffset;
    float moveOffset;
    float scaleOffset;

    public float moveAmount = 15f;
    public float minAlpha = 0.3f;
    public float maxAlpha = 1f;

    public float scaleAmount = 0.15f;

    void Start()
    {
        rect = GetComponent<RectTransform>();
        img = GetComponent<Image>();

        startPos = rect.anchoredPosition;

        // 랜덤 파라미터
        moveSpeed = Random.Range(0.5f, 1.2f);
        alphaSpeed = Random.Range(1f, 2f);
        scaleSpeed = Random.Range(0.8f, 1.5f);

        moveOffset = Random.Range(0f, 10f);
        alphaOffset = Random.Range(0f, 10f);
        scaleOffset = Random.Range(0f, 10f);

        randomOffset = new Vector2(
            Random.Range(-moveAmount, moveAmount),
            Random.Range(-moveAmount, moveAmount)
        );
    }

    void Update()
    {
        float time = Time.time;

        // 🌟 위치 이동
        float x = Mathf.Sin(time * moveSpeed + moveOffset) * randomOffset.x;
        float y = Mathf.Cos(time * moveSpeed + moveOffset) * randomOffset.y;

        rect.anchoredPosition = startPos + new Vector2(x, y);

        // 🌟 투명도 반짝임
        float alpha = Mathf.Lerp(
            minAlpha,
            maxAlpha,
            (Mathf.Sin(time * alphaSpeed + alphaOffset) + 1f) * 0.5f
        );

        Color c = img.color;
        c.a = alpha;
        img.color = c;

        // 🌟 크기 펄스
        float scale = 1 + Mathf.Sin(time * scaleSpeed + scaleOffset) * scaleAmount;

        rect.localScale = Vector3.one * scale;
    }
}