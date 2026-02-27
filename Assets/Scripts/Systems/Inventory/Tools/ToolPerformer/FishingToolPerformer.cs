using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class FishingToolPerformer : IToolPerformer
{
    public string GetDescription(ToolItemSO toolSO)
    {
        // 각 도구 아이템 패널에 띄울 설명글 return
        return "추가로 수확할 수 있는 개수 : " + toolSO.bonusCatchCount;
    }

    public void ChangeTool(ToolItemSO toolSO)
    {
        // 도구 변경되었을 때 씬에서 수행하는 일
        if (FishingManagerUI.Instance != null)
        {
            FishingManagerUI.Instance.bonusCatchCount = toolSO.bonusCatchCount;
            FishingManagerUI.Instance.FishingButton.image.sprite = toolSO.image;
        }

        // DB 저장은 UI에서 바로 하고 있어서, 씬 내에서 도구에 의해 변경되는 것만 조절해주시면 됨
    }

    public void ToolUIOn()
    {
        // 도구 UI 패널 켜졌을 때 씬에서 수행하는 일
        // ex) 낚시/채집 위해 켜둔 타이머 꺼짐
    }

    public void ToolUIOff()
    {
        // 도구 UI 패널 켜졌을 때 씬에서 수행하는 일
        // ex) ex) 낚시/채집 위해 켜둔 타이머 켜짐
    }
}
