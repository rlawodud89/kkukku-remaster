using UnityEngine;
using UnityEngine.UI;

[AddComponentMenu("Layout/Flow Layout Group")]
public class FlowLayoutGroup : LayoutGroup
{
    [SerializeField] protected float spacingX = 5f;
    [SerializeField] protected float spacingY = 5f;

    // 유니티 레이아웃 시스템이 너비를 계산할 때 호출
    public override void CalculateLayoutInputHorizontal()
    {
        base.CalculateLayoutInputHorizontal();
        float width = rectTransform.rect.width;
        float height = CalculateHeight(width);

        // 부모(Content)에게 내가 이만큼의 높이가 필요하다고 알림 (Content Size Fitter와 연동됨)
        SetLayoutInputForAxis(height, height, -1, 1);
    }

    public override void CalculateLayoutInputVertical() { }

    public override void SetLayoutHorizontal() => SetLayout();

    public override void SetLayoutVertical() => SetLayout();

    private void SetLayout()
    {
        float width = rectTransform.rect.width;
        CalculateHeight(width, true);
    }

    // 높이를 계산하고, layoutChildren이 true면 실제로 배치까지 수행
    private float CalculateHeight(float width, bool layoutChildren = false)
    {
        float workingWidth = width - padding.horizontal;
        float x = padding.left;
        float y = padding.top;
        float currentRowHeight = 0;

        for (int i = 0; i < rectChildren.Count; i++)
        {
            var child = rectChildren[i];

            // 자식의 선호 크기 가져오기
            float childW = LayoutUtility.GetPreferredSize(child, 0);
            float childH = LayoutUtility.GetPreferredSize(child, 1);

            // 줄바꿈 로직: 현재 줄에 더 들어갈 자리가 없으면 다음 줄로
            if (x + childW > width - padding.right && x > padding.left)
            {
                x = padding.left;
                y += currentRowHeight + spacingY;
                currentRowHeight = 0;
            }

            if (layoutChildren)
            {
                // 유니티 표준 함수로 자식 배치 (이게 있어야 구석에 안 박힘)
                SetChildAlongAxis(child, 0, x, childW);
                SetChildAlongAxis(child, 1, y, childH);
            }

            x += childW + spacingX;
            currentRowHeight = Mathf.Max(currentRowHeight, childH);
        }

        return y + currentRowHeight + padding.bottom;
    }
}