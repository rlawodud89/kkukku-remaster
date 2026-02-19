using UnityEngine;

[CreateAssetMenu(menuName = "TutorialStep")]
public class TutorialStep : ScriptableObject
{
    public TutorialAnchorID targetAnchor;
    [TextArea] public string dialogue;

    public bool waitForClick = true;
    public bool autoProceed = false;
}
