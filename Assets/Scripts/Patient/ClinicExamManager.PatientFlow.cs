
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ClinicExamManager
{
    private PendingPatientMailData pendingPatientMailData;

    private void OnPrescriptionConfirmed(
        Dictionary<HerbData, int> selectedPrescription
    )
    {
        Debug.Log("===== CLINIC NHẬN ĐƠN THUỐC =====");

        if (
            selectedPrescription == null
            || selectedPrescription.Count == 0
        )
        {
            Debug.LogWarning("Đơn thuốc rỗng.");
            return;
        }

        if (
            currentPatient == null
            || currentPatient.PatientCase == null
        )
        {
            Debug.LogError(
                "Không có bệnh nhân hoặc PatientCase hiện tại."
            );
            return;
        }

        PatientCase patientCase =
            currentPatient.PatientCase;

        if (patientCase.realDisease == null)
        {
            Debug.LogError(
                "PatientCase không có bệnh thật."
            );
            return;
        }

        foreach (
            KeyValuePair<HerbData, int> pair
            in selectedPrescription
        )
        {
            if (pair.Key == null)
                continue;

            Debug.Log(
                "Thuốc đã kê: "
                + pair.Key.herbName
                + " x"
                + pair.Value
            );
        }

        isCurrentPrescriptionCorrect =
            ClinicPrescriptionService
                .IsPrescriptionCorrectForDisease(
                    patientCase.realDisease,
                    selectedPrescription
                );

        Debug.Log("===== KẾT QUẢ KIỂM TRA =====");
        Debug.Log(
            "Bệnh thật: "
            + patientCase.realDisease.diseaseName
        );
        Debug.Log(
            "Chẩn đoán đúng: "
            + isCurrentDiagnosisCorrect
        );
        Debug.Log(
            "Đơn thuốc đúng: "
            + isCurrentPrescriptionCorrect
        );
        Debug.Log(
            "Thuốc cần có: "
            + ClinicPrescriptionService
                .GetRequiredHerbNames(
                    patientCase.realDisease
                )
        );

        if (HerbInventory.Instance == null)
        {
            Debug.LogError(
                "Không tìm thấy HerbInventory. "
                + "Kiểm tra GameSystems ở LoginScene."
            );
            return;
        }

        bool removed =
            HerbInventory.Instance
                .RemovePrescription(
                    selectedPrescription
                );

        if (!removed)
        {
            Debug.LogWarning(
                "Không trừ được thuốc trong kho. "
                + "Có thể kho không đủ thuốc."
            );
            return;
        }

        Debug.Log(
            "ĐÃ TRỪ THUỐC TRONG KHO THẬT."
        );

        ClinicQuestProgressService
            .RecordTreatmentProgress(
                patientCase.realDisease,
                isCurrentDiagnosisCorrect,
                isCurrentPrescriptionCorrect
            );

        pendingPatientMailData =
            ClinicPatientMailService
                .PreparePatientMail(
                    currentPatient.gameObject,
                    patientCase.selectedDisease,
                    patientCase.realDisease,
                    selectedPrescription,
                    isCurrentDiagnosisCorrect,
                    isCurrentPrescriptionCorrect
                );

        ClinicYThuUsageRewardService
            .ApplyRewardOrPenalty(
                pendingPatientMailData
            );

        shouldReturnCurrentPatientToQueueOnExit =
            false;

        currentStage =
            ClinicExamStage.PatientReceivingMedicine;

        isClinicUiTemporarilyClosed = false;
        restoredClinicUiNeedsReopen = false;

        StartCoroutine(
            PatientReceiveMedicineAndLeave()
        );
    }

    private IEnumerator
        PatientReceiveMedicineAndLeave()
    {
        if (currentPatient == null)
        {
            Debug.LogWarning(
                "Không có NPC bệnh nhân để nhận thuốc."
            );
            yield break;
        }

        DiseaseData diseaseForIcon =
            GetDiseaseForMedicineIcon();

        if (medicineCounterDisplay != null)
        {
            medicineCounterDisplay
                .ShowForDisease(
                    diseaseForIcon
                );
        }
        else
        {
            Debug.LogWarning(
                "Chưa kéo MedicineCounterDisplay "
                + "vào ClinicExamManager."
            );
        }

        Debug.Log(
            "Bệnh nhân đang nhận thuốc."
        );

        yield return new WaitForSeconds(
            receiveMedicineTime
        );

        if (medicineCounterDisplay != null)
        {
            medicineCounterDisplay.Hide();
        }

        currentPatient.LeaveClinic(
            npcLeavePoints,
            FinishCurrentPatient
        );
    }

    private DiseaseData GetDiseaseForMedicineIcon()
    {
        if (currentPatient == null)
            return null;

        PatientCase patientCase =
            currentPatient.PatientCase;

        if (patientCase == null)
            return null;

        if (patientCase.selectedDisease != null)
            return patientCase.selectedDisease;

        return patientCase.realDisease;
    }

    private void FinishCurrentPatient()
    {
        if (currentPatient != null)
        {
            Destroy(
                currentPatient.gameObject
            );
        }

        ClinicPatientMailService
            .SendPatientMail(
                pendingPatientMailData
            );

        pendingPatientMailData = null;

        currentPatient = null;
        currentVisitData = null;
        canStartExam = false;
        isExamRunning = false;
        isCurrentDiagnosisCorrect = false;
        isCurrentPrescriptionCorrect = false;

        shouldReturnCurrentPatientToQueueOnExit =
            false;

        isClinicUiTemporarilyClosed = false;
        restoredClinicUiNeedsReopen = false;

        currentStage =
            ClinicExamStage.None;

        float delay;

        if (
            PatientVisitManager.Instance != null
            && PatientVisitManager
                .Instance
                .HasWaitingPatient
        )
        {
            delay = queuedPatientEnterDelay;

            Debug.Log(
                "Ca khám đã kết thúc. "
                + "Hàng chờ còn bệnh nhân, "
                + "người tiếp theo sẽ lên sau "
                + delay.ToString("0.0")
                + " giây."
            );
        }
        else
        {
            delay =
                GetRandomNextPatientDelay();

            Debug.Log(
                "Ca khám đã kết thúc. "
                + "Hàng chờ trống, phòng khám nghỉ "
                + delay.ToString("0.0")
                + " giây rồi mới nhận bệnh nhân tiếp theo."
            );
        }

        ScheduleNextPatientEnter(delay);
    }

    private float GetRandomNextPatientDelay()
    {
        float minDelay =
            Mathf.Max(
                0f,
                minNextPatientDelay
            );

        float maxDelay =
            Mathf.Max(
                minDelay,
                maxNextPatientDelay
            );

        return Random.Range(
            minDelay,
            maxDelay
        );
    }

    private void ScheduleNextPatientEnter(
        float delay
    )
    {
        delay = Mathf.Max(0f, delay);

        nextPatientEnterTime =
            Time.time + delay;

        if (delay > 0f)
        {
            Debug.Log(
                "Bệnh nhân tiếp theo có thể vào sau "
                + delay.ToString("0.0")
                + " giây."
            );
        }
    }

    private void ReturnCurrentPatientToQueueIfNeeded()
    {
        if (!Application.isPlaying)
            return;

        if (applicationQuitting)
            return;

        if (!shouldReturnCurrentPatientToQueueOnExit)
            return;

        if (currentVisitData == null)
            return;

        if (
            currentVisitData.patientCase == null
            || currentVisitData
                .patientCase
                .realDisease == null
        )
        {
            return;
        }

        if (PatientVisitManager.Instance == null)
        {
            Debug.LogWarning(
                "Player rời phòng khám nhưng "
                + "không có PatientVisitManager "
                + "để lưu phiên khám dở."
            );

            return;
        }

        ClinicExamStage stageToSave =
            currentStage;

        if (
            stageToSave
            == ClinicExamStage.None
        )
        {
            stageToSave =
                ClinicExamStage
                    .WaitingAtExamPoint;
        }

        // Lấy số lần mở Y thư trước khi tracker bị reset.
        int currentYThuOpenCount =
            ClinicYThuUsageRewardService
                .GetCurrentOpenCount();

        PatientVisitManager
            .Instance
            .SaveSuspendedClinicSession(
                currentVisitData,
                stageToSave,
                isCurrentDiagnosisCorrect,
                currentYThuOpenCount
            );

        Debug.Log(
            "Player rời phòng khám khi chưa chữa xong. "
            + "Đã lưu phiên khám dở: "
            + currentVisitData
                .patientCase
                .realDisease
                .diseaseName
            + ", Stage: "
            + stageToSave
            + ", số lần mở Y thư: "
            + currentYThuOpenCount
        );

        // Chỉ reset sau khi đã lưu openCount.
        ClinicYThuUsageRewardService
            .CancelTracking();

        currentVisitData = null;
        currentPatient = null;
        canStartExam = false;
        isExamRunning = false;
        isCurrentDiagnosisCorrect = false;
        isCurrentPrescriptionCorrect = false;

        pendingPatientMailData = null;

        shouldReturnCurrentPatientToQueueOnExit =
            false;

        isClinicUiTemporarilyClosed = false;
        restoredClinicUiNeedsReopen = false;

        currentStage =
            ClinicExamStage.None;
    }

    private void OnDestroy()
    {
        ReturnCurrentPatientToQueueIfNeeded();
    }

    private void OnApplicationQuit()
    {
        applicationQuitting = true;
    }
}

