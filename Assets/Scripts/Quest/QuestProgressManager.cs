using System.Globalization;
using System.Text;
using UnityEngine;

public class QuestProgressManager : MonoBehaviour
{
    public static QuestProgressManager Instance { get; private set; }

    private const string CorrectDiagnosisCountKey = "Quest_CorrectDiagnosisCount";
    private const string CorrectTreatmentCountKey = "Quest_CorrectTreatmentCount";

    private const string GatheredHerbTotalKey = "Quest_GatheredHerbTotal";
    private const string BoughtHerbTotalKey = "Quest_BoughtHerbTotal";
    private const string MoneySpentOnHerbsKey = "Quest_MoneySpentOnHerbs";

    private const string OfficialQuestCompletedKey = "OfficialQuestCompleted";

    private const string DiseaseKeyPrefix = "Quest_CuredDisease_";
    private const string LevelKeyPrefix = "Quest_CuredLevel_";
    private const string GatheredHerbKeyPrefix = "Quest_GatheredHerb_";
    private const string BoughtHerbKeyPrefix = "Quest_BoughtHerb_";

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
        "Bạc hà",
        "Sinh khương",
        "Tía tô",
        "Kinh giới",
        "Cam thảo",
        "Trần bì",
        "Bạch truật",
        "Tam thất",
        "Nhục quế",
        "Hùng hoàng",
        "Hoàng liên"
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

    public void RecordTreatmentResult(DiseaseData realDisease, bool diagnosisCorrect, bool prescriptionCorrect)
    {
        if (diagnosisCorrect)
        {
            AddInt(CorrectDiagnosisCountKey, 1);
        }

        bool fullCorrect = diagnosisCorrect && prescriptionCorrect;

        if (!fullCorrect)
        {
            PlayerPrefs.Save();
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
    }

    public void RecordHerbGathered(HerbData herb, int amount)
    {
        if (herb == null || amount <= 0)
            return;

        AddInt(GatheredHerbTotalKey, amount);
        AddInt(GetGatheredHerbKey(herb.herbName), amount);

        PlayerPrefs.Save();

        Debug.Log("Quest ghi nhận hái thuốc: " + herb.herbName + " x" + amount);
    }

    public void RecordHerbBought(HerbData herb, int amount, int totalCost)
    {
        if (herb == null || amount <= 0)
            return;

        AddInt(BoughtHerbTotalKey, amount);
        AddInt(GetBoughtHerbKey(herb.herbName), amount);
        AddInt(MoneySpentOnHerbsKey, Mathf.Max(0, totalCost));

        PlayerPrefs.Save();

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

    public bool IsOfficialQuestCompleted()
    {
        return PlayerPrefs.GetInt(OfficialQuestCompletedKey, 0) == 1;
    }

    public void CompleteOfficialQuest()
    {
        PlayerPrefs.SetInt(OfficialQuestCompletedKey, 1);
        PlayerPrefs.Save();

        Debug.Log("Đã hoàn thành nhiệm vụ chữa bệnh cho quan.");
    }

    public void ResetQuestProgressForNewGame()
    {
        PlayerPrefs.DeleteKey(CorrectDiagnosisCountKey);
        PlayerPrefs.DeleteKey(CorrectTreatmentCountKey);

        PlayerPrefs.DeleteKey(GatheredHerbTotalKey);
        PlayerPrefs.DeleteKey(BoughtHerbTotalKey);
        PlayerPrefs.DeleteKey(MoneySpentOnHerbsKey);

        PlayerPrefs.DeleteKey(OfficialQuestCompletedKey);

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
        }

        PlayerPrefs.Save();

        Debug.Log("Đã reset toàn bộ tiến độ nhiệm vụ.");
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