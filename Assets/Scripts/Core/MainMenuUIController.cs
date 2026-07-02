using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class MainMenuUIController : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string introSceneName = "IntroScene";
    [SerializeField] private string gameSceneName = "SampleScene";

    [Header("UI")]
    [SerializeField] private TMP_Text messageText;

    [Header("Loading UI")]
    [SerializeField] private GameObject loadingPanel;
    [SerializeField] private TMP_Text loadingText;
    [SerializeField] private RectTransform loadingSpinner;
    [SerializeField] private float spinnerRotateSpeed = 600f;

    private const string HasLocalSaveKey = "HasLocalSave";
    private const string HasSeenIntroKey = "HasSeenIntro";

    private bool isLoading = false;

    private void Start()
    {
        if (messageText != null)
            messageText.text = "";

        if (loadingPanel != null)
            loadingPanel.SetActive(false);
    }

    private void Update()
    {
        if (!isLoading) return;

        if (loadingSpinner != null)
        {
            loadingSpinner.Rotate(0f, 0f, -spinnerRotateSpeed * Time.deltaTime);
        }
    }

    public void OnPlayNowClicked()
    {
        if (isLoading) return;

        ClearOldSave();

        PlayerPrefs.SetInt(HasSeenIntroKey, 0);
        PlayerPrefs.Save();

        StartCoroutine(LoadSceneWithLoading(introSceneName, "Đang mở cốt truyện..."));
    }

    public void OnContinueClicked()
    {
        if (isLoading) return;

        if (!HasSave())
        {
            ShowMessage("Chưa có dữ liệu lưu.");
            return;
        }

        StartCoroutine(LoadSceneWithLoading(gameSceneName, "Đang tải dữ liệu..."));
    }

    public void OnExitClicked()
    {
        if (isLoading) return;

        Application.Quit();

#if UNITY_EDITOR
        UnityEditor.EditorApplication.isPlaying = false;
#endif
    }

    private IEnumerator LoadSceneWithLoading(string sceneName, string loadingMessage)
    {
        isLoading = true;

        ShowMessage("");

        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        if (loadingText != null)
            loadingText.text = loadingMessage;

        yield return null;

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = true;
    }

    private bool HasSave()
    {
        return PlayerPrefs.GetInt(HasLocalSaveKey, 0) == 1;
    }

    private void ClearOldSave()
    {
        PlayerPrefs.DeleteKey(HasLocalSaveKey);
        PlayerPrefs.DeleteKey("PlayerScene");
        PlayerPrefs.DeleteKey("PlayerX");
        PlayerPrefs.DeleteKey("PlayerY");
        PlayerPrefs.DeleteKey("PlayerZ");
        PlayerPrefs.DeleteKey(HasSeenIntroKey);
        PlayerPrefs.Save();
    }

    private void ShowMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;
    }
}