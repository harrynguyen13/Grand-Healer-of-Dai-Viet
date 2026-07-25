using System.Collections;
using UnityEngine;

public partial class ClinicExamManager
{
    private void ReceiveWaitingPatient()
    {
        if (PatientVisitManager.Instance == null)
        {
            Debug.LogWarning("Không có PatientVisitManager.");
            return;
        }

        if (!PatientVisitManager.Instance.HasWaitingPatient)
        {
            Debug.Log("Không có NPC bệnh nhân nào đang chờ.");
            return;
        }

        currentVisitData = PatientVisitManager.Instance.TakeWaitingPatient();

        if (currentVisitData == null)
        {
            Debug.LogWarning("Không lấy được PatientVisitData.");
            return;
        }

        if (!CreateCurrentPatientAtExamPoint(currentVisitData))
        {
            return;
        }

        canStartExam = true;
        isExamRunning = false;
        isCurrentDiagnosisCorrect = false;

        currentStage = ClinicExamStage.WaitingAtExamPoint;
        isClinicUiTemporarilyClosed = false;
        restoredClinicUiNeedsReopen = false;

        shouldReturnCurrentPatientToQueueOnExit = true;

        Debug.Log("Đã tạo đúng NPC bệnh nhân trong phòng khám.");
        Debug.Log("Bệnh thật: " + currentVisitData.patientCase.realDisease.diseaseName);
        Debug.Log("Player hãy đứng vào PlayerExamPoint rồi bấm " + examineKey + " để hỏi bệnh.");
    }

    private bool TryRestoreSuspendedClinicSession()
    {
        if (PatientVisitManager.Instance == null)
            return false;

        if (!PatientVisitManager.Instance.HasSuspendedClinicSession)
            return false;

        ClinicExamStage restoredStage;
        bool restoredDiagnosisCorrect;
        int restoredYThuOpenCount;

        PatientVisitData restoredVisitData = PatientVisitManager.Instance.TakeSuspendedClinicSession(
            out restoredStage,
            out restoredDiagnosisCorrect,
            out restoredYThuOpenCount);

        if (restoredVisitData == null)
            return false;

        currentVisitData = restoredVisitData;

        if (!CreateCurrentPatientAtExamPoint(currentVisitData))
        {
            currentVisitData = null;
            return false;
        }

        canStartExam = true;
        isCurrentDiagnosisCorrect = restoredDiagnosisCorrect;
        shouldReturnCurrentPatientToQueueOnExit = true;

        currentStage = restoredStage;

        if (currentStage == ClinicExamStage.Diagnosing ||
            currentStage == ClinicExamStage.Prescribing)
        {
            isExamRunning = true;
            isClinicUiTemporarilyClosed = true;
            restoredClinicUiNeedsReopen = true;

        ClinicYThuUsageRewardService.BeginTracking(
            restoredYThuOpenCount
        );

        Debug.Log(
            "Đã khôi phục phiên khám dở. Bấm "
            + examineKey
            + " để mở lại UI. Stage: "
            + currentStage
            + ", số lần mở Y thư: "
            + restoredYThuOpenCount
        );
        }
        else
        {
            isExamRunning = false;
            isClinicUiTemporarilyClosed = false;
            restoredClinicUiNeedsReopen = false;

            currentStage = ClinicExamStage.WaitingAtExamPoint;

            Debug.Log("Đã khôi phục bệnh nhân ở quầy khám. Bấm " + examineKey + " để khám tiếp.");
        }

        Debug.Log("Bệnh được khôi phục: " + currentVisitData.patientCase.realDisease.diseaseName);

        return true;
    }

    private bool CreateCurrentPatientAtExamPoint(PatientVisitData visitData)
    {
        if (visitData == null)
        {
            Debug.LogWarning("Không thể tạo NPC vì PatientVisitData null.");
            return false;
        }

        if (visitData.patientPrefab == null)
        {
            Debug.LogError("PatientVisitData không có patientPrefab.");
            return false;
        }

        if (visitData.patientCase == null || visitData.patientCase.realDisease == null)
        {
            Debug.LogError("PatientVisitData không có PatientCase hợp lệ.");
            return false;
        }

        if (npcExamPoint == null)
        {
            Debug.LogError("Chưa kéo NPCExamPoint vào ClinicExamManager.");
            return false;
        }

        if (playerExamPoint == null)
        {
            Debug.LogError("Chưa kéo PlayerExamPoint vào ClinicExamManager.");
            return false;
        }

        GameObject npcObject = Instantiate(
            visitData.patientPrefab,
            npcExamPoint.position,
            Quaternion.identity
        );

        currentPatient = npcObject.GetComponent<PatientController>();

        if (currentPatient == null)
        {
            Debug.LogError("Prefab NPC không có PatientController.");
            Destroy(npcObject);
            return false;
        }

        currentPatient.gameObject.SetActive(true);
        currentPatient.PrepareForClinicExam(visitData.patientCase);

        currentPatient.transform.position = npcExamPoint.position;

        Rigidbody2D rb2d = currentPatient.GetComponent<Rigidbody2D>();

        if (rb2d != null)
        {
            rb2d.position = npcExamPoint.position;
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
        }

        currentPatient.FaceToPosition(playerExamPoint.position);

        return true;
    }

    private IEnumerator StartExaminationFlow()
    {
        isExamRunning = true;
        isClinicUiTemporarilyClosed = false;
        restoredClinicUiNeedsReopen = false;
        currentStage = ClinicExamStage.Diagnosing;

        ClinicYThuUsageRewardService.BeginTracking();

        if (currentPatient == null)
        {
            Debug.LogError("Không có NPC bệnh nhân hiện tại.");
            isExamRunning = false;
            currentStage = ClinicExamStage.None;
            ClinicYThuUsageRewardService.CancelTracking();
            yield break;
        }

        PatientCase patientCase = currentPatient.PatientCase;

        if (patientCase == null || patientCase.realDisease == null)
        {
            Debug.LogError("NPC bệnh nhân không có ca bệnh.");
            isExamRunning = false;
            currentStage = ClinicExamStage.None;
            ClinicYThuUsageRewardService.CancelTracking();
            yield break;
        }

        if (medicalDatabase == null)
        {
            Debug.LogError("Chưa kéo MedicalDatabase vào ClinicExamManager.");
            isExamRunning = false;
            currentStage = ClinicExamStage.None;
            ClinicYThuUsageRewardService.CancelTracking();
            yield break;
        }

        if (diagnosisUIController == null)
        {
            Debug.LogError("Chưa kéo DiagnosisUIController vào ClinicExamManager.");
            isExamRunning = false;
            currentStage = ClinicExamStage.None;
            ClinicYThuUsageRewardService.CancelTracking();
            yield break;
        }

        int currentClinicLevel = GetCurrentClinicLevel();

        diagnosisUIController.gameObject.SetActive(true);
        SetClinicUiVisibleWithoutDisabling(diagnosisUIController.gameObject, true);

        diagnosisUIController.Show(
            patientCase,
            medicalDatabase,
            OnDiseaseSelected
        );

        Debug.Log("Mở UI chẩn đoán theo cấp hiện tại: " + currentClinicLevel);
    }

    private void OnDiseaseSelected(DiseaseData selectedDisease)
    {
        if (currentPatient == null)
        {
            Debug.LogError("Không có NPC bệnh nhân hiện tại.");
            return;
        }

        if (selectedDisease == null)
        {
            Debug.LogError("Bệnh được chọn bị null.");
            return;
        }

        PatientCase patientCase = currentPatient.PatientCase;

        if (patientCase == null || patientCase.realDisease == null)
        {
            Debug.LogError("Không có PatientCase hoặc bệnh thật.");
            return;
        }

        patientCase.selectedDisease = selectedDisease;

        Debug.Log("===== PLAYER ĐÃ CHỌN BỆNH =====");
        Debug.Log("Bệnh player chọn: " + selectedDisease.diseaseName);
        Debug.Log("Bệnh thật: " + patientCase.realDisease.diseaseName);

        if (selectedDisease == patientCase.realDisease)
        {
            isCurrentDiagnosisCorrect = true;
            Debug.Log("Kết quả đoán bệnh: ĐÚNG");
        }
        else
        {
            isCurrentDiagnosisCorrect = false;
            Debug.Log("Kết quả đoán bệnh: SAI");
        }

        if (prescriptionUIController == null)
        {
            Debug.LogError("Chưa kéo PrescriptionUIController vào ClinicExamManager.");
            return;
        }

        currentStage = ClinicExamStage.Prescribing;
        isClinicUiTemporarilyClosed = false;
        restoredClinicUiNeedsReopen = false;

        Debug.Log("Mở UI bốc thuốc.");

        prescriptionUIController.gameObject.SetActive(true);
        SetClinicUiVisibleWithoutDisabling(prescriptionUIController.gameObject, true);

        prescriptionUIController.Show(OnPrescriptionConfirmed);
    }

    public void CloseCurrentClinicUiTemporarily()
    {
        if (!isExamRunning)
            return;

        if (isClinicUiTemporarilyClosed)
            return;

        if (currentStage == ClinicExamStage.Diagnosing)
        {
            if (diagnosisUIController != null)
            {
                SetClinicUiVisibleWithoutDisabling(diagnosisUIController.gameObject, false);

                isClinicUiTemporarilyClosed = true;
                restoredClinicUiNeedsReopen = false;

                Debug.Log("Đã tạm ẩn UI chẩn đoán. Bấm " + examineKey + " để mở lại.");
            }

            return;
        }

        if (currentStage == ClinicExamStage.Prescribing)
        {
            if (prescriptionUIController != null)
            {
                SetClinicUiVisibleWithoutDisabling(prescriptionUIController.gameObject, false);

                if (medicineCounterDisplay != null)
                {
                    medicineCounterDisplay.Hide();
                }

                isClinicUiTemporarilyClosed = true;
                restoredClinicUiNeedsReopen = false;

                Debug.Log("Đã tạm ẩn UI bốc thuốc. Bấm " + examineKey + " để mở lại.");
            }

            return;
        }
    }

    public void CloseClinicUiTemporarilyByButton()
    {
        CloseCurrentClinicUiTemporarily();
    }

    private void ResumeTemporarilyClosedClinicUi()
    {
        if (!isClinicUiTemporarilyClosed)
            return;

        if (currentPatient == null)
            return;

        PatientCase patientCase = currentPatient.PatientCase;

        if (patientCase == null || patientCase.realDisease == null)
            return;

        if (currentStage == ClinicExamStage.Diagnosing)
        {
            if (diagnosisUIController == null)
                return;

            diagnosisUIController.gameObject.SetActive(true);
            SetClinicUiVisibleWithoutDisabling(diagnosisUIController.gameObject, true);

            if (restoredClinicUiNeedsReopen)
            {
                int currentClinicLevel = GetCurrentClinicLevel();

                diagnosisUIController.Show(
                    patientCase,
                    medicalDatabase,
                    OnDiseaseSelected
                );

                Debug.Log("Dựng lại UI chẩn đoán theo cấp hiện tại: " + currentClinicLevel);
            }

            isClinicUiTemporarilyClosed = false;
            restoredClinicUiNeedsReopen = false;

            Debug.Log("Đã mở lại UI chẩn đoán đang dở.");
            return;
        }

        if (currentStage == ClinicExamStage.Prescribing)
        {
            if (prescriptionUIController == null)
                return;

            prescriptionUIController.gameObject.SetActive(true);
            SetClinicUiVisibleWithoutDisabling(prescriptionUIController.gameObject, true);

            if (restoredClinicUiNeedsReopen)
            {
                prescriptionUIController.Show(OnPrescriptionConfirmed);
            }

            isClinicUiTemporarilyClosed = false;
            restoredClinicUiNeedsReopen = false;

            Debug.Log("Đã mở lại UI bốc thuốc đang dở.");
            return;
        }
    }

    private void SetClinicUiVisibleWithoutDisabling(GameObject targetObject, bool visible)
    {
        if (targetObject == null)
            return;

        if (visible && !targetObject.activeSelf)
        {
            targetObject.SetActive(true);
        }

        CanvasGroup canvasGroup = targetObject.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = targetObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;

        RefreshPlayerLocksAroundPanel(targetObject);
    }

    private void RefreshPlayerLocksAroundPanel(GameObject targetObject)
    {
        if (targetObject == null)
            return;

        PlayerControlLockByPanel[] childLocks =
            targetObject.GetComponentsInChildren<PlayerControlLockByPanel>(true);

        for (int i = 0; i < childLocks.Length; i++)
        {
            if (childLocks[i] != null)
            {
                childLocks[i].RefreshLockState();
            }
        }

        PlayerControlLockByPanel[] parentLocks =
            targetObject.GetComponentsInParent<PlayerControlLockByPanel>(true);

        for (int i = 0; i < parentLocks.Length; i++)
        {
            if (parentLocks[i] != null)
            {
                parentLocks[i].RefreshLockState();
            }
        }

        Debug.Log("Trạng thái khóa Player hiện tại: " + PlayerControlLock.GetDebugLockReasons());
    }

    private bool IsClinicUiHiddenByCanvasGroup(GameObject targetObject)
    {
        if (targetObject == null)
            return false;

        CanvasGroup canvasGroup = targetObject.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            return false;

        return canvasGroup.alpha <= 0.01f;
    }
}