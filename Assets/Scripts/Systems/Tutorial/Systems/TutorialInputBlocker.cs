using UnityEngine;

public class TutorialInputBlocker : MonoBehaviour
{
    public static TutorialInputBlocker Instance;

    private void Awake()
    {
        Instance = this;
        gameObject.SetActive(false);
    }

    public void SetBlock(bool block)
    {
        gameObject.SetActive(block);
    }
}