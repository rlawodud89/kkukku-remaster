using System.Collections;
using System.Collections.Generic;
using Unity.VisualScripting;
using UnityEngine;
using UnityEngine.UIElements;

public class ToolInventoryUI : MonoBehaviour
{
    [SerializeField] private ToolType toolType;

    [Header("도구 선택 패널")]
    [SerializeField] private GameObject toolPanel;

    [Header("도구 스크롤뷰")]
    [SerializeField] private RectTransform scrollView;

    [Header("도구 스크롤뷰 내 Content")]
    [SerializeField] private RectTransform toolContent;

    [Header("도구 스크롤뷰에 들어갈 프리팹")]
    [SerializeField] private GameObject toolPrefab;

    private Dictionary<string, ToolItemPanel> toolPanelDictionary = new Dictionary<string, ToolItemPanel>();
    private ToolItemSO selectedTool;
    private ToolItemPanel selectedToolPanel;

    void Start()
    {
        var tools = ServiceLocator.Get<GameData>().Inventory.GetAllToolItems(toolType);
        foreach (var tool in tools)
        {
            GameObject gameObject = Instantiate(toolPrefab, toolContent);
            ToolItemPanel ui = gameObject.GetComponent<ToolItemPanel>();
            ui.SetTool(tool, this);
            toolPanelDictionary.Add(tool.itemName, ui);
        }

        selectedTool = ServiceLocator.Get<GameData>().User.GetCurrentUsedTool(toolType);
        selectedToolPanel = toolPanelDictionary[selectedTool.itemName];
        selectedToolPanel.UsedOn();
    }

    public void SelectTool(ToolItemSO selectedTool, ToolItemPanel selectedToolPanel)
    {
        // 기존 선택 해제
        this.selectedToolPanel.UsedOff();

        // 새로운 선택 저장 및 표시
        this.selectedTool = selectedTool;
        this.selectedToolPanel = selectedToolPanel;
        selectedToolPanel.UsedOn();

        ServiceLocator.Get<GameData>().User.SetCurrentUsedTool(toolType, selectedTool.itemName);

        if (GatheringManager.Instance != null)
        {
            GatheringManager.Instance.ChangeGatheringTool(selectedTool.needClickCount);
        }
    }

    public void OnClickToolBtn()
    {
        toolPanel.SetActive(true);

        // 수면정원인 경우, 채집 제한시간 타이머 멈춰둠
        if (GatheringManager.Instance != null)
        {
            GatheringManager.Instance.StopTimer();
        }
    }

    public void OnClickToolBackBtn()
    {
        toolPanel.SetActive(false);

        // 수면정원인 경우, 채집 제한시간 타이머 다시 시작
        if (GatheringManager.Instance != null)
        {
            GatheringManager.Instance.StartTimer();
        }
    }
}
