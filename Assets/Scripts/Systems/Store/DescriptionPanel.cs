using System.Collections;
using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class DescriptionPanel : MonoBehaviour
{
    [SerializeField] private RectTransform panel;
    [SerializeField] private TMP_Text descriptionText;

    private void Awake()
    {
        Hide();
    }

    public void Show(string message, RectTransform target)
    {
        descriptionText.text = message;

        // 버튼 위에 위치시키기
        Vector3[] corners = new Vector3[4];
        target.GetWorldCorners(corners);

        Vector3 topCenter = (corners[1] + corners[2]) * 0.5f;
        panel.position = topCenter + Vector3.up * 10f;

        panel.gameObject.SetActive(true);
    }

    public void Hide()
    {
        panel.gameObject.SetActive(false);
    }
}
