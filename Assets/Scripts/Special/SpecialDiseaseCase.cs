using UnityEngine;

public enum SpecialCaseState
{
    NotStarted,
    InProgress,
    Cured,
    Failed
}

public class SpecialDiseaseCase : MonoBehaviour
{
    private const string HasExaminedKey = "SpecialCase_HasExamined";
    private const string HasChosenDiseaseNameKey = "SpecialCase_HasChosenDiseaseName";
    private const string HasAddedToYThuKey = "SpecialCase_HasAddedToYThu";
    private const string TreatmentAttemptCountKey = "SpecialCase_TreatmentAttemptCount";
    private const string IsCuredKey = "SpecialCase_IsCured";
    private const string IsFailedKey = "SpecialCase_IsFailed";

    [Header("Bệnh đặc biệt của Quan Huyện")]
    [SerializeField] private DiseaseData specialDisease;

    [Header("4 tên bệnh để người chơi lựa chọn khi khám")]
    [SerializeField] private string[] diseaseNameOptions = new string[4];

    [Header("Số lần được thử chữa")]
    [SerializeField] private int maxTreatmentAttempts = 3;

    [Header("Trạng thái ca bệnh")]
    [SerializeField] private bool questUnlocked;
    [SerializeField] private bool hasExamined;
    [SerializeField] private bool hasChosenDiseaseName;
    [SerializeField] private bool hasAddedToYThu;
    [SerializeField] private bool isCured;
    [SerializeField] private bool isFailed;

    [SerializeField] private int treatmentAttemptCount;

    [Header("Tên bệnh người chơi đã chọn")]
    [SerializeField] private string selectedDiseaseName;

    public DiseaseData SpecialDisease => specialDisease;
    public string[] DiseaseNameOptions => diseaseNameOptions;

    public string SelectedDiseaseName => selectedDiseaseName;
    public int MaxTreatmentAttempts => maxTreatmentAttempts;
    public int TreatmentAttemptCount => treatmentAttemptCount;

    public bool QuestUnlocked => questUnlocked;
    public bool HasExamined => hasExamined;
    public bool HasChosenDiseaseName => hasChosenDiseaseName;
    public bool HasAddedToYThu => hasAddedToYThu;
    public bool IsCured => isCured;
    public bool IsFailed => isFailed;

    public bool IsFinished
    {
        get
        {
            return isCured || isFailed;
        }
    }

    public bool CanUnlockFinalRank
    {
        get
        {
            return isCured && !isFailed;
        }
    }

    public int RemainingAttempts
    {
        get
        {
            return Mathf.Max(0, maxTreatmentAttempts - treatmentAttemptCount);
        }
    }

    public SpecialCaseState CurrentState
    {
        get
        {
            if (isCured)
                return SpecialCaseState.Cured;

            if (isFailed)
                return SpecialCaseState.Failed;

            if (hasExamined || hasChosenDiseaseName || hasAddedToYThu || treatmentAttemptCount > 0)
                return SpecialCaseState.InProgress;

            return SpecialCaseState.NotStarted;
        }
    }

    private void Awake()
    {
        SpecialYThuDiseaseService.RegisterCase(this);
        LoadSavedProgress();
    }

    private void OnEnable()
    {
        SpecialYThuDiseaseService.RegisterCase(this);
        LoadSavedProgress();
    }

    public bool CanStartExam()
    {
        if (!questUnlocked)
            return false;

        if (specialDisease == null)
            return false;

        if (isCured)
            return false;

        if (isFailed)
            return false;

        return true;
    }

    public bool CanChooseDiseaseName()
    {
        if (!questUnlocked)
            return false;

        if (!hasExamined)
            return false;

        if (hasChosenDiseaseName)
            return false;

        if (specialDisease == null)
            return false;

        if (isCured || isFailed)
            return false;

        return true;
    }

    public bool CanTryTreatment()
    {
        if (!questUnlocked)
            return false;

        if (!hasExamined)
            return false;

        if (!hasChosenDiseaseName)
            return false;

        if (!hasAddedToYThu)
            return false;

        if (isCured)
            return false;

        if (isFailed)
            return false;

        if (treatmentAttemptCount >= maxTreatmentAttempts)
            return false;

        return true;
    }

    public void UnlockQuest()
    {
        questUnlocked = true;
        LoadSavedProgress();
    }

    public void MarkExamined()
    {
        if (!questUnlocked)
            return;

        if (specialDisease == null)
            return;

        if (isCured || isFailed)
            return;

        hasExamined = true;
        SaveProgress();
    }

    public void ChooseDiseaseName(string diseaseName)
    {
        if (!CanChooseDiseaseName())
            return;

        if (string.IsNullOrWhiteSpace(diseaseName))
            return;

        selectedDiseaseName = diseaseName.Trim();
        hasChosenDiseaseName = true;
        hasAddedToYThu = true;

        SpecialYThuDiseaseService.AddToYThu(this);

        SaveProgress();
    }

    public void ChooseDiseaseNameByIndex(int index)
    {
        if (diseaseNameOptions == null)
            return;

        if (index < 0 || index >= diseaseNameOptions.Length)
            return;

        ChooseDiseaseName(diseaseNameOptions[index]);
    }

    public void RegisterTreatmentResult(bool success)
    {
        if (!CanTryTreatment())
            return;

        treatmentAttemptCount++;

        if (success)
        {
            isCured = true;
            isFailed = false;
        }
        else
        {
            if (treatmentAttemptCount >= maxTreatmentAttempts)
            {
                isFailed = true;
                isCured = false;
            }
        }

        SaveProgress();

        Debug.Log(
            "SpecialDiseaseCase: Kết quả chữa Quan Huyện."
            + " | Success = " + success
            + " | Attempts = " + treatmentAttemptCount + "/" + maxTreatmentAttempts
            + " | Remaining = " + RemainingAttempts
            + " | IsCured = " + isCured
            + " | IsFailed = " + isFailed
            + " | State = " + CurrentState
        );
    }

    private void LoadSavedProgress()
    {
        if (specialDisease == null)
            return;

        if (SpecialYThuDiseaseService.HasSpecialDiseaseInYThu())
        {
            string savedName = SpecialYThuDiseaseService.GetSelectedDiseaseName();

            if (!string.IsNullOrWhiteSpace(savedName))
            {
                selectedDiseaseName = savedName.Trim();

                hasExamined = true;
                hasChosenDiseaseName = true;
                hasAddedToYThu = true;
            }
        }

        if (PlayerPrefs.GetInt(HasExaminedKey, 0) == 1)
            hasExamined = true;

        if (PlayerPrefs.GetInt(HasChosenDiseaseNameKey, 0) == 1)
            hasChosenDiseaseName = true;

        if (PlayerPrefs.GetInt(HasAddedToYThuKey, 0) == 1)
            hasAddedToYThu = true;

        treatmentAttemptCount = PlayerPrefs.GetInt(TreatmentAttemptCountKey, treatmentAttemptCount);
        treatmentAttemptCount = Mathf.Clamp(treatmentAttemptCount, 0, maxTreatmentAttempts);

        isCured = PlayerPrefs.GetInt(IsCuredKey, 0) == 1;
        isFailed = PlayerPrefs.GetInt(IsFailedKey, 0) == 1;

        if (isCured)
            isFailed = false;

        if (!isCured && treatmentAttemptCount >= maxTreatmentAttempts)
            isFailed = true;

        Debug.Log(
            "SpecialDiseaseCase: Đã load trạng thái Quan Huyện."
            + " | HasExamined = " + hasExamined
            + " | HasChosenDiseaseName = " + hasChosenDiseaseName
            + " | HasAddedToYThu = " + hasAddedToYThu
            + " | SelectedName = " + selectedDiseaseName
            + " | Attempts = " + treatmentAttemptCount + "/" + maxTreatmentAttempts
            + " | IsCured = " + isCured
            + " | IsFailed = " + isFailed
            + " | State = " + CurrentState
        );
    }

    private void SaveProgress()
    {
        PlayerPrefs.SetInt(HasExaminedKey, hasExamined ? 1 : 0);
        PlayerPrefs.SetInt(HasChosenDiseaseNameKey, hasChosenDiseaseName ? 1 : 0);
        PlayerPrefs.SetInt(HasAddedToYThuKey, hasAddedToYThu ? 1 : 0);
        PlayerPrefs.SetInt(TreatmentAttemptCountKey, treatmentAttemptCount);
        PlayerPrefs.SetInt(IsCuredKey, isCured ? 1 : 0);
        PlayerPrefs.SetInt(IsFailedKey, isFailed ? 1 : 0);

        PlayerPrefs.Save();

        Debug.Log("SpecialDiseaseCase: Đã lưu trạng thái Quan Huyện.");
    }

    public void ResetSpecialCase()
    {
        questUnlocked = false;
        hasExamined = false;
        hasChosenDiseaseName = false;
        hasAddedToYThu = false;
        isCured = false;
        isFailed = false;
        treatmentAttemptCount = 0;
        selectedDiseaseName = "";

        PlayerPrefs.DeleteKey(HasExaminedKey);
        PlayerPrefs.DeleteKey(HasChosenDiseaseNameKey);
        PlayerPrefs.DeleteKey(HasAddedToYThuKey);
        PlayerPrefs.DeleteKey(TreatmentAttemptCountKey);
        PlayerPrefs.DeleteKey(IsCuredKey);
        PlayerPrefs.DeleteKey(IsFailedKey);
        PlayerPrefs.Save();

        SpecialYThuDiseaseService.Reset();
        SpecialYThuPrescriptionRecordService.Reset();
    }

    [ContextMenu("DEBUG - Unlock Quest")]
    private void DebugUnlockQuest()
    {
        UnlockQuest();
        Debug.Log("SpecialDiseaseCase: Đã mở nhiệm vụ đặc biệt Quan Huyện.");
    }

    [ContextMenu("DEBUG - Mark Examined")]
    private void DebugMarkExamined()
    {
        MarkExamined();
        Debug.Log("SpecialDiseaseCase: Đã đánh dấu đã khám bệnh đặc biệt.");
    }

    [ContextMenu("DEBUG - Choose Name 0")]
    private void DebugChooseName0()
    {
        ChooseDiseaseNameByIndex(0);
        Debug.Log("SpecialDiseaseCase: Đã chọn tên bệnh: " + selectedDiseaseName);
    }

    [ContextMenu("DEBUG - Reset Special Case")]
    private void DebugResetSpecialCase()
    {
        ResetSpecialCase();
        Debug.Log("SpecialDiseaseCase: Đã reset ca bệnh đặc biệt.");
    }

    [ContextMenu("DEBUG - Print Case Info")]
    private void DebugPrintCaseInfo()
    {
        if (specialDisease == null)
        {
            Debug.LogWarning("SpecialDiseaseCase: Chưa gán bệnh đặc biệt.");
            return;
        }

        Debug.Log(
            "Bệnh gốc: " + specialDisease.diseaseName +
            " | Tên người chơi chọn: " + selectedDiseaseName +
            " | Đã mở quest: " + questUnlocked +
            " | Đã khám: " + hasExamined +
            " | Đã chọn tên: " + hasChosenDiseaseName +
            " | Đã thêm vào Y Thư: " + hasAddedToYThu +
            " | Đã khỏi: " + isCured +
            " | Đã thất bại: " + isFailed +
            " | Trạng thái: " + CurrentState +
            " | Số lần thử: " + treatmentAttemptCount + "/" + maxTreatmentAttempts
        );
    }
}