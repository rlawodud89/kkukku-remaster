using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UIElements;

public class TutorialLoader : MonoBehaviour
{
    public static TutorialLoader Instance; // 중복 방지용

    [Header("껐다 켤 튜토리얼 시스템 부모 객체")]
    [SerializeField] private GameObject tutorialSystemRoot;

    private void Awake()
    {
        // 씬이 이동해도 파괴되지 않게 막아주는 유저님의 아이디어 적용!
        if (Instance == null)
        {
            Instance = this;
            DontDestroyOnLoad(gameObject); // 나 자신을 파괴하지 마라!
        }
        else
        {
            // 씬을 이동했는데 이미 똑같은 수신기가 있다면? 나는 미련 없이 파괴! (중복 방지)
            Destroy(gameObject);
        }

        TutorialInvisible();
    }

    public void TutorialStart()
    {
        if (ServiceLocator.Get<GameData>().User.GetStartState() == StartStateType.TUTORIAL)
            tutorialSystemRoot.gameObject.SetActive(true);
    }

    public void TutorialInvisible()
    {
        tutorialSystemRoot.gameObject.SetActive(false);
    }
}
