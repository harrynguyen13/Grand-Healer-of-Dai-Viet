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

    [Tooltip("Thời gian tối thiểu để loading hiện ra. Để 0 nếu không muốn delay.")]
    [SerializeField] private float minimumLoadingVisibleTime = 0.15f;

    private const string HasLocalSaveKey = "HasLocalSave";
    private const string HasSeenIntroKey = "HasSeenIntro";

    private const string PlayerSceneKey = "PlayerScene";
    private const string PlayerXKey = "PlayerX";
    private const string PlayerYKey = "PlayerY";
    private const string PlayerZKey = "PlayerZ";

    private const string LoadFromSaveKey = "LoadFromSave";

    private const string OfficialQuestCompletedKey = "OfficialQuestCompleted";

    private const string QuestPanelActiveStageKey = "QuestPanel_ActiveStage";
    private const string QuestPanelActiveQuest0Key = "QuestPanel_ActiveQuest_0";
    private const string QuestPanelActiveQuest1Key = "QuestPanel_ActiveQuest_1";
    private const string QuestPanelRewardRunIdKey = "QuestPanel_RewardRunId";

    private const string CorrectDiagnosisCountKey = "Quest_CorrectDiagnosisCount";
    private const string CorrectTreatmentCountKey = "Quest_CorrectTreatmentCount";
    private const string GatheredHerbTotalKey = "Quest_GatheredHerbTotal";
    private const string BoughtHerbTotalKey = "Quest_BoughtHerbTotal";
    private const string MoneySpentOnHerbsKey = "Quest_MoneySpentOnHerbs";

    private const string HomeGardenReadyKey = "HerbGarden_HomeGarden_01_Ready";
    private const string HomeGardenNextReadyKey = "HerbGarden_HomeGarden_01_NextReadyUtcTicks";

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

        // Chơi mới tuyệt đối không được load vị trí save cũ
        PlayerPrefs.SetInt(LoadFromSaveKey, 0);
        PlayerPrefs.DeleteKey(PlayerSceneKey);
        PlayerPrefs.DeleteKey(PlayerXKey);
        PlayerPrefs.DeleteKey(PlayerYKey);
        PlayerPrefs.DeleteKey(PlayerZKey);

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

        StartCoroutine(ContinueGameWithLoading());
    }

    private IEnumerator ContinueGameWithLoading()
    {
        isLoading = true;

        ShowMessage("");

        if (loadingPanel != null)
            loadingPanel.SetActive(true);

        if (loadingText != null)
            loadingText.text = "Đang tải dữ liệu...";

        Canvas.ForceUpdateCanvases();

        yield return null;

        if (minimumLoadingVisibleTime > 0f)
        {
            yield return new WaitForSecondsRealtime(minimumLoadingVisibleTime);
        }

        if (LocalSaveManager.Instance != null && LocalSaveManager.Instance.HasLocalSave())
        {
            LocalSaveManager.Instance.ContinueGameFromLocalSave();
            yield break;
        }

        string savedSceneName = PlayerPrefs.GetString(PlayerSceneKey, gameSceneName);

        if (string.IsNullOrWhiteSpace(savedSceneName))
            savedSceneName = gameSceneName;

        PlayerPrefs.SetInt(LoadFromSaveKey, 1);
        PlayerPrefs.Save();

        Debug.Log("Tiếp tục game tại scene đã save: " + savedSceneName);

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(savedSceneName);
        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
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

        Canvas.ForceUpdateCanvases();

        yield return null;

        if (minimumLoadingVisibleTime > 0f)
        {
            yield return new WaitForSecondsRealtime(minimumLoadingVisibleTime);
        }

        AsyncOperation asyncLoad = SceneManager.LoadSceneAsync(sceneName);
        asyncLoad.allowSceneActivation = true;

        while (!asyncLoad.isDone)
        {
            yield return null;
        }
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

        DeleteAllJsonSaveFiles();

        if (LocalSaveManager.Instance != null)
        {
            LocalSaveManager.Instance.ResetForNewGame();
        }

        ResetRuntimeData();

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
        PlayerPrefs.SetInt(LoadFromSaveKey, 0);

        ClearQuestPlayerPrefsSave();
        ClearGardenPlayerPrefsSave();

        Debug.Log("Đã xóa PlayerPrefs save.");
    }

    private void ClearQuestPlayerPrefsSave()
    {
        PlayerPrefs.DeleteKey(OfficialQuestCompletedKey);

        PlayerPrefs.DeleteKey(QuestPanelActiveStageKey);
        PlayerPrefs.DeleteKey(QuestPanelActiveQuest0Key);
        PlayerPrefs.DeleteKey(QuestPanelActiveQuest1Key);
        PlayerPrefs.DeleteKey(QuestPanelRewardRunIdKey);

        PlayerPrefs.DeleteKey(CorrectDiagnosisCountKey);
        PlayerPrefs.DeleteKey(CorrectTreatmentCountKey);
        PlayerPrefs.DeleteKey(GatheredHerbTotalKey);
        PlayerPrefs.DeleteKey(BoughtHerbTotalKey);
        PlayerPrefs.DeleteKey(MoneySpentOnHerbsKey);

        for (int i = 1; i <= 5; i++)
        {
            PlayerPrefs.DeleteKey("Quest_CuredLevel_" + i);
        }

        string[] diseaseKeys =
        {
            "AchNghichAnNac",
            "KhaiThauPhongNhiet",
            "TamHoaVuong",
            "ThanDuongHu",
            "ThatDietTrungDocDich"
        };

        for (int i = 0; i < diseaseKeys.Length; i++)
        {
            PlayerPrefs.DeleteKey("Quest_CuredDisease_" + diseaseKeys[i]);
        }

        string[] herbKeys =
        {
            "bac_ha",
            "sinh_khuong",
            "tia_to",
            "kinh_gioi",
            "cam_thao",
            "tran_bi",
            "bach_truat",
            "tam_that",
            "nhuc_que",
            "hung_hoang",
            "hoang_lien"
        };

        for (int i = 0; i < herbKeys.Length; i++)
        {
            PlayerPrefs.DeleteKey("Quest_GatheredHerb_" + herbKeys[i]);
            PlayerPrefs.DeleteKey("Quest_BoughtHerb_" + herbKeys[i]);
        }

        Debug.Log("Đã xóa toàn bộ PlayerPrefs nhiệm vụ.");
    }

    private void ClearGardenPlayerPrefsSave()
    {
        PlayerPrefs.DeleteKey(HomeGardenReadyKey);
        PlayerPrefs.DeleteKey(HomeGardenNextReadyKey);

        Debug.Log("Đã xóa PlayerPrefs vườn thuốc.");
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

        if (MailboxManager.Instance != null)
        {
            MailboxManager.Instance.ResetMailboxForNewGame();

            Debug.Log("Đã reset hòm thư trong RAM.");
        }

        if (QuestProgressManager.Instance != null)
        {
            QuestProgressManager.Instance.ResetQuestProgressForNewGame();

            Debug.Log("Đã reset tiến độ nhiệm vụ trong RAM.");
        }

        if (QuestRuntimeManager.Instance != null)
        {
            QuestRuntimeManager.Instance.ResetQuestRuntimeForNewGame();

            Debug.Log("Đã reset nhiệm vụ đang nhận trong RAM.");
        }

        if (QuestRewardManager.Instance != null)
        {
            QuestRewardManager.Instance.ResetRewardForNewGame();

            Debug.Log("Đã reset phần thưởng nhiệm vụ trong RAM.");
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