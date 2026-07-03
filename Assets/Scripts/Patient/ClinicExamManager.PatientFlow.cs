using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public partial class ClinicExamManager
{
    private void OnPrescriptionConfirmed(Dictionary<HerbData, int> selectedPrescription)
    {
        Debug.Log("===== CLINIC NHẬN ĐƠN THUỐC =====");

        if (selectedPrescription == null || selectedPrescription.Count == 0)
        {
            Debug.LogWarning("Đơn thuốc rỗng.");
            return;
        }

        if (currentPatient == null || currentPatient.PatientCase == null)
        {
            Debug.LogError("Không có bệnh nhân hoặc PatientCase hiện tại.");
            return;
        }

        PatientCase patientCase = currentPatient.PatientCase;

        if (patientCase.realDisease == null)
        {
            Debug.LogError("PatientCase không có bệnh thật.");
            return;
        }

        foreach (KeyValuePair<HerbData, int> pair in selectedPrescription)
        {
            if (pair.Key == null)
                continue;

            Debug.Log("Thuốc đã kê: " + pair.Key.herbName + " x" + pair.Value);
        }

        isCurrentPrescriptionCorrect = IsPrescriptionCorrectForDisease(
            patientCase.realDisease,
            selectedPrescription
        );

        Debug.Log("===== KẾT QUẢ KIỂM TRA =====");
        Debug.Log("Bệnh thật: " + patientCase.realDisease.diseaseName);
        Debug.Log("Chẩn đoán đúng: " + isCurrentDiagnosisCorrect);
        Debug.Log("Đơn thuốc đúng: " + isCurrentPrescriptionCorrect);
        Debug.Log("Thuốc cần có: " + GetRequiredHerbNames(patientCase.realDisease));

        if (HerbInventory.Instance == null)
        {
            Debug.LogError("Không tìm thấy HerbInventory. Kiểm tra GameSystems ở LoginScene.");
            return;
        }

        bool removed = HerbInventory.Instance.RemovePrescription(selectedPrescription);

        if (!removed)
        {
            Debug.LogWarning("Không trừ được thuốc trong kho. Có thể kho không đủ thuốc.");
            return;
        }

        Debug.Log("ĐÃ TRỪ THUỐC TRONG KHO THẬT.");

        RecordQuestProgress(patientCase.realDisease);

        ApplyMoneyAndReputation(selectedPrescription);

        shouldReturnCurrentPatientToQueueOnExit = false;

        currentStage = ClinicExamStage.PatientReceivingMedicine;
        isClinicUiTemporarilyClosed = false;
        restoredClinicUiNeedsReopen = false;

        StartCoroutine(PatientReceiveMedicineAndLeave());
    }

    private void RecordQuestProgress(DiseaseData realDisease)
    {
        if (realDisease == null)
        {
            Debug.LogWarning("Không ghi nhiệm vụ được vì realDisease bị null.");
            return;
        }

        if (QuestProgressManager.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy QuestProgressManager để ghi tiến độ nhiệm vụ.");
            return;
        }

        QuestProgressManager.Instance.RecordTreatmentResult(
            realDisease,
            isCurrentDiagnosisCorrect,
            isCurrentPrescriptionCorrect
        );

        Debug.Log(
            "Đã ghi tiến độ nhiệm vụ: "
            + realDisease.diseaseName
            + " | Cấp bệnh: "
            + (int)realDisease.diseaseLevel
            + " | Chẩn đoán đúng: "
            + isCurrentDiagnosisCorrect
            + " | Kê đơn đúng: "
            + isCurrentPrescriptionCorrect
        );
    }

    private void ApplyMoneyAndReputation(Dictionary<HerbData, int> prescription)
    {
        int payment = CalculatePrescriptionPayment(prescription);
        int diseaseLevel = GetCurrentDiseaseLevel();

        int reputationChange;
        float paymentMultiplier;

        if (isCurrentDiagnosisCorrect && isCurrentPrescriptionCorrect)
        {
            reputationChange = GetCorrectTreatmentReward(diseaseLevel);
            paymentMultiplier = 1f;

            Debug.Log("Kết quả: ĐÚNG BỆNH + ĐÚNG THUỐC.");
        }
        else if (isCurrentDiagnosisCorrect && !isCurrentPrescriptionCorrect)
        {
            reputationChange = -GetWrongPrescriptionPenalty(diseaseLevel);
            paymentMultiplier = 0.4f;

            Debug.Log("Kết quả: ĐÚNG BỆNH nhưng SAI THUỐC.");
        }
        else if (!isCurrentDiagnosisCorrect && isCurrentPrescriptionCorrect)
        {
            reputationChange = -GetWrongDiagnosisPenalty(diseaseLevel);
            paymentMultiplier = 0.5f;

            Debug.Log("Kết quả: SAI BỆNH nhưng THUỐC ĐÚNG BỆNH THẬT.");
        }
        else
        {
            reputationChange = -GetWrongTreatmentPenalty(diseaseLevel);
            paymentMultiplier = 0.2f;

            Debug.Log("Kết quả: SAI BỆNH + SAI THUỐC.");
        }

        payment = Mathf.RoundToInt(payment * paymentMultiplier);

        if (payment < 1)
            payment = 1;

        if (PlayerEconomy.Instance != null)
        {
            PlayerEconomy.Instance.AddMoney(payment);
            PlayerEconomy.Instance.AddReputation(reputationChange);

            Debug.Log("Bệnh level: " + diseaseLevel);
            Debug.Log("Nhận tiền: " + payment);
            Debug.Log("Tín nhiệm thay đổi: " + reputationChange);
        }
        else
        {
            Debug.LogWarning("Không tìm thấy PlayerEconomy. Đã trừ thuốc nhưng chưa cộng tiền/tín nhiệm.");
        }
    }

    private int CalculatePrescriptionPayment(Dictionary<HerbData, int> prescription)
    {
        if (prescription == null)
            return 0;

        int total = 0;

        foreach (KeyValuePair<HerbData, int> pair in prescription)
        {
            HerbData herb = pair.Key;
            int quantity = pair.Value;

            if (herb == null || quantity <= 0)
                continue;

            total += herb.sellPrice * quantity;
        }

        return Mathf.Max(1, total);
    }

    private bool IsPrescriptionCorrectForDisease(
        DiseaseData disease,
        Dictionary<HerbData, int> selectedPrescription
    )
    {
        if (disease == null || disease.correctHerbs == null)
            return false;

        if (selectedPrescription == null)
            return false;

        HashSet<string> requiredHerbs = new HashSet<string>();

        foreach (HerbData herb in disease.correctHerbs)
        {
            if (herb == null)
                continue;

            string herbKey = GetHerbKey(herb);

            if (!string.IsNullOrEmpty(herbKey))
                requiredHerbs.Add(herbKey);
        }

        HashSet<string> selectedHerbs = new HashSet<string>();

        foreach (KeyValuePair<HerbData, int> pair in selectedPrescription)
        {
            HerbData herb = pair.Key;
            int quantity = pair.Value;

            if (herb == null || quantity <= 0)
                continue;

            string herbKey = GetHerbKey(herb);

            if (!string.IsNullOrEmpty(herbKey))
                selectedHerbs.Add(herbKey);
        }

        if (requiredHerbs.Count <= 0)
        {
            Debug.LogWarning("Bệnh này chưa có correctHerbs.");
            return false;
        }

        if (selectedHerbs.Count != requiredHerbs.Count)
        {
            Debug.Log("Sai số lượng vị thuốc. Cần: "
                + requiredHerbs.Count
                + ", đã kê: "
                + selectedHerbs.Count);

            return false;
        }

        foreach (string requiredHerb in requiredHerbs)
        {
            if (!selectedHerbs.Contains(requiredHerb))
            {
                Debug.Log("Thiếu hoặc sai vị thuốc: " + requiredHerb);
                return false;
            }
        }

        return true;
    }

    private string GetHerbKey(HerbData herb)
    {
        if (herb == null || string.IsNullOrWhiteSpace(herb.herbName))
            return string.Empty;

        return herb.herbName.Trim().ToLower();
    }

    private string GetRequiredHerbNames(DiseaseData disease)
    {
        if (disease == null || disease.correctHerbs == null)
            return "Không có dữ liệu thuốc.";

        List<string> herbNames = new List<string>();

        foreach (HerbData herb in disease.correctHerbs)
        {
            if (herb == null)
                continue;

            herbNames.Add(herb.herbName);
        }

        return string.Join(", ", herbNames);
    }

    private int GetCurrentDiseaseLevel()
    {
        if (currentPatient == null)
            return 1;

        PatientCase patientCase = currentPatient.PatientCase;

        if (patientCase == null || patientCase.realDisease == null)
            return 1;

        return Mathf.Max(1, (int)patientCase.realDisease.diseaseLevel);
    }

    private int GetCorrectTreatmentReward(int diseaseLevel)
    {
        if (diseaseLevel <= 1) return 10;
        if (diseaseLevel == 2) return 15;
        if (diseaseLevel == 3) return 22;
        if (diseaseLevel == 4) return 30;

        return 45;
    }

    private int GetWrongPrescriptionPenalty(int diseaseLevel)
    {
        if (diseaseLevel <= 1) return 3;
        if (diseaseLevel == 2) return 5;
        if (diseaseLevel == 3) return 8;
        if (diseaseLevel == 4) return 12;

        return 18;
    }

    private int GetWrongDiagnosisPenalty(int diseaseLevel)
    {
        if (diseaseLevel <= 1) return 2;
        if (diseaseLevel == 2) return 4;
        if (diseaseLevel == 3) return 6;
        if (diseaseLevel == 4) return 9;

        return 14;
    }

    private int GetWrongTreatmentPenalty(int diseaseLevel)
    {
        if (diseaseLevel <= 1) return 5;
        if (diseaseLevel == 2) return 8;
        if (diseaseLevel == 3) return 12;
        if (diseaseLevel == 4) return 18;

        return 25;
    }

    private IEnumerator PatientReceiveMedicineAndLeave()
    {
        if (currentPatient == null)
        {
            Debug.LogWarning("Không có NPC bệnh nhân để nhận thuốc.");
            yield break;
        }

        DiseaseData diseaseForIcon = GetDiseaseForMedicineIcon();

        if (medicineCounterDisplay != null)
        {
            medicineCounterDisplay.ShowForDisease(diseaseForIcon);
        }
        else
        {
            Debug.LogWarning("Chưa kéo MedicineCounterDisplay vào ClinicExamManager.");
        }

        Debug.Log("Bệnh nhân đang nhận thuốc.");

        yield return new WaitForSeconds(receiveMedicineTime);

        if (medicineCounterDisplay != null)
        {
            medicineCounterDisplay.Hide();
        }

        currentPatient.LeaveClinic(npcLeavePoints, FinishCurrentPatient);
    }

    private DiseaseData GetDiseaseForMedicineIcon()
    {
        if (currentPatient == null)
            return null;

        PatientCase patientCase = currentPatient.PatientCase;

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
            Destroy(currentPatient.gameObject);
        }

        currentPatient = null;
        currentVisitData = null;
        canStartExam = false;
        isExamRunning = false;
        isCurrentDiagnosisCorrect = false;
        isCurrentPrescriptionCorrect = false;

        shouldReturnCurrentPatientToQueueOnExit = false;
        isClinicUiTemporarilyClosed = false;
        restoredClinicUiNeedsReopen = false;
        currentStage = ClinicExamStage.None;

        float delay;

        if (PatientVisitManager.Instance != null && PatientVisitManager.Instance.HasWaitingPatient)
        {
            delay = queuedPatientEnterDelay;

            Debug.Log("Ca khám đã kết thúc. Hàng chờ còn bệnh nhân, người tiếp theo sẽ lên sau "
                + delay.ToString("0.0")
                + " giây.");
        }
        else
        {
            delay = GetRandomNextPatientDelay();

            Debug.Log("Ca khám đã kết thúc. Hàng chờ trống, phòng khám nghỉ "
                + delay.ToString("0.0")
                + " giây rồi mới nhận bệnh nhân tiếp theo.");
        }

        ScheduleNextPatientEnter(delay);
    }

    private float GetRandomNextPatientDelay()
    {
        float minDelay = Mathf.Max(0f, minNextPatientDelay);
        float maxDelay = Mathf.Max(minDelay, maxNextPatientDelay);

        return Random.Range(minDelay, maxDelay);
    }

    private void ScheduleNextPatientEnter(float delay)
    {
        delay = Mathf.Max(0f, delay);
        nextPatientEnterTime = Time.time + delay;

        if (delay > 0f)
        {
            Debug.Log("Bệnh nhân tiếp theo có thể vào sau " + delay.ToString("0.0") + " giây.");
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

        if (currentVisitData.patientCase == null || currentVisitData.patientCase.realDisease == null)
            return;

        if (PatientVisitManager.Instance == null)
        {
            Debug.LogWarning("Player rời phòng khám nhưng không có PatientVisitManager để lưu phiên khám dở.");
            return;
        }

        ClinicExamStage stageToSave = currentStage;

        if (stageToSave == ClinicExamStage.None)
        {
            stageToSave = ClinicExamStage.WaitingAtExamPoint;
        }

        PatientVisitManager.Instance.SaveSuspendedClinicSession(
            currentVisitData,
            stageToSave,
            isCurrentDiagnosisCorrect
        );

        Debug.Log("Player rời phòng khám khi chưa chữa xong. Đã lưu phiên khám dở: "
            + currentVisitData.patientCase.realDisease.diseaseName
            + ", Stage: "
            + stageToSave);

        currentVisitData = null;
        currentPatient = null;
        canStartExam = false;
        isExamRunning = false;
        isCurrentDiagnosisCorrect = false;
        isCurrentPrescriptionCorrect = false;

        shouldReturnCurrentPatientToQueueOnExit = false;
        isClinicUiTemporarilyClosed = false;
        restoredClinicUiNeedsReopen = false;
        currentStage = ClinicExamStage.None;
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