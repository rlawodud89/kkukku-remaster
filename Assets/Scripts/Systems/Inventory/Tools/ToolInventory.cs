using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class ToolInventory : MonoBehaviour
{
    [Header("도구 선택 패널")]
    [SerializeField] private GameObject toolPanel;

    [Header("도구 스크롤뷰")]
    [SerializeField] private RectTransform scrollView;

    [Header("도구 스크롤뷰 내 Content")]
    [SerializeField] private RectTransform toolContent;

    [Header("도구 스크롤뷰에 들어갈 프리팹")]
    [SerializeField] private GameObject toolPrefab;

    private Dictionary<string, ToolItemPanel> toolContentDictionary = new Dictionary<string, ToolItemPanel>();
    //private ToolSO selectedTool;
    private ToolItemPanel selectedToolContent;

    void Start()
    {
        // 스크롤뷰에 아이템 추가할 때 사용하는 코드
        //GameObject tool = Instantiate(toolPrefab, toolContent);
        //ToolContent ui = tool.GetComponent<ToolContent>();
        //ui.SetTool();
        //toolContentDictionary.Add("", ui);

        // 현재 선택되어 있는 도구 데이터 받아서 설정 필요
        //selectedTool = GetToolSO();
        //selectedToolContent = toolContentDictionary[""];


        GameObject tool = Instantiate(toolPrefab, toolContent);
        ToolItemPanel ui = tool.GetComponent<ToolItemPanel>();
        ui.SetTool(this);

        GameObject tool2 = Instantiate(toolPrefab, toolContent);
        ToolItemPanel ui2 = tool2.GetComponent<ToolItemPanel>();
        ui2.SetTool(this);
        selectedToolContent = ui2;
        selectedToolContent.UsedOn();
    }

    /*public void SelectTool(ToolSO selectedTool, ToolContent selectedToolContent)
    {
        // 기존 선택 해제
        this.selectedToolContent.HighlightOff();

        // 새로운 선택 저장 및 표시
        this.selectedTool = selectedTool;
        this.selectedToolContent = selectedToolContent;
        selectedToolContent.HighlightOn();
    }*/

    public void SelectTool(ToolItemPanel selectedToolContent)
    {
        // 기존 선택 해제
        this.selectedToolContent.UsedOff();

        // 새로운 선택 저장 및 표시
        this.selectedToolContent = selectedToolContent;
        selectedToolContent.UsedOn();
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
