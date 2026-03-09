using UnityEngine;
using TMPro;

public class TutorialDialogue : MonoBehaviour
{
    public static TutorialDialogue Instance;

    [SerializeField] private GameObject dialoguePanel;
    [SerializeField] private TMP_Text dialogueText;

    private void Awake()
    {
        Instance = this;
        dialoguePanel.SetActive(false);
    }

    public void ShowDialogue(string message)
    {
        dialoguePanel.SetActive(true);
        dialogueText.text = message;
    }

    public void HideDialogue()
    {
        dialoguePanel.SetActive(false);

    }

    public void OnClickCloseButton()
    {
        dialoguePanel.SetActive(false);
        TutorialEventBus.Raise(TutorialID.DialogueNext);
    }
}
