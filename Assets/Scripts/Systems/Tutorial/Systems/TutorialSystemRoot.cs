using UnityEngine;

public class TutorialSystemRoot : MonoBehaviour
{
    private static TutorialSystemRoot Instance;

    private void Awake()
    {
        Debug.Log($"[TutorialSystemRoot] Awake in {gameObject.scene.name}");

        if (Instance != null && Instance != this)
        {
            Debug.Log("[TutorialSystemRoot] Duplicate → Destroy");
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            Instance = null;
        }
    }
}