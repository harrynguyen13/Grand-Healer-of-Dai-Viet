using UnityEngine;

public partial class GovernmentSpecialExamManager
{
    private bool CanUnlockSpecialGovernmentQuest()
    {
        // Chế độ test được phép bỏ qua toàn bộ điều kiện thật.
        if (debugTestSpecialQuest)
            return true;

        int currentStage = PlayerLevelService.GetCurrentStage();

        if (currentStage < 5)
        {
            Debug.Log(
                "Chưa mở nhiệm vụ Quan Huyện: Player chưa đạt Chương 5."
                + " | CurrentStage = " + currentStage
                + " | Rank = " + PlayerLevelService.GetCurrentRankName()
            );

            return false;
        }

        if (SpecialQuestMailBridge.Instance == null)
        {
            Debug.LogWarning(
                "Chưa mở nhiệm vụ Quan Huyện: Không tìm thấy SpecialQuestMailBridge."
            );
            return false;
        }

        if (!SpecialQuestMailBridge.Instance.HasSentMail())
        {
            Debug.Log(
                "Chưa mở nhiệm vụ Quan Huyện: Thư nhiệm vụ chưa được gửi."
            );
            return false;
        }

        return true;
    }

    private void PrintCannotStartReason()
    {
        if (specialDiseaseCase == null)
            return;

        if (!debugTestSpecialQuest &&
            PlayerLevelService.GetCurrentStage() < 5)
        {
            Debug.LogWarning(
                "Không khám được: Player chưa đạt Chương 5."
                + " | CurrentStage = " + PlayerLevelService.GetCurrentStage()
                + " | Rank = " + PlayerLevelService.GetCurrentRankName()
            );
            return;
        }

        if (!specialDiseaseCase.QuestUnlocked)
        {
            Debug.LogWarning("Không khám được: Chưa mở nhiệm vụ Quan Huyện.");
            return;
        }

        if (specialDiseaseCase.SpecialDisease == null)
        {
            Debug.LogWarning("Không khám được: Chưa gán DiseaseData bệnh đặc biệt.");
            return;
        }

        if (specialDiseaseCase.IsCured)
        {
            Debug.Log("Quan Huyện đã khỏi bệnh. Nhiệm vụ Quan Huyện đã hoàn thành.");
            return;
        }

        if (specialDiseaseCase.IsFailed)
        {
            Debug.LogWarning("Nhiệm vụ Quan Huyện đã thất bại. Không thể khám hoặc bốc thuốc tiếp.");
            return;
        }

        Debug.LogWarning(
            "Không khám được: CanStartExam = false"
            + " | QuestUnlocked = " + specialDiseaseCase.QuestUnlocked
            + " | Disease = " + (specialDiseaseCase.SpecialDisease != null)
            + " | IsCured = " + specialDiseaseCase.IsCured
            + " | IsFailed = " + specialDiseaseCase.IsFailed
            + " | RemainingAttempts = " + specialDiseaseCase.RemainingAttempts
        );
    }
}