using UnityEngine;
using UnityEngine.UI;

public class HighlightMask : Image
{
    private Material runtimeMaterial;

    protected override void Awake()
    {
        base.Awake();
        runtimeMaterial = material;
    }

    public override bool IsRaycastLocationValid(Vector2 screenPoint, Camera eventCamera)
    {
        RectTransform rect = rectTransform;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            rect,
            screenPoint,
            eventCamera,
            out Vector2 localPoint);

        Vector2 uv = new Vector2(
            (localPoint.x - rect.rect.x) / rect.rect.width,
            (localPoint.y - rect.rect.y) / rect.rect.height
        );

        float dist = Vector2.Distance(uv, HighlightSystem.CurrentHoleCenter);

        if (dist < HighlightSystem.CurrentHoleSize)
            return false;

        return true;
    }
}