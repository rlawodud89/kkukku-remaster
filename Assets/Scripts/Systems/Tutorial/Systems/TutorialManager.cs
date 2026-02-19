using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    public List<TutorialStep> steps;

    private int currentStepIndex = -1;
    private TutorialStep currentStep;

    private bool isWaitingForInput = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void Start()
    {
        StartTutorial();
    }

    public void StartTutorial()
    {
        currentStepIndex = -1;
        NextStep();
    }

    public void NextStep()
    {
        currentStepIndex++;

        if (currentStepIndex >= steps.Count)
        {
            EndTutorial();
            return;
        }

        currentStep = steps[currentStepIndex];
        StartCoroutine(RunStep(currentStep));
    }

    private IEnumerator RunStep(TutorialStep step)
    {
        // Anchor 로드 대기
        yield return new WaitUntil(() =>
            AnchorRegistry.HasAnchor(step.targetAnchor));

        var anchor = AnchorRegistry.GetAnchor(step.targetAnchor);

        RectTransform targetRect = anchor.GetComponent<RectTransform>();

        HighlightSystem.Instance.Highlight(targetRect);

        // 대화 표시
        TutorialDialogue.Instance.ShowDialogue(step.dialogue);

        if (step.waitForClick)
        {
            isWaitingForInput = true;

            // Anchor에 클릭 이벤트 연결
            ButtonClickListener listener = anchor.GetComponent<ButtonClickListener>();
            listener.OnClicked += OnTargetClicked;
        }
        else if (step.autoProceed)
        {
            yield return new WaitForSeconds(1.5f);
            NextStep();
        }
    }

    private void OnTargetClicked()
    {
        if (!isWaitingForInput) return;

        isWaitingForInput = false;

        HighlightSystem.Instance.Clear();
        TutorialDialogue.Instance.HideDialogue();

        NextStep();
    }

    private void EndTutorial()
    {
        HighlightSystem.Instance.Clear();
        TutorialDialogue.Instance.HideDialogue();
        Debug.Log("Tutorial Finished");
    }
}
