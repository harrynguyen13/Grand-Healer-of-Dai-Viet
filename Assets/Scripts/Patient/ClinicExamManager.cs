using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClinicExamManager : MonoBehaviour
{
    [Header("Điểm trong phòng khám")]
    [SerializeField] private Transform npcExamPoint;
    [SerializeField] private Transform playerExamPoint;

    [Header("Điểm NPC rời phòng")]
    [SerializeField] private Transform[] npcLeavePoints;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float playerExamDistance = 1f;

    [Header("Phím bắt đầu khám")]
    [SerializeField] private Key examineKey = Key.F;

    [Header("Database")]
    [SerializeField] private MedicalDatabase medicalDatabase;

    [Header("UI chẩn đoán")]
    [SerializeField] private DiagnosisUIController diagnosisUIController;

    [Header("UI bốc thuốc")]
    [SerializeField] private PrescriptionUIController prescriptionUIController;

    [Header("Hiển thị thuốc trên quầy")]
    [SerializeField] private MedicineCounterDisplay medicineCounterDisplay;

    [Header("Thời gian bệnh nhân nhận thuốc")]
    [SerializeField] private float receiveMedicineTime = 0.8f;

    [Header("Thời gian nghỉ giữa 2 bệnh nhân")]
    [SerializeField] private float firstPatientEnterDelay = 2f;
    [SerializeField] private float minNextPatientDelay = 12f;
    [SerializeField] private float maxNextPatientDelay = 20f;

    [Header("Cấp y quán")]
    [SerializeField] private int clinicLevel = 1;

    [Header("Thưởng / phạt")]
    [SerializeField] private int correctDiagnosisReputationReward = 10;
    [SerializeField] private int wrongDiagnosisReputationPenalty = -5;
    [SerializeField] private float wrongDiagnosisPaymentMultiplier = 0.5f;

    private PatientVisitData currentVisitData;
    private PatientController currentPatient;
    private Transform player;

    private bool canStartExam;
    private bool isExamRunning;
    private bool isCurrentDiagnosisCorrect;

    private float nextPatientEnterTime;

    private void Start()
    {
        if (medicineCounterDisplay != null)
        {
            medicineCounterDisplay.Hide();
        }

        FindPlayerIfMissing();

        ScheduleNextPatientEnter(firstPatientEnterDelay);
    }

    private void Update()
    {
        if (currentPatient == null)
        {
            if (Time.time >= nextPatientEnterTime)
            {
                TryReceiveWaitingPatient();
            }

            return;
        }

        if (isExamRunning)
            return;

        CheckPlayerReady();
    }

    private void TryReceiveWaitingPatient()
    {
        if (currentPatient != null)
            return;

        if (PatientVisitManager.Instance == null)
            return;

        if (!PatientVisitManager.Instance.HasWaitingPatient)
            return;

        ReceiveWaitingPatient();
    }

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

        if (currentVisitData.patientPrefab == null)
        {
            Debug.LogError("PatientVisitData không có patientPrefab.");
            currentVisitData = null;
            return;
        }

        if (currentVisitData.patientCase == null || currentVisitData.patientCase.realDisease == null)
        {
            Debug.LogError("PatientVisitData không có PatientCase hợp lệ.");
            currentVisitData = null;
            return;
        }

        if (npcExamPoint == null)
        {
            Debug.LogError("Chưa kéo NPCExamPoint vào ClinicExamManager.");
            currentVisitData = null;
            return;
        }

        if (playerExamPoint == null)
        {
            Debug.LogError("Chưa kéo PlayerExamPoint vào ClinicExamManager.");
            currentVisitData = null;
            return;
        }

        GameObject npcObject = Instantiate(
            currentVisitData.patientPrefab,
            npcExamPoint.position,
            Quaternion.identity
        );

        currentPatient = npcObject.GetComponent<PatientController>();

        if (currentPatient == null)
        {
            Debug.LogError("Prefab NPC không có PatientController.");
            Destroy(npcObject);
            currentVisitData = null;
            return;
        }

        currentPatient.gameObject.SetActive(true);
        currentPatient.PrepareForClinicExam(currentVisitData.patientCase);

        currentPatient.transform.position = npcExamPoint.position;

        Rigidbody2D rb2d = currentPatient.GetComponent<Rigidbody2D>();

        if (rb2d != null)
        {
            rb2d.position = npcExamPoint.position;
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
        }

        currentPatient.FaceToPosition(playerExamPoint.position);

        canStartExam = true;
        isExamRunning = false;
        isCurrentDiagnosisCorrect = false;

        Debug.Log("Đã tạo đúng NPC bệnh nhân trong phòng khám.");
        Debug.Log("Bệnh thật: " + currentVisitData.patientCase.realDisease.diseaseName);
        Debug.Log("Player hãy đứng vào PlayerExamPoint rồi bấm " + examineKey + " để hỏi bệnh.");
    }

    private void CheckPlayerReady()
    {
        if (!canStartExam)
            return;

        FindPlayerIfMissing();

        if (player == null)
            return;

        if (playerExamPoint == null)
        {
            Debug.LogError("Chưa kéo PlayerExamPoint vào ClinicExamManager.");
            return;
        }

        float distance = Vector2.Distance(player.position, playerExamPoint.position);

        if (distance > playerExamDistance)
            return;

        if (Keyboard.current != null && Keyboard.current[examineKey].wasPressedThisFrame)
        {
            StartCoroutine(StartExaminationFlow());
        }
    }

    private IEnumerator StartExaminationFlow()
    {
        isExamRunning = true;

        if (currentPatient == null)
        {
            Debug.LogError("Không có NPC bệnh nhân hiện tại.");
            isExamRunning = false;
            yield break;
        }

        PatientCase patientCase = currentPatient.PatientCase;

        if (patientCase == null || patientCase.realDisease == null)
        {
            Debug.LogError("NPC bệnh nhân không có ca bệnh.");
            isExamRunning = false;
            yield break;
        }

        if (medicalDatabase == null)
        {
            Debug.LogError("Chưa kéo MedicalDatabase vào ClinicExamManager.");
            isExamRunning = false;
            yield break;
        }

        if (diagnosisUIController == null)
        {
            Debug.LogError("Chưa kéo DiagnosisUIController vào ClinicExamManager.");
            isExamRunning = false;
            yield break;
        }

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

        Debug.Log("Mở UI bốc thuốc.");

        prescriptionUIController.Show(OnPrescriptionConfirmed);
    }

    private void OnPrescriptionConfirmed(Dictionary<HerbData, int> selectedPrescription)
    {
        Debug.Log("===== CLINIC NHẬN ĐƠN THUỐC =====");

        if (selectedPrescription == null || selectedPrescription.Count == 0)
        {
            Debug.LogWarning("Đơn thuốc rỗng.");
            return;
        }

        foreach (KeyValuePair<HerbData, int> pair in selectedPrescription)
        {
            if (pair.Key == null)
                continue;

            Debug.Log("Thuốc đã kê: " + pair.Key.herbName + " x" + pair.Value);
        }

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

        ApplyMoneyAndReputation(selectedPrescription);

        StartCoroutine(PatientReceiveMedicineAndLeave());
    }

    private void ApplyMoneyAndReputation(Dictionary<HerbData, int> prescription)
    {
        int payment = CalculatePrescriptionPayment(prescription);

        if (!isCurrentDiagnosisCorrect)
        {
            payment = Mathf.RoundToInt(payment * wrongDiagnosisPaymentMultiplier);
        }

        if (payment < 1)
            payment = 1;

        if (PlayerEconomy.Instance != null)
        {
            PlayerEconomy.Instance.AddMoney(payment);

            if (isCurrentDiagnosisCorrect)
            {
                PlayerEconomy.Instance.AddReputation(correctDiagnosisReputationReward);
                Debug.Log("Điều trị đúng. Nhận tiền: " + payment + ", tín nhiệm +" + correctDiagnosisReputationReward);
            }
            else
            {
                PlayerEconomy.Instance.AddReputation(wrongDiagnosisReputationPenalty);
                Debug.Log("Chẩn đoán sai. Nhận tiền giảm còn: " + payment + ", tín nhiệm " + wrongDiagnosisReputationPenalty);
            }
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

        float delay = GetRandomNextPatientDelay();
        ScheduleNextPatientEnter(delay);

        Debug.Log("Ca khám đã kết thúc. Phòng khám nghỉ " + delay.ToString("0.0") + " giây rồi mới nhận bệnh nhân tiếp theo.");
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

    private void FindPlayerIfMissing()
    {
        if (player != null)
            return;

        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (playerExamPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(playerExamPoint.position, playerExamDistance);
        }

        if (npcExamPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(npcExamPoint.position, 0.35f);
        }

        if (npcLeavePoints != null)
        {
            Gizmos.color = Color.cyan;

            foreach (Transform point in npcLeavePoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.15f);
                }
            }
        }
    }
}