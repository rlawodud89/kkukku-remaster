using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GatheringToolPerformer : IToolPerformer
{
    public string GetDescription(ToolItemSO toolSO)
    {
        return $"클릭 필요 횟수: {toolSO.needClickCount}번";
    }

    public void ChangeTool(ToolItemSO selectedtool)
    {
        if (GatheringManager.Instance != null)
        {
            GatheringManager.Instance.ChangeGatheringTool(selectedtool.needClickCount);
        }
    }

    public void ToolUIOn()
    {
        // 채집 제한시간 타이머 멈춰둠

        if (GatheringManager.Instance != null)
        {
            GatheringManager.Instance.StopTimer();
        }
    }

    public void ToolUIOff()
    {
        // 채집 제한시간 타이머 다시 시작

        if (GatheringManager.Instance != null)
        {
            GatheringManager.Instance.StartTimer();
        }
    }
}
