using UnityEngine;

[CreateAssetMenu(menuName = "Tutorial/Tutorial Step")]
public class TutorialStep : ScriptableObject
{
    [Header("Highlight")]
    public TutorialID highlightTarget;   // None이면 하이라이트 안 함

    [Header("Dialogue")]
    [TextArea] public string dialogue;
    public bool showDialogue = true;

    [Header("Completion Condition")]
    public TutorialID completeEvent;     // 어떤 이벤트를 기다릴지
    public bool autoProceed;                   // 자동 진행 여부
}