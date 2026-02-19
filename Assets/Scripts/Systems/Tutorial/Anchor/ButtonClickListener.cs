using UnityEngine;
using UnityEngine.UI;
using System;

[RequireComponent(typeof(Button))]
public class ButtonClickListener : MonoBehaviour
{
    public event Action OnClicked;

    private void Awake()
    {
        GetComponent<Button>().onClick.AddListener(() =>
        {
            OnClicked?.Invoke();
        });
    }
}