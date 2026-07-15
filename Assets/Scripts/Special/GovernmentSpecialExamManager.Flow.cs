using System.Collections;
using UnityEngine;

public partial class GovernmentSpecialExamManager
{
    private IEnumerator StartSpecialExamRoutine()
    {
        isExamining = true;

        StopPlayerMovementOnly();

        if (specialNpcAI != null && player != null)
        {
            specialNpcAI.SetBusy(true);
            specialNpcAI.ForceStopMovement();
            specialNpcAI.FaceTarget(player.position);
        }

        yield return null;

        StartSpecialCaseUI();

        isExamining = false;
    }

    private void StartSpecialCaseUI()
    {
        if (specialDiseaseCase == null)
        {
            Debug.LogError("GovernmentSpecialExamManager: Chưa kéo SpecialDiseaseCase.");
            return;
        }

        if (!CanUnlockSpecialGovernmentQuest())
        {
            Debug.LogWarning("GovernmentSpecialExamManager: Chưa đủ điều kiện mở nhiệm vụ Quan Huyện.");
            return;
        }

        if (!specialDiseaseCase.CanStartExam())
        {
            PrintCannotStartReason();
            return;
        }

        if (specialDiseaseCase.HasExamined &&
            specialDiseaseCase.HasChosenDiseaseName &&
            specialDiseaseCase.HasAddedToYThu)
        {
            StartSpecialPrescriptionUI();
            return;
        }

        specialDiseaseCase.MarkExamined();
        StartSpecialDiagnosisUI();
    }

    private void StartSpecialDiagnosisUI()
    {
        if (specialDiseaseCase == null)
        {
            Debug.LogError("GovernmentSpecialExamManager: Chưa có SpecialDiseaseCase.");
            return;
        }

        if (!specialDiseaseCase.CanChooseDiseaseName())
        {
            Debug.LogWarning(
                "Chưa thể mở UI chọn tên bệnh."
                + " | HasExamined = " + specialDiseaseCase.HasExamined
                + " | HasChosenDiseaseName = " + specialDiseaseCase.HasChosenDiseaseName
                + " | HasAddedToYThu = " + specialDiseaseCase.HasAddedToYThu
                + " | IsCured = " + specialDiseaseCase.IsCured
                + " | IsFailed = " + specialDiseaseCase.IsFailed
            );

            return;
        }

        if (specialDiagnosisUIController == null)
        {
            Debug.LogError("GovernmentSpecialExamManager: Chưa kéo GovernmentSpecialDiagnosisUIController.");
            return;
        }

        specialDiagnosisUIController.Show(
            specialDiseaseCase,
            OnSpecialDiseaseNameSelected
        );

        Debug.Log("Đã mở UI khám bệnh đặc biệt cho Quan Huyện.");
    }

    private void StartSpecialPrescriptionUI()
    {
        if (specialDiseaseCase == null)
        {
            Debug.LogError("GovernmentSpecialExamManager: Chưa có SpecialDiseaseCase.");
            return;
        }

        if (!CanUnlockSpecialGovernmentQuest())
        {
            Debug.LogWarning("Chưa thể bốc thuốc: nhiệm vụ Quan Huyện chưa đủ điều kiện kích hoạt.");
            return;
        }

        if (!specialDiseaseCase.CanTryTreatment())
        {
            Debug.LogWarning(
                "Chưa thể bốc thuốc cho Quan Huyện."
                + " | HasExamined = " + specialDiseaseCase.HasExamined
                + " | HasChosenDiseaseName = " + specialDiseaseCase.HasChosenDiseaseName
                + " | HasAddedToYThu = " + specialDiseaseCase.HasAddedToYThu
                + " | RemainingAttempts = " + specialDiseaseCase.RemainingAttempts
                + " | IsCured = " + specialDiseaseCase.IsCured
                + " | IsFailed = " + specialDiseaseCase.IsFailed
            );

            return;
        }

        if (specialPrescriptionUIController == null)
        {
            Debug.LogError("Chưa kéo GovernmentSpecialPrescriptionUIController.");
            return;
        }

        specialPrescriptionUIController.Show(
            specialDiseaseCase,
            OnSpecialTreatmentFinished
        );

        Debug.Log("Đã mở UI bốc thuốc đặc biệt cho Quan Huyện.");
    }

    private void OnSpecialDiseaseNameSelected(string selectedDiseaseName)
    {
        Debug.Log("Người chơi chọn tên bệnh cho Quan Huyện: " + selectedDiseaseName);

        StartSpecialPrescriptionUI();
    }

    private void OnSpecialTreatmentFinished(SpecialPrescriptionEvaluationResult result)
    {
        if (result == null)
            return;

        if (specialDiseaseCase == null)
            return;

        if (result.isCorrect)
        {
            HandleTreatmentSuccess(result);
            return;
        }

        if (specialDiseaseCase.IsFailed || specialDiseaseCase.RemainingAttempts <= 0)
        {
            HandleTreatmentFailed(result);
            return;
        }

        HandleTreatmentWrongButCanRetry(result);
    }
}