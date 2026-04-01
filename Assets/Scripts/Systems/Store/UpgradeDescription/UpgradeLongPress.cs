using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;
using System.Collections;

public class UpgradeLongPress : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler,
    IBeginDragHandler
{
    public Button targetButton;
    public float longPressTime = 0.5f;
    public GameObject descriptionPanel;

    private bool isPressed;
    private bool longPressTriggered;
    private Coroutine pressCoroutine;

    public void OnPointerDown(PointerEventData eventData)
    {
        isPressed = true;
        longPressTriggered = false;

        // Button에게 눌림 전달
        ExecuteEvents.Execute(
            targetButton.gameObject,
            eventData,
            ExecuteEvents.pointerDownHandler
        );

        pressCoroutine = StartCoroutine(LongPressRoutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        // Button에게 떼기 전달
        ExecuteEvents.Execute(
            targetButton.gameObject,
            eventData,
            ExecuteEvents.pointerUpHandler
        );

        if (!longPressTriggered)
        {
            targetButton.onClick.Invoke();
        }

        CancelPress();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        RectTransform rect = transform as RectTransform;

        if (RectTransformUtility.RectangleContainsScreenPoint(
            rect, eventData.position, eventData.pressEventCamera))
        {
            // 실제로는 아직 안 벗어남 → 무시
            return;
        }

        ExecuteEvents.Execute(
            targetButton.gameObject,
            eventData,
            ExecuteEvents.pointerExitHandler
        );

        CancelPress();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        CancelPress();
    }

    private IEnumerator LongPressRoutine()
    {
        yield return new WaitForSeconds(longPressTime);

        if (!isPressed) yield break;

        longPressTriggered = true;
        descriptionPanel.SetActive(true);
        TutorialEventBus.Raise(TutorialID.MaterialLevelUpgradeDescription);
    }

    private void CancelPress()
    {
        isPressed = false;

        if (pressCoroutine != null)
        {
            StopCoroutine(pressCoroutine);
            pressCoroutine = null;
        }

        descriptionPanel.SetActive(false);
    }
}
