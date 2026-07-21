using System.Collections;
using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public class QuestProgressManager : MonoBehaviour
{
    public static QuestProgressManager Instance { get; private set; }

    private bool questRefreshPending;

    private const string CorrectDiagnosisCountKey = "Quest_CorrectDiagnosisCount";
    private const string CorrectTreatmentCountKey = "Quest_CorrectTreatmentCount";

    private const string GatheredHerbTotalKey = "Quest_GatheredHerbTotal";
    private const string BoughtHerbTotalKey = "Quest_BoughtHerbTotal";
    private const string MoneySpentOnHerbsKey = "Quest_MoneySpentOnHerbs";

    private const string GardenHarvestSessionTotalKey = "Quest_GardenHarvestSessionTotal";
    private const string GardenHarvestSessionHerbPrefix = "Quest_GardenHarvestSessionHerb_";
    private const string GardenHerbQuestTargetPrefix = "Quest_GardenHerbTarget_";

    private const string OfficialQuestCompletedKey = "OfficialQuestCompleted";
    private const string OfficialQuestFailedKey = "OfficialQuestFailed";

    private const string DiseaseKeyPrefix = "Quest_CuredDisease_";
    private const string LevelKeyPrefix = "Quest_CuredLevel_";
    private const string GatheredHerbKeyPrefix = "Quest_GatheredHerb_";
    private const string BoughtHerbKeyPrefix = "Quest_BoughtHerb_";
    private const string GardenHarvestBatchCompletedPrefix = "Quest_GardenHarvestBatchCompleted_";

    private readonly string[] trackedDiseaseKeys =
    {
        "AchNghichAnNac",
        "KhaiThauPhongNhiet",
        "TamHoaVuong",
        "ThanDuongHu",
        "ThatDietTrungDocDich"
    };

    private readonly string[] trackedHerbNames =
    {
        "Gừng",
        "Tía tô",
        "Kinh giới",
        "Bạc hà",
        "Diếp cá",
        "Bồ công anh"
    };

    private readonly string[] trackedGardenHerbQuestTargetIds =
    {
        "S2_RandomHerb_2_A",
        "S2_RandomHerb_3_B",

        "S3_RandomHerb_4_A",
        "S3_RandomHerb_5_B",
        "S3_RandomHerb_6_C",

        "S4_RandomHerb_8_A",
        "S4_RandomHerb_10_B",
        "S4_RandomHerb_12_C",

        "S5_RandomHerb_12_A",
        "S5_RandomHerb_15_B"
    };

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void RecordTreatmentResult(
        DiseaseData realDisease,
        bool diagnosisCorrect,
        bool prescriptionCorrect
    )
    {
        if (diagnosisCorrect)
        {
            AddInt(CorrectDiagnosisCountKey, 1);
        }

        bool fullCorrect = diagnosisCorrect && prescriptionCorrect;

        if (!fullCorrect)
        {
            PlayerPrefs.Save();
            RequestQuestRuntimeRefresh();
            return;
        }

        AddInt(CorrectTreatmentCountKey, 1);

        if (realDisease != null)
        {
            string diseaseAssetName = realDisease.name;
            int diseaseLevel = Mathf.Max(1, (int)realDisease.diseaseLevel);

            AddInt(GetDiseaseKey(diseaseAssetName), 1);
            AddInt(GetLevelKey(diseaseLevel), 1);

            Debug.Log("Quest ghi nhận chữa đúng bệnh: " + realDisease.diseaseName);
        }

        PlayerPrefs.Save();
        RequestQuestRuntimeRefresh();
    }

    public void RecordHerbGathered(HerbData herb, int amount)
    {
        if (herb == null || amount <= 0)
            return;

        AddInt(GatheredHerbTotalKey, amount);
        AddInt(GetGatheredHerbKey(herb.herbName), amount);

        PlayerPrefs.Save();
        RequestQuestRuntimeRefresh();

        Debug.Log("Quest ghi nhận số lượng hái thuốc: " + herb.herbName + " x" + amount);
    }

    public void RecordGardenHarvestSessionForHerb(HerbData herb)
    {
        if (herb == null)
            return;

        RecordGardenHarvestSessionForHerbName(herb.herbName);
    }

    public void RecordGardenHarvestSessionForHerbName(string herbName)
    {
        if (string.IsNullOrWhiteSpace(herbName))
            return;

        string herbKey = GetGardenHarvestHerbKey(herbName);

        if (string.IsNullOrWhiteSpace(herbKey))
            return;

        AddInt(GardenHarvestSessionTotalKey, 1);
        AddInt(GardenHarvestSessionHerbPrefix + herbKey, 1);

        PlayerPrefs.Save();
        RequestQuestRuntimeRefresh();

        Debug.Log("Quest ghi nhận số lần thu hoạch vị " + herbName + ": +1 lần.");
    }

    public void RecordGardenHarvestSessionForHerbNames(List<string> herbNames)
    {
        if (herbNames == null || herbNames.Count == 0)
            return;

        List<string> uniqueHerbKeys = new List<string>();

        for (int i = 0; i < herbNames.Count; i++)
        {
            string herbName = herbNames[i];

            if (string.IsNullOrWhiteSpace(herbName))
                continue;

            string herbKey = GetGardenHarvestHerbKey(herbName);

            if (string.IsNullOrWhiteSpace(herbKey))
                continue;

            if (uniqueHerbKeys.Contains(herbKey))
                continue;

            uniqueHerbKeys.Add(herbKey);
        }

        if (uniqueHerbKeys.Count == 0)
            return;

        AddInt(GardenHarvestSessionTotalKey, 1);

        for (int i = 0; i < uniqueHerbKeys.Count; i++)
        {
            AddInt(GardenHarvestSessionHerbPrefix + uniqueHerbKeys[i], 1);
        }

        PlayerPrefs.Save();
        RequestQuestRuntimeRefresh();

        Debug.Log("Quest ghi nhận 1 lần thu hoạch vườn. Số loại vị được tính: " + uniqueHerbKeys.Count);
    }

    public void RecordGardenHarvestSessionForHerbBatch(HerbData herb, string batchId)
    {
        if (herb == null)
            return;

        RecordGardenHarvestSessionForHerbBatchName(herb.herbName, batchId);
    }

    public void RecordGardenHarvestSessionForHerbBatchName(string herbName, string batchId)
    {
        if (string.IsNullOrWhiteSpace(herbName))
            return;

        if (string.IsNullOrWhiteSpace(batchId))
            batchId = System.Guid.NewGuid().ToString("N");

        string herbKey = GetGardenHarvestHerbKey(herbName);

        if (string.IsNullOrWhiteSpace(herbKey))
            return;

        string completedBatchKey =
            GardenHarvestBatchCompletedPrefix + herbKey + "_" + batchId.Trim();

        if (PlayerPrefs.GetInt(completedBatchKey, 0) == 1)
        {
            Debug.Log("Lượt trồng " + herbName + " này đã được tính nhiệm vụ rồi.");
            return;
        }

        PlayerPrefs.SetInt(completedBatchKey, 1);

        AddInt(GardenHarvestSessionTotalKey, 1);
        AddInt(GardenHarvestSessionHerbPrefix + herbKey, 1);

        PlayerPrefs.Save();
        RequestQuestRuntimeRefresh();

        Debug.Log("Quest ghi nhận lượt trồng vị " + herbName + ": +1 lần.");
    }

    public void RecordHerbBought(HerbData herb, int amount, int totalCost)
    {
        if (herb == null || amount <= 0)
            return;

        AddInt(BoughtHerbTotalKey, amount);
        AddInt(GetBoughtHerbKey(herb.herbName), amount);
        AddInt(MoneySpentOnHerbsKey, Mathf.Max(0, totalCost));

        PlayerPrefs.Save();
        RequestQuestRuntimeRefresh();

        Debug.Log("Quest ghi nhận mua thuốc: " + herb.herbName + " x" + amount);
    }

    public int GetCorrectDiagnosisCount()
    {
        return PlayerPrefs.GetInt(CorrectDiagnosisCountKey, 0);
    }

    public int GetCorrectTreatmentCount()
    {
        return PlayerPrefs.GetInt(CorrectTreatmentCountKey, 0);
    }

    public int GetDiseaseCuredCount(string diseaseAssetName)
    {
        if (string.IsNullOrWhiteSpace(diseaseAssetName))
            return 0;

        return PlayerPrefs.GetInt(GetDiseaseKey(diseaseAssetName), 0);
    }

    public int GetLevelCuredCount(int diseaseLevel)
    {
        diseaseLevel = Mathf.Max(1, diseaseLevel);
        return PlayerPrefs.GetInt(GetLevelKey(diseaseLevel), 0);
    }

    public int GetGatheredHerbTotal()
    {
        return PlayerPrefs.GetInt(GatheredHerbTotalKey, 0);
    }

    public int GetBoughtHerbTotal()
    {
        return PlayerPrefs.GetInt(BoughtHerbTotalKey, 0);
    }

    public int GetMoneySpentOnHerbs()
    {
        return PlayerPrefs.GetInt(MoneySpentOnHerbsKey, 0);
    }

    public int GetGatheredHerbCount(string herbName)
    {
        if (string.IsNullOrWhiteSpace(herbName))
            return 0;

        return PlayerPrefs.GetInt(GetGatheredHerbKey(herbName), 0);
    }

    public int GetBoughtHerbCount(string herbName)
    {
        if (string.IsNullOrWhiteSpace(herbName))
            return 0;

        return PlayerPrefs.GetInt(GetBoughtHerbKey(herbName), 0);
    }

    public int GetGardenHarvestSessionTotal()
    {
        return PlayerPrefs.GetInt(GardenHarvestSessionTotalKey, 0);
    }

    public int GetGardenHerbHarvestSessionCount(string herbName)
    {
        if (string.IsNullOrWhiteSpace(herbName))
            return 0;

        string herbKey = GetGardenHarvestHerbKey(herbName);

        if (string.IsNullOrWhiteSpace(herbKey))
            return 0;

        return PlayerPrefs.GetInt(GardenHarvestSessionHerbPrefix + herbKey, 0);
    }

    public bool IsOfficialQuestCompleted()
    {
        return PlayerPrefs.GetInt(OfficialQuestCompletedKey, 0) == 1;
    }

    public bool IsOfficialQuestFailed()
    {
        return PlayerPrefs.GetInt(OfficialQuestFailedKey, 0) == 1;
    }

    public void FailOfficialQuest()
    {
        PlayerPrefs.SetInt(OfficialQuestFailedKey, 1);
        PlayerPrefs.SetInt(OfficialQuestCompletedKey, 0);

        PlayerPrefs.Save();
        RequestQuestRuntimeRefresh();

        Debug.Log("Nhiệm vụ chữa bệnh cho quan đã thất bại.");
    }

    public void CompleteOfficialQuest()
    {
        PlayerPrefs.SetInt(OfficialQuestCompletedKey, 1);
        PlayerPrefs.SetInt(OfficialQuestFailedKey, 0);

        PlayerPrefs.Save();
        RequestQuestRuntimeRefresh();

        Debug.Log("Đã hoàn thành nhiệm vụ chữa bệnh cho quan.");
    }

    public void ResetQuestProgressForNewGame()
    {
        PlayerPrefs.DeleteKey(CorrectDiagnosisCountKey);
        PlayerPrefs.DeleteKey(CorrectTreatmentCountKey);

        PlayerPrefs.DeleteKey(GatheredHerbTotalKey);
        PlayerPrefs.DeleteKey(BoughtHerbTotalKey);
        PlayerPrefs.DeleteKey(MoneySpentOnHerbsKey);

        PlayerPrefs.DeleteKey(GardenHarvestSessionTotalKey);

        PlayerPrefs.DeleteKey("Quest_CompletedOnce_S1_Rank_100");
        PlayerPrefs.DeleteKey("Quest_CompletedOnce_S2_Rank_200");
        PlayerPrefs.DeleteKey("Quest_CompletedOnce_S3_Rank_300");
        PlayerPrefs.DeleteKey("Quest_CompletedOnce_S4_Rank_500");

        PlayerPrefs.DeleteKey(OfficialQuestCompletedKey);
        PlayerPrefs.DeleteKey(OfficialQuestFailedKey);

        for (int i = 1; i <= 5; i++)
        {
            PlayerPrefs.DeleteKey(GetLevelKey(i));
        }

        for (int i = 0; i < trackedDiseaseKeys.Length; i++)
        {
            PlayerPrefs.DeleteKey(GetDiseaseKey(trackedDiseaseKeys[i]));
        }

        for (int i = 0; i < trackedHerbNames.Length; i++)
        {
            PlayerPrefs.DeleteKey(GetGatheredHerbKey(trackedHerbNames[i]));
            PlayerPrefs.DeleteKey(GetBoughtHerbKey(trackedHerbNames[i]));

            string gardenHarvestKey = GetGardenHarvestHerbKey(trackedHerbNames[i]);

            if (!string.IsNullOrWhiteSpace(gardenHarvestKey))
                PlayerPrefs.DeleteKey(GardenHarvestSessionHerbPrefix + gardenHarvestKey);
        }

        for (int i = 0; i < trackedGardenHerbQuestTargetIds.Length; i++)
        {
            PlayerPrefs.DeleteKey(
                GardenHerbQuestTargetPrefix + trackedGardenHerbQuestTargetIds[i]
            );
        }

        PlayerPrefs.Save();

        Debug.Log("Đã reset toàn bộ tiến độ nhiệm vụ.");
    }

    private void RequestQuestRuntimeRefresh()
    {
        if (questRefreshPending)
            return;

        questRefreshPending = true;
        StartCoroutine(RefreshQuestRuntimeNextFrame());
    }

    private IEnumerator RefreshQuestRuntimeNextFrame()
    {
        yield return null;

        questRefreshPending = false;

        if (QuestRuntimeManager.Instance == null)
        {
            Debug.LogWarning(
                "QuestProgressManager: Không tìm thấy QuestRuntimeManager để kiểm tra nhiệm vụ."
            );

            yield break;
        }

        QuestRuntimeManager.Instance.RefreshQuestStateNow();
    }

    private void AddInt(string key, int amount)
    {
        int currentValue = PlayerPrefs.GetInt(key, 0);
        currentValue += amount;

        if (currentValue < 0)
            currentValue = 0;

        PlayerPrefs.SetInt(key, currentValue);
    }

    private string GetDiseaseKey(string diseaseAssetName)
    {
        return DiseaseKeyPrefix + diseaseAssetName;
    }

    private string GetLevelKey(int diseaseLevel)
    {
        return LevelKeyPrefix + diseaseLevel;
    }

    private string GetGatheredHerbKey(string herbName)
    {
        return GatheredHerbKeyPrefix + NormalizeKey(herbName);
    }

    private string GetBoughtHerbKey(string herbName)
    {
        return BoughtHerbKeyPrefix + NormalizeKey(herbName);
    }

    private string GetGardenHarvestHerbKey(string herbName)
    {
        string key = NormalizeKey(herbName);

        if (key == "gung")
            return "gung";

        if (key == "tia_to")
            return "tia_to";

        if (key == "kinh_gioi")
            return "kinh_gioi";

        if (key == "bac_ha")
            return "bac_ha";

        if (key == "diep_ca")
            return "diep_ca";

        if (key == "bo_cong_anh")
            return "bo_cong_anh";

        return key;
    }

    private string NormalizeKey(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        string normalized = value.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);

        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < normalized.Length; i++)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(normalized[i]);

            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(normalized[i]);
            }
        }

        string key = builder.ToString().Normalize(NormalizationForm.FormC);

        key = key.Replace("đ", "d");
        key = key.Replace(" ", "_");
        key = key.Replace("-", "_");

        while (key.Contains("__"))
        {
            key = key.Replace("__", "_");
        }

        return key;
    }
}