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

        PatientVisitData restoredVisitData = PatientVisitManager.Instance.TakeSuspendedClinicSession(
            out restoredStage,
            out restoredDiagnosisCorrect
        );

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

            Debug.Log("Đã khôi phục phiên khám dở. Bấm " + examineKey + " để mở lại UI. Stage: " + currentStage);
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

        if (currentPatient == null)
        {
            Debug.LogError("Không có NPC bệnh nhân hiện tại.");
            isExamRunning = false;
            currentStage = ClinicExamStage.None;
            yield break;
        }

        PatientCase patientCase = currentPatient.PatientCase;

        if (patientCase == null || patientCase.realDisease == null)
        {
            Debug.LogError("NPC bệnh nhân không có ca bệnh.");
            isExamRunning = false;
            currentStage = ClinicExamStage.None;
            yield break;
        }

        if (medicalDatabase == null)
        {
            Debug.LogError("Chưa kéo MedicalDatabase vào ClinicExamManager.");
            isExamRunning = false;
            currentStage = ClinicExamStage.None;
            yield break;
        }

        if (diagnosisUIController == null)
        {
            Debug.LogError("Chưa kéo DiagnosisUIController vào ClinicExamManager.");
            isExamRunning = false;
            currentStage = ClinicExamStage.None;
            yield break;
        }

        diagnosisUIController.gameObject.SetActive(true);

        diagnosisUIController.Show(
            patientCase,
            medicalDatabase,
            clinicLevel,
            OnDiseaseSelected
        );
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
                diagnosisUIController.gameObject.SetActive(false);
                isClinicUiTemporarilyClosed = true;
                restoredClinicUiNeedsReopen = false;

                Debug.Log("Đã ẩn tạm UI chẩn đoán. Bấm " + examineKey + " để mở lại.");
            }

            return;
        }

        if (currentStage == ClinicExamStage.Prescribing)
        {
            if (prescriptionUIController != null)
            {
                prescriptionUIController.gameObject.SetActive(false);
                isClinicUiTemporarilyClosed = true;
                restoredClinicUiNeedsReopen = false;

                Debug.Log("Đã ẩn tạm UI bốc thuốc. Bấm " + examineKey + " để mở lại.");
            }

            return;
        }
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

            /*
             * Nếu chỉ bấm X trong cùng scene:
             * restoredClinicUiNeedsReopen = false
             * => chỉ bật lại panel, không gọi Show(), không reset.
             *
             * Nếu đi ra ngoài scene rồi quay lại:
             * UI cũ đã mất, restoredClinicUiNeedsReopen = true
             * => phải gọi Show() lại để dựng UI.
             */
            if (restoredClinicUiNeedsReopen)
            {
                diagnosisUIController.Show(
                    patientCase,
                    medicalDatabase,
                    clinicLevel,
                    OnDiseaseSelected
                );
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

            /*
             * Nếu chỉ bấm X trong cùng scene thì không gọi Show() lại,
             * để giữ thuốc đang chọn.
             *
             * Nếu quay lại từ scene khác thì UI phải dựng lại.
             */
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
}