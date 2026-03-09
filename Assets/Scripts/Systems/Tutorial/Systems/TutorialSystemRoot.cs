using UnityEngine;

public class TutorialSystemRoot : MonoBehaviour
{
    private static TutorialSystemRoot instance;

    private void Awake()
    {
        // 이미 존재하면 중복 제거
        if (instance != null && instance != this)
        {
            Destroy(gameObject);
            return;
        }

        instance = this;

        // 루트 전체를 유지
        DontDestroyOnLoad(gameObject);
    }
}
