using UnityEngine;

[CreateAssetMenu(menuName = "TutorialStep")]
public class TutorialStep : ScriptableObject
{
    [Header("하이라이트할 Anchor의 ID")]
    public TutorialID highlightTarget;   // None이면 하이라이트 안 함

    [Header("설명 대화창")]
    [TextArea] public string dialogue;
    public bool showDialogue = true;

    [Header("다음 스텝으로 넘어가는 이벤트")]
    public TutorialID completeEvent;     // 어떤 이벤트를 기다릴지
    public bool autoProceed;                   // 자동 진행 여부
}