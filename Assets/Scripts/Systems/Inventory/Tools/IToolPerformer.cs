using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public interface IToolPerformer
{
    public string GetDescription(ToolItemSO toolSO);
    public void ChangeTool(ToolItemSO toolSO);
    public void ToolUIOn();
    public void ToolUIOff();
}
