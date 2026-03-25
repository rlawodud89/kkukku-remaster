using UnityEngine;

public class TutorialSystemRoot : MonoBehaviour
{
    private static TutorialSystemRoot Instance;

    private void Awake()
    {
        // 이미 존재하면 중복 제거
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        // 루트 전체를 유지
        DontDestroyOnLoad(gameObject);
    }
}
