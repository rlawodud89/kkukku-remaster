using UnityEngine;
using TMPro; // TextMeshPro를 사용하기 위해 필요해요
using System.Collections;

public class LoadingTextEffect : MonoBehaviour
{
    public TextMeshProUGUI loadingText; // 연결할 텍스트 UI
    public float speed = 0.5f; // 점이 찍히는 속도

    void OnEnable()
    {
        // 오브젝트가 활성화될 때 코루틴 시작
        StartCoroutine(AnimateText());
    }

    IEnumerator AnimateText()
    {
        while (true)
        {
            loadingText.text = "Loading";
            yield return new WaitForSeconds(speed);

            loadingText.text = "Loading.";
            yield return new WaitForSeconds(speed);

            loadingText.text = "Loading..";
            yield return new WaitForSeconds(speed);

            loadingText.text = "Loading...";
            yield return new WaitForSeconds(speed);
        }
    }
}