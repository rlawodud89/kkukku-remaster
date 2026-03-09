using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;
using UnityEngine.EventSystems;

public class PanelItemImg : MonoBehaviour,
    IPointerDownHandler,
    IPointerUpHandler,
    IPointerExitHandler,
    IBeginDragHandler
{
    public Image itemImg;

    [Header("설명글 누르기 시간 설정")]
    public float longPressTime = 0.5f;

    private DescriptionPanel descriptionPanel;
    private string description;
    private Coroutine pressCoroutine;
    private bool isPressed;

    public void SetItemImg(Sprite itemSprite, string description, DescriptionPanel descriptionPanel)
    {
        itemImg.sprite = itemSprite;
        this.description = description;
        this.descriptionPanel = descriptionPanel;
    }

    // === 설명글 패널 관련 메서드 ===

    public void OnPointerDown(PointerEventData eventData)
    {
        Debug.Log($"{name} DOWN at {eventData.position}");
        isPressed = true;
        pressCoroutine = StartCoroutine(LongPressRoutine());
    }

    public void OnPointerUp(PointerEventData eventData)
    {
        Debug.Log($"{name} UP");
        CancelPress();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        Debug.Log($"{name} EXIT");

        RectTransform rect = transform as RectTransform;

        if (RectTransformUtility.RectangleContainsScreenPoint(
            rect, eventData.position, eventData.pressEventCamera))
        {
            // 실제로는 아직 안 벗어남 → 무시
            return;
        }

        CancelPress();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        Debug.Log($"{name} DRAG");
        CancelPress();
    }


    private IEnumerator LongPressRoutine()
    {
        yield return new WaitForSeconds(longPressTime);

        if (!isPressed) yield break;

        // 이미 다른 버튼이 점유 중이면 실패
        if (!DescriptionLock.TryAcquire(this))
            yield break;

        descriptionPanel.Show(description, transform as RectTransform);
        TutorialEventBus.Raise(TutorialID.StoreDescription);
    }


    private void CancelPress()
    {
        isPressed = false;

        if (pressCoroutine != null)
        {
            StopCoroutine(pressCoroutine);
            pressCoroutine = null;
        }

        if (DescriptionLock.IsOwner(this))
        {
            descriptionPanel.Hide();
            DescriptionLock.Release(this);
        }
    }
}
