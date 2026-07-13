using UnityEngine;
using UnityEngine.SceneManagement;

public class PersistentHUD : MonoBehaviour
{
    public static PersistentHUD Instance { get; private set; }

    [Header("Không hiện HUD ở các scene này")]
    [SerializeField] private string loginSceneName = "LoginScene";
    [SerializeField] private string introSceneName = "IntroScene";

    private Canvas hudCanvas;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (transform.parent != null)
        {
            transform.SetParent(null);
        }

        DontDestroyOnLoad(gameObject);

        hudCanvas = GetComponent<Canvas>();
    }

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        RefreshHUDVisibility();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        RefreshHUDVisibility();
    }

    private void RefreshHUDVisibility()
    {
        string sceneName = SceneManager.GetActiveScene().name;

        bool shouldHide =
            sceneName == loginSceneName ||
            sceneName == introSceneName;

        if (hudCanvas == null)
            hudCanvas = GetComponent<Canvas>();

        if (hudCanvas != null)
            hudCanvas.enabled = !shouldHide;
    }
}