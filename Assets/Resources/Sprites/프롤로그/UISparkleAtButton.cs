using UnityEngine;
using UnityEngine.UI;

public class UISparkleAtButton : MonoBehaviour
{
    public GameObject sparklePrefab;  // 스파클 프리팹
    public RectTransform target;      // 이 버튼의 RectTransform
    public Camera uiCamera;           // Canvas에 물린 카메라

    public void Fire()
    {
        if (!sparklePrefab || !target || !uiCamera) return;

        // Screen Space - Camera 기준: RectTransform의 world pos 사용
        Vector3 worldPos = target.position;
        var go = Instantiate(sparklePrefab, worldPos, Quaternion.identity);

        // UI 위로 정렬
        var psr = go.GetComponent<ParticleSystemRenderer>();
        if (psr)
        {
            psr.sortingLayerName = "UI";
            psr.sortingOrder = 500;
        }

        // ★ 핵심: SparklePreset2D가 있으면 Burst() 호출 (팔레트/단색 모두 커버)
        var preset = go.GetComponent<SparklePreset2D>();
        var ps = go.GetComponent<ParticleSystem>();

        if (preset != null)
        {
            preset.Burst(); // 팔레트 ON이면 수동 Emit, OFF면 내부적으로 Play만 함
            // ttl 계산은 ParticleSystem에서 가져오자
            if (ps) Destroy(go, ps.main.duration + ps.main.startLifetime.constantMax + 0.05f);
            else Destroy(go, 1.5f);
        }
        else if (ps)
        {
            // 예전 프리팹처럼 모듈만 있는 경우
            ps.Play();
            Destroy(go, ps.main.duration + ps.main.startLifetime.constantMax + 0.05f);
        }
        else
        {
            Destroy(go, 1.5f);
        }
    }
}
