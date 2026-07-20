using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.UI;

public class UIButtonSFXManager : MonoBehaviour
{
    public static UIButtonSFXManager Instance { get; private set; }

    [Header("Nguồn phát âm thanh nút")]
    [SerializeField] private AudioSource sfxSource;

    [Header("Âm thanh click nút")]
    [SerializeField] private AudioClip buttonClickClip;

    [Range(0f, 1f)]
    [SerializeField] private float buttonClickVolume = 0.8f;

    [Header("Tự gắn âm cho tất cả Button trong scene")]
    [SerializeField] private bool autoRegisterButtons = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        if (sfxSource == null)
            sfxSource = GetComponent<AudioSource>();

        if (sfxSource == null)
            sfxSource = gameObject.AddComponent<AudioSource>();

        sfxSource.playOnAwake = false;
        sfxSource.loop = false;

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        if (autoRegisterButtons)
            RefreshButtons();
    }

    private void OnDestroy()
    {
        if (Instance == this)
            SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (autoRegisterButtons)
            RefreshButtons();
    }

    public void PlayButtonClick()
    {
        if (sfxSource == null)
            return;

        if (buttonClickClip == null)
            return;

        sfxSource.PlayOneShot(buttonClickClip, buttonClickVolume);
    }

    public void RefreshButtons()
    {
        Button[] buttons = UnityEngine.Object.FindObjectsByType<Button>(
            FindObjectsInactive.Include
        );

        int registeredCount = 0;

        for (int i = 0; i < buttons.Length; i++)
        {
            Button button = buttons[i];

            if (button == null)
                continue;

            if (!button.gameObject.scene.IsValid())
                continue;

            button.onClick.RemoveListener(PlayButtonClick);
            button.onClick.AddListener(PlayButtonClick);

            registeredCount++;
        }

        Debug.Log("Đã gắn âm click cho " + registeredCount + " button trong scene.");
    }
}