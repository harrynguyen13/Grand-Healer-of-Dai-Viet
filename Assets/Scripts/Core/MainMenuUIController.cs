using System.Collections;
using System.IO;
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

    private const string PlayerSceneKey = "PlayerScene";
    private const string PlayerXKey = "PlayerX";
    private const string PlayerYKey = "PlayerY";
    private const string PlayerZKey = "PlayerZ";

    private const string LoadFromSaveKey = "LoadFromSave";

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
        if (!isLoading)
            return;

        if (loadingSpinner != null)
        {
            loadingSpinner.Rotate(0f, 0f, -spinnerRotateSpeed * Time.deltaTime);
        }
    }

    public void OnPlayNowClicked()
    {
        if (isLoading)
            return;

        ClearOldSave();

        PlayerPrefs.SetInt(HasSeenIntroKey, 0);
        PlayerPrefs.Save();

        StartCoroutine(LoadSceneWithLoading(introSceneName, "Đang mở cốt truyện..."));
    }

    public void OnContinueClicked()
    {
        if (isLoading)
            return;

        if (!HasSave())
        {
            ShowMessage("Chưa có dữ liệu lưu.");
            return;
        }

        string savedSceneName = PlayerPrefs.GetString(PlayerSceneKey, gameSceneName);

        if (string.IsNullOrWhiteSpace(savedSceneName))
            savedSceneName = gameSceneName;

        PlayerPrefs.SetInt(LoadFromSaveKey, 1);
        PlayerPrefs.Save();

        Debug.Log("Tiếp tục game tại scene đã save: " + savedSceneName);

        StartCoroutine(LoadSceneWithLoading(savedSceneName, "Đang tải dữ liệu..."));
    }

    public void OnExitClicked()
    {
        if (isLoading)
            return;

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
        if (PlayerPrefs.GetInt(HasLocalSaveKey, 0) == 1)
            return true;

        if (File.Exists(GetJsonSavePath("player_economy_save.json")))
            return true;

        if (File.Exists(GetJsonSavePath("herb_inventory_save.json")))
            return true;

        return false;
    }

    private void ClearOldSave()
    {
        Debug.Log("===== RESET TOÀN BỘ SAVE ĐỂ BẮT ĐẦU GAME MỚI =====");

        ClearPlayerPrefsSave();

        ResetRuntimeData();

        DeleteAllJsonSaveFiles();

        PlayerPrefs.Save();

        Debug.Log("Đã reset dữ liệu save cũ.");
    }

    private void ClearPlayerPrefsSave()
    {
        PlayerPrefs.DeleteKey(HasLocalSaveKey);
        PlayerPrefs.DeleteKey(PlayerSceneKey);
        PlayerPrefs.DeleteKey(PlayerXKey);
        PlayerPrefs.DeleteKey(PlayerYKey);
        PlayerPrefs.DeleteKey(PlayerZKey);
        PlayerPrefs.DeleteKey(HasSeenIntroKey);
        PlayerPrefs.DeleteKey(LoadFromSaveKey);

        Debug.Log("Đã xóa PlayerPrefs save.");
    }

    private void ResetRuntimeData()
    {
        if (PlayerEconomy.Instance != null)
        {
            PlayerEconomy.Instance.SetMoney(200);
            PlayerEconomy.Instance.SetReputation(0);

            Debug.Log("Đã reset tiền/tín nhiệm trong RAM.");
        }

        if (HerbInventory.Instance != null)
        {
            HerbInventory.Instance.SendMessage(
                "ResetInventoryForNewGame",
                SendMessageOptions.DontRequireReceiver
            );

            Debug.Log("Đã gọi reset kho thuốc nếu HerbInventory có hàm ResetInventoryForNewGame.");
        }

        if (PatientVisitManager.Instance != null)
        {
            PatientVisitManager.Instance.SendMessage(
                "ClearAllPatientsForNewGame",
                SendMessageOptions.DontRequireReceiver
            );

            Debug.Log("Đã gọi reset hàng chờ bệnh nhân nếu PatientVisitManager có hàm ClearAllPatientsForNewGame.");
        }
    }

    private void DeleteAllJsonSaveFiles()
    {
        string folderPath = Application.persistentDataPath;

        if (!Directory.Exists(folderPath))
        {
            Debug.Log("Không tìm thấy thư mục save: " + folderPath);
            return;
        }

        string[] jsonFiles = Directory.GetFiles(folderPath, "*.json");

        foreach (string file in jsonFiles)
        {
            File.Delete(file);
            Debug.Log("Đã xóa file save: " + file);
        }
    }

    private string GetJsonSavePath(string fileName)
    {
        return Path.Combine(Application.persistentDataPath, fileName);
    }

    private void ShowMessage(string message)
    {
        if (messageText != null)
            messageText.text = message;
    }
}