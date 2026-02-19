using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class LoadingSceneManager : MonoBehaviour
{
    // 이동할 목적지 씬의 이름을 저장할 static 변수
    public static string nextSceneName;

    void Start()
    {
        // 로딩 씬이 켜지자마자 백그라운드 로딩 시작
        StartCoroutine(LoadSceneRoutine());
    }

    // 다른 씬에서 넘어갈 때 부르는 함수
    public static void LoadScene(string sceneName)
    {
        nextSceneName = sceneName;
        SceneManager.LoadScene("LoadingScene"); // 일단 로딩씬으로 이동
    }

    IEnumerator LoadSceneRoutine()
    {
        // 💡 꿀팁: 기기가 너무 좋아서 0.1초만에 화면이 껌벅! 하고 넘어가는 걸 방지
        // 최소 1초 정도는 로딩 화면(일러스트나 팁 텍스트 등)을 보여주도록 대기합니다.
        yield return new WaitForSeconds(1.0f);

        // 1. 목적지 씬을 비동기(백그라운드)로 불러오기 시작
        AsyncOperation op = SceneManager.LoadSceneAsync(nextSceneName);

        // 2. 마음대로 씬을 넘기지 못하게 막음
        op.allowSceneActivation = false;

        // 3. 로딩이 끝날 때까지 대기
        while (!op.isDone)
        {
            // 유니티의 비동기 로딩 진행도는 0.9가 끝입니다 (나머지 0.1은 씬 전환에 씀)
            if (op.progress >= 0.9f)
            {
                // 씬 로드가 100% 다 됐으면 화면 전환 허락!
                op.allowSceneActivation = true;
                yield break;
            }

            yield return null;
        }
    }
}

/*
 File -> Build Settings에 모든 씬을 다 넣어줍니다.

가게 씬으로 넘어갈 때 원래 코드 대신 **LoadingSceneManager.LoadScene("가게씬이름");**을 써주면 끝!
 
 */