using UnityEngine;

public class SpecialDiseaseCase : MonoBehaviour
{
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

    public int RemainingAttempts
    {
        get
        {
            return Mathf.Max(0, maxTreatmentAttempts - treatmentAttemptCount);
        }
    }

    public bool CanStartExam()
    {
        if (!questUnlocked)
            return false;

        if (specialDisease == null)
            return false;

        if (isCured)
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

        if (treatmentAttemptCount >= maxTreatmentAttempts)
            return false;

        return true;
    }

    public void UnlockQuest()
    {
        questUnlocked = true;
    }

    public void MarkExamined()
    {
        if (!questUnlocked)
            return;

        if (specialDisease == null)
            return;

        hasExamined = true;
    }

    public void ChooseDiseaseName(string diseaseName)
    {
        if (!CanChooseDiseaseName())
            return;

        if (string.IsNullOrWhiteSpace(diseaseName))
            return;

        selectedDiseaseName = diseaseName.Trim();
        hasChosenDiseaseName = true;

        // Chỉ sau khi người chơi chọn tên bệnh thì mới cho ghi vào Y Thư
        hasAddedToYThu = true;
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
            isCured = true;
    }

    public void ResetSpecialCase()
    {
        questUnlocked = false;
        hasExamined = false;
        hasChosenDiseaseName = false;
        hasAddedToYThu = false;
        isCured = false;
        treatmentAttemptCount = 0;
        selectedDiseaseName = "";
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
            " | Số lần thử: " + treatmentAttemptCount + "/" + maxTreatmentAttempts
        );
    }
}