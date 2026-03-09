// SparklePreset2D.cs
// 2D 스파클 버스트: 파스텔 팔레트(입자별 랜덤) 지원
using UnityEngine;

[ExecuteAlways]
public class SparklePreset2D : MonoBehaviour
{
    public enum ColorTheme { WarmGold, SoftSky, Custom }

    [Header("버스트 기본")]
    public bool burstMode = true;
    public Vector2Int burstCountRange = new Vector2Int(28, 40);
    public float duration = 0.6f;
    public float radius = 0.08f;

    [Header("초기 속도/크기/수명")]
    public float startSpeedMin = 1.6f;
    public float startSpeedMax = 2.6f;
    public float startSizeMin = 0.06f;
    public float startSizeMax = 0.14f;
    public float lifetimeMin = 0.22f;
    public float lifetimeMax = 0.45f;

    [Header("감속")]
    public float speedLimit = 1.0f;
    [Range(0f, 1f)] public float dampen = 0.7f;

    [Header("색상 테마(팔레트 미사용 시)")]
    public ColorTheme colorTheme = ColorTheme.WarmGold;
    public Color startColor = new Color(1f, 0.97f, 0.75f, 1f);
    public Gradient colorOverLife;

    [Header("트레일/노이즈")]
    public bool enableTrails = true;
    public float trailLifetime = 0.12f;
    [Range(0f, 1f)] public float trailRatio = 0.35f;
    public bool enableNoise = true;
    public float noiseStrength = 0.25f;
    public float noiseFrequency = 0.6f;

    [Header("정렬/머티리얼")]
    public string sortingLayerName = "Default";
    public int sortingOrder = 10;
    public string materialName = "Sparkle"; // Resources/Sparkle.mat 권장

    [Header("연속 모드 옵션")]
    public float rateOverTime = 0f;

    [Header("스케일 조절")]
    [Range(0.2f, 3f)] public float sizeMultiplier = 1.35f;
    [Range(0.2f, 3f)] public float rangeMultiplier = 1.4f;

    // ★ 파스텔 팔레트 (입자별 랜덤 색)
    [Header("파스텔 팔레트(여러 색 랜덤)")]
    public bool usePastelPalette = true;
    public Color[] pastelPalette;                  // 비워두면 Reset에서 기본 5색 채움
    [Range(0f, 0.2f)] public float pastelHueJitter = 0.06f;
    [Range(0f, 0.3f)] public float pastelValueJitter = 0.10f;

    ParticleSystem ps;

    // 내부 캐시(버스트 카운트 보정)
    int cachedMinBurst, cachedMaxBurst;

    // ===== Unity Hooks =====
    void OnEnable()
    {
        if (ps == null) Build();
        Apply();
    }

    void Reset()
    {
        colorTheme = ColorTheme.WarmGold;
        colorOverLife = MakeGoldGradient();
        startColor = new Color(1f, 0.97f, 0.75f, 1f);

        // 기본 파스텔 팔레트(레몬, 피치, 라벤더, 민트, 스카이)
        pastelPalette = new[]
        {
            new Color(1.00f, 0.98f, 0.75f), // lemon
            new Color(1.00f, 0.85f, 0.80f), // peach
            new Color(0.90f, 0.86f, 1.00f), // lavender
            new Color(0.82f, 1.00f, 0.90f), // mint
            new Color(0.85f, 0.95f, 1.00f), // sky
        };
    }

    void Build()
    {
        ps = GetComponent<ParticleSystem>();
        if (ps == null) ps = gameObject.AddComponent<ParticleSystem>();

        var pr = GetComponent<ParticleSystemRenderer>();
        pr.renderMode = ParticleSystemRenderMode.Billboard;
        pr.sortingLayerName = sortingLayerName;
        pr.sortingOrder = sortingOrder;

        // Sparkle 머티리얼 로드
        Material src = !string.IsNullOrEmpty(materialName) ? Resources.Load<Material>(materialName) : null;
        if (src == null)
        {
            var shader = Shader.Find("Particles/Additive");
            if (shader == null) shader = Shader.Find("Sprites/Default");
            if (shader != null) src = new Material(shader) { name = "Sparkle (Auto)" };
        }

        if (src != null)
        {
            if (Application.isPlaying)
            {
                var inst = new Material(src);
                NormalizeMaterialColor(inst);
                GetComponent<ParticleSystemRenderer>().material = inst;
                GetComponent<ParticleSystemRenderer>().trailMaterial = inst;
            }
            else
            {
                GetComponent<ParticleSystemRenderer>().sharedMaterial = src;
                GetComponent<ParticleSystemRenderer>().trailMaterial = src;
            }
        }
    }

    void Apply()
    {
        if (ps == null) return;

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Clear();

        // 스케일
        float sMin = startSizeMin * sizeMultiplier;
        float sMax = startSizeMax * sizeMultiplier;
        float vMin = startSpeedMin * rangeMultiplier;
        float vMax = startSpeedMax * rangeMultiplier;
        float r = radius * rangeMultiplier;
        float limit = speedLimit * rangeMultiplier;

        // Main
        var main = ps.main;
        main.playOnAwake = false;
        main.duration = Mathf.Max(0.1f, duration);
        main.loop = !burstMode;
        main.stopAction = ParticleSystemStopAction.Disable;
        main.simulationSpace = ParticleSystemSimulationSpace.World;
        main.startLifetime = new ParticleSystem.MinMaxCurve(lifetimeMin, lifetimeMax);
        main.startSpeed = new ParticleSystem.MinMaxCurve(vMin, vMax);
        main.startSize = new ParticleSystem.MinMaxCurve(sMin, sMax);

        // 팔레트 사용 시, 시작색은 흰색으로 두고(팔레트가 입자별로 칠함),
        // ColorOverLifetime은 "알파 페이드"만 수행(색은 보존).
        if (usePastelPalette)
        {
            main.startColor = Color.white;
        }
        else
        {
            main.startColor = (colorTheme == ColorTheme.WarmGold) ? new Color(1f, 0.97f, 0.75f, 1f)
                           : (colorTheme == ColorTheme.SoftSky) ? new Color(0.85f, 0.95f, 1f, 1f)
                           : startColor;
        }

        // Emission
        var emission = ps.emission;
        emission.enabled = true;

        float densityScale = Mathf.Lerp(1f, rangeMultiplier, 0.5f);
        cachedMinBurst = Mathf.Max(1, Mathf.RoundToInt(burstCountRange.x * densityScale));
        cachedMaxBurst = Mathf.Max(cachedMinBurst, Mathf.RoundToInt(burstCountRange.y * densityScale));

        if (burstMode && !usePastelPalette)
        {
            // 기존(단색/테마) 버스트는 엔진 버스트 기능 사용
            emission.rateOverTime = 0f;
            var burst = new ParticleSystem.Burst(0f, (short)cachedMinBurst, (short)cachedMaxBurst, 1, 0.01f);
            emission.SetBursts(new[] { burst });
        }
        else
        {
            // 팔레트 사용 시: 엔진 버스트는 비활성, Burst()에서 수동 Emit
            emission.rateOverTime = burstMode ? 0f : rateOverTime;
            emission.SetBursts(System.Array.Empty<ParticleSystem.Burst>());
        }

        // Shape
        var shape = ps.shape;
        shape.enabled = true;
        shape.shapeType = ParticleSystemShapeType.Sphere;
        shape.radius = r;
        shape.radiusThickness = 0f;

        // Velocity / Limit
        var vel = ps.velocityOverLifetime;
        vel.enabled = true;
        vel.space = ParticleSystemSimulationSpace.Local;
        vel.radial = new ParticleSystem.MinMaxCurve(0.2f, 0.6f);
        vel.orbitalZ = 0.1f;

        var lim = ps.limitVelocityOverLifetime;
        lim.enabled = true;
        lim.separateAxes = false;
        lim.limit = limit;
        lim.dampen = dampen;

        // Size over Lifetime
        var size = ps.sizeOverLifetime;
        size.enabled = true;
        size.size = new ParticleSystem.MinMaxCurve(1f, new AnimationCurve(
            new Keyframe(0f, 1.25f, 0f, -5f),
            new Keyframe(0.07f, 1.0f),
            new Keyframe(0.35f, 0.5f),
            new Keyframe(1f, 0f)
        ));

        // Color over Lifetime
        var col = ps.colorOverLifetime;
        col.enabled = true;
        col.color = new ParticleSystem.MinMaxGradient(
            usePastelPalette ? MakeAlphaFadeGradient()  // 색 유지 + 알파만 페이드
                             : (colorTheme == ColorTheme.WarmGold ? MakeGoldGradient()
                               : colorTheme == ColorTheme.SoftSky ? MakeSkyGradient()
                               : (colorOverLife ?? MakeGoldGradient()))
        );

        // Trails / Noise
        var trails = ps.trails; trails.enabled = enableTrails;
        if (enableTrails) { trails.lifetime = trailLifetime; trails.ratio = trailRatio; trails.dieWithParticles = true; }
        var noise = ps.noise; noise.enabled = enableNoise;
        if (enableNoise) { noise.strength = noiseStrength; noise.frequency = noiseFrequency; noise.scrollSpeed = 0f; }

        // 정렬
        var pr = GetComponent<ParticleSystemRenderer>();
        pr.sortingLayerName = sortingLayerName;
        pr.sortingOrder = sortingOrder;

        if (!Application.isPlaying) { ps.Simulate(0f, true, true); ps.Play(); }
        else { ps.Play(); }
    }

    // === 외부 호출: 버스트 ===
    public void Burst()
    {
        if (ps == null) return;

        ps.Stop(true, ParticleSystemStopBehavior.StopEmittingAndClear);
        ps.Clear();
        ps.Play(true);

        if (!burstMode)
            return;

        if (usePastelPalette)
        {
            // 수동 Emit: 파티클마다 팔레트에서 색을 뽑아 지정
            int count = Random.Range(cachedMinBurst, cachedMaxBurst + 1);
            for (int i = 0; i < count; i++)
            {
                var emit = new ParticleSystem.EmitParams();
                emit.startColor = PickPastel();
                // 크기/속도/수명은 Main의 MinMaxCurve에 맡김
                ps.Emit(emit, 1);
            }
        }
        // 팔레트 미사용이면 Apply()에서 세팅한 엔진 Burst가 자동으로 처리됨
    }

    public void Refresh() => Apply();

    // ===== Helpers =====
    static readonly int ID_Color = Shader.PropertyToID("_Color");
    static readonly int ID_BaseColor = Shader.PropertyToID("_BaseColor");
    static readonly int ID_TintColor = Shader.PropertyToID("_TintColor");
    static readonly int ID_EmissionCol = Shader.PropertyToID("_EmissionColor");

    void NormalizeMaterialColor(Material m)
    {
        if (!m) return;
        if (m.HasProperty(ID_Color)) m.SetColor(ID_Color, Color.white);
        if (m.HasProperty(ID_BaseColor)) m.SetColor(ID_BaseColor, Color.white);
        if (m.HasProperty(ID_TintColor)) m.SetColor(ID_TintColor, Color.white);
        if (m.HasProperty(ID_EmissionCol)) m.SetColor(ID_EmissionCol, Color.black);
    }

    Gradient MakeGoldGradient()
    {
        var g = new Gradient();
        g.SetKeys(
            new[] {
                new GradientColorKey(new Color(1f, 0.96f, 0.70f), 0f),
                new GradientColorKey(new Color(1f, 0.99f, 0.88f), 0.15f),
                new GradientColorKey(Color.white, 0.35f),
                new GradientColorKey(Color.white, 1f)
            },
            new[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.8f, 0.15f),
                new GradientAlphaKey(0.35f, 0.6f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        return g;
    }

    Gradient MakeSkyGradient()
    {
        var g = new Gradient();
        g.SetKeys(
            new[] {
                new GradientColorKey(new Color(0.84f, 0.94f, 1f), 0f),
                new GradientColorKey(new Color(0.90f, 0.97f, 1f), 0.2f),
                new GradientColorKey(Color.white, 0.4f),
                new GradientColorKey(Color.white, 1f)
            },
            new[] {
                new GradientAlphaKey(1f, 0f),
                new GradientAlphaKey(0.75f, 0.2f),
                new GradientAlphaKey(0.3f, 0.6f),
                new GradientAlphaKey(0f, 1f)
            }
        );
        return g;
    }

    // 색은 유지하고, 알파만 페이드되는 그라데이션(팔레트 모드에서 사용)
    Gradient MakeAlphaFadeGradient()
    {
        var g = new Gradient();
        g.SetKeys(
            new[] { new GradientColorKey(Color.white, 0f), new GradientColorKey(Color.white, 1f) },
            new[] { new GradientAlphaKey(1f, 0f), new GradientAlphaKey(0f, 1f) }
        );
        return g;
    }

    // 팔레트에서 하나 뽑아 파스텔 톤으로 살짝 흔들기
    Color PickPastel()
    {
        if (pastelPalette == null || pastelPalette.Length == 0)
            return Jitter(new Color(0.9f, 0.95f, 1f), pastelHueJitter, pastelValueJitter); // sky 계열

        var baseC = pastelPalette[Random.Range(0, pastelPalette.Length)];
        return Jitter(baseC, pastelHueJitter, pastelValueJitter);
    }

    Color Jitter(Color c, float hNoise, float vNoise)
    {
        Color.RGBToHSV(c, out float h, out float s, out float v);
        // 파스텔 유지: 낮은 채도/높은 명도로 클램프
        h += Random.Range(-hNoise, hNoise);
        v += Random.Range(-vNoise, vNoise);
        s = Mathf.Clamp01(s * 0.8f);   // 채도 조금 낮춤
        v = Mathf.Clamp01(v);          // 명도 유지
        return Color.HSVToRGB(Mathf.Repeat(h, 1f), s, v);
    }
}
