using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class AudioManager : MonoBehaviour
{
    public static AudioManager Instance { get; private set; }

    [System.Serializable]
    public class SceneMusic
    {
        public string sceneName;
        public AudioClip musicClip;
    }

    [Header("Nguồn phát nhạc")]
    [SerializeField] private AudioSource musicSource;

    [Header("Nhạc theo từng Scene")]
    [SerializeField] private SceneMusic[] sceneMusics;

    [Header("Âm lượng nhạc nền")]
    [Range(0f, 1f)]
    [SerializeField] private float musicVolume = 0.45f;

    [Header("Thời gian chuyển nhạc")]
    [SerializeField] private float fadeDuration = 1f;

    private AudioClip currentMusic;
    private Coroutine fadeCoroutine;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SetupMusicSource();

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        PlayMusicForScene(SceneManager.GetActiveScene().name);
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        PlayMusicForScene(scene.name);
    }

    private void SetupMusicSource()
    {
        if (musicSource == null)
        {
            musicSource = gameObject.AddComponent<AudioSource>();
        }

        musicSource.playOnAwake = false;
        musicSource.loop = true;
        musicSource.spatialBlend = 0f;
        musicSource.volume = musicVolume;
    }

    private void PlayMusicForScene(string sceneName)
    {
        AudioClip targetMusic = GetMusicForScene(sceneName);

        if (targetMusic == null)
        {
            Debug.Log("Scene chưa có nhạc nền: " + sceneName);
            return;
        }

        if (currentMusic == targetMusic)
            return;

        currentMusic = targetMusic;

        if (fadeCoroutine != null)
        {
            StopCoroutine(fadeCoroutine);
        }

        fadeCoroutine = StartCoroutine(FadeToMusic(targetMusic));
    }

    private AudioClip GetMusicForScene(string sceneName)
    {
        if (sceneMusics == null)
            return null;

        for (int i = 0; i < sceneMusics.Length; i++)
        {
            if (sceneMusics[i] == null)
                continue;

            if (sceneMusics[i].sceneName == sceneName)
            {
                return sceneMusics[i].musicClip;
            }
        }

        return null;
    }

    private IEnumerator FadeToMusic(AudioClip newMusic)
    {
        if (musicSource.isPlaying)
        {
            float startVolume = musicSource.volume;
            float timer = 0f;

            while (timer < fadeDuration)
            {
                timer += Time.deltaTime;
                musicSource.volume = Mathf.Lerp(startVolume, 0f, timer / fadeDuration);
                yield return null;
            }
        }

        musicSource.Stop();
        musicSource.clip = newMusic;
        musicSource.volume = 0f;
        musicSource.Play();

        float fadeInTimer = 0f;

        while (fadeInTimer < fadeDuration)
        {
            fadeInTimer += Time.deltaTime;
            musicSource.volume = Mathf.Lerp(0f, musicVolume, fadeInTimer / fadeDuration);
            yield return null;
        }

        musicSource.volume = musicVolume;
        fadeCoroutine = null;
    }

    public void SetMusicVolume(float volume)
    {
        musicVolume = Mathf.Clamp01(volume);

        if (musicSource != null)
        {
            musicSource.volume = musicVolume;
        }
    }

    public void StopMusic()
    {
        if (musicSource == null)
            return;

        musicSource.Stop();
        currentMusic = null;
    }
}