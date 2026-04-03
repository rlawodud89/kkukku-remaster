using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    private static AudioManager _instance;

    public UnityEngine.Audio.AudioMixer audioMixer;
    [SerializeField] public AudioSource bgmAudioSource;
    [SerializeField] private AudioSource sfxAudioSource;

    public static AudioManager Instance
    {
        get
        {
            // 씬에 생성된 싱글톤이 없으면 자동 생성
            if (_instance == null)
            {
                var obj = new GameObject("AudioManager");
                _instance = obj.AddComponent<AudioManager>();
                DontDestroyOnLoad(obj);
            }
            return _instance;
        }
    }

    protected virtual void Awake()
    {
        if (_instance == null)
        {
            _instance = this as AudioManager;
            DontDestroyOnLoad(gameObject);
        }
        else
        {
            Destroy(gameObject);
        }


        // AudioSource 준비
        if (bgmAudioSource == null)
        {
            bgmAudioSource = transform.GetChild(0).GetComponent<AudioSource>();
            bgmAudioSource.loop = true;
            bgmAudioSource.playOnAwake = false;
        }


        // audioMixer 할당
        if (audioMixer == null)
        {
            var grp = bgmAudioSource.outputAudioMixerGroup;
            audioMixer = grp.audioMixer;
        }
    }

    void Start()
    {
        // BGM 시작
        //audioMixer.SetFloat("BGM", Mathf.Log10(gameManager.Get_BgSound()) * 20);
        //bgmAudioSource.Play();
        //Debug.Log("BGM Volume: " + bgmAudioSource.volume);

        //audioMixer.SetFloat("SFX", Mathf.Log10(gameManager.Get_EffectSound()) * 20);

        float savedBGM = PlayerPrefs.GetFloat("BGM_Volume", 0.75f);
        float savedSFX = PlayerPrefs.GetFloat("SFX_Volume", 0.75f);

        // 0 레벨 에러 방지용 안전장치
        float safeBGM = Mathf.Max(0.0001f, savedBGM);
        float safeSFX = Mathf.Max(0.0001f, savedSFX);

        // 믹서에 즉시 적용! (설정창을 안 켜도 여기서 무조건 한 번 맞춰줍니다)
        if (audioMixer != null)
        {
            audioMixer.SetFloat("BGM", Mathf.Log10(safeBGM) * 20);
            audioMixer.SetFloat("SFX", Mathf.Log10(safeSFX) * 20);
        }
    }

    private void OnEnable()
    {
        // 씬 로드 이벤트 연결
        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void OnDisable()
    {
        // 이벤트 연결 해제
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (scene.name == "Boot" || scene.name == "Prolog" || scene.name == "Start" || scene.name == "Ending")
        {
            bgmAudioSource.Stop();
        }
        else
        {
            if (!bgmAudioSource.isPlaying)
            {
                bgmAudioSource.Play();
            }
        }
    }
}
