using UnityEngine;

public class TutorialSystemRoot : MonoBehaviour
{
    private static TutorialSystemRoot Instance;

    private void Awake()
    {
        Debug.Log($"[TutorialSystemRoot] Awake: {gameObject.GetInstanceID()} in {gameObject.scene.name}");

        if (Instance != null && Instance != this)
        {
            Debug.Log($"[TutorialSystemRoot] Duplicate Destroy: {gameObject.GetInstanceID()}");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        Debug.Log($"[TutorialSystemRoot] Destroyed: {gameObject.GetInstanceID()}");

        if (Instance == this)
            Instance = null;
    }
}