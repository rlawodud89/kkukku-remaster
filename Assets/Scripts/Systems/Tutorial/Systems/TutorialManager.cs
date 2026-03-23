using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class TutorialManager : MonoBehaviour
{
    public static TutorialManager Instance;

    public List<TutorialStep> steps;

    private int currentStepIndex = -1;
    private TutorialStep currentStep;

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

    private void OnEnable()
    {
        TutorialEventBus.Subscribe(HandleEvent);
    }

    private void OnDisable()
    {
        TutorialEventBus.Unsubscribe(HandleEvent);
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
        // 대화창만 있는 경우(DialogueNext), Mask 대신 따로 입력 차단
        bool shouldBlockInput =
            step.completeEvent == TutorialID.DialogueNext;

        TutorialInputBlocker.Instance.SetBlock(shouldBlockInput);


        // 하이라이트 처리
        if (step.highlightTarget != TutorialID.None)
        {
            yield return new WaitUntil(() =>
                AnchorRegistry.HasAnchor(step.highlightTarget));

            var anchor = AnchorRegistry.GetAnchor(step.highlightTarget);
            RectTransform targetRect = anchor.GetComponent<RectTransform>();

            HighlightSystem.Instance.Highlight(targetRect);
        }
        else
        {
            HighlightSystem.Instance.Clear();
        }


        // 대사 처리
        if (step.showDialogue)
            TutorialDialogue.Instance.ShowDialogue(step.dialogue);
        else
            TutorialDialogue.Instance.HideDialogue();


        // 자동 진행
        if (step.autoProceed)
        {
            yield return new WaitForSeconds(3.0f);
            NextStep();
        }
    }

    private void HandleEvent(TutorialID eventID)
    {
        if (currentStep == null)
            return;

        if (currentStep.completeEvent != eventID)
            return;

        HighlightSystem.Instance.Clear();
        TutorialDialogue.Instance.HideDialogue();
        TutorialInputBlocker.Instance.SetBlock(false);

        NextStep();
    }

    private void EndTutorial()
    {
        HighlightSystem.Instance.Clear();
        TutorialDialogue.Instance.HideDialogue();

        ServiceLocator.Get<GameData>().User.SetStartState(StartStateType.GAME);
        ServiceLocator.Get<SaveService>().SaveNow();
        ServiceLocator.Get<SaveService>().SetAutoSave(true);

        Debug.Log("Tutorial Finished");
    }
}