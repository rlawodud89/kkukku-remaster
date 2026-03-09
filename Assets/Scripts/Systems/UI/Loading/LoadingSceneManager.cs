using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class LoadingSceneManager : MonoBehaviour
{
    // 정적 변수: 어떤 씬에서든 다음에 갈 곳을 지정할 수 있게 함
    public static string nextScene;

    [Header("UI Reference")]
    [SerializeField] private Slider progressBar;

    // 다른 씬에서 LoadingSceneManager.LoadScene("가게") 라고 호출하면 실행됨
    public static void LoadScene(string sceneName)
    {
        nextScene = sceneName;
        // 일단 로딩용 씬으로 이동 (이때 로딩 씬이 활성화되면서 Start()가 실행됨)
        SceneManager.LoadScene("Loading");
    }

    private void Start()
    {
        // 로딩 씬이 켜지자마자 백그라운드에서 진짜 목적지 씬 로드 시작
        StartCoroutine(LoadSceneProcess());
    }

    IEnumerator LoadSceneProcess()
    {
        AsyncOperation op = SceneManager.LoadSceneAsync(nextScene);
        op.allowSceneActivation = false;

        float timer = 0f;
        while (!op.isDone)
        {
            yield return null;
            timer += Time.unscaledDeltaTime;

            if (op.progress < 0.9f)
            {
                if (progressBar != null)
                    progressBar.value = Mathf.Lerp(progressBar.value, op.progress, timer);
            }
            else
            {
                // 로딩이 거의 다 된 경우 (0.9 ~ 1.0)
                if (progressBar != null)
                {
                    progressBar.value = Mathf.Lerp(progressBar.value, 1f, timer);
                    // 슬라이더가 다 찼을 때만 넘어감
                    if (progressBar.value >= 1f)
                    {
                        op.allowSceneActivation = true;
                    }
                }
                else
                {
                    // 중요!! 슬라이더를 연결 안 했다면 로딩 직후 바로 넘어감
                    op.allowSceneActivation = true;
                }
            }

            // 안전장치: 혹시 모르니 op가 완료 가능한 상태면 탈출
            if (op.allowSceneActivation && op.isDone) yield break;
        }
    }
}