using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class ClinicExamManager : MonoBehaviour
{
    [Header("Điểm trong phòng khám")]
    [SerializeField] private Transform npcExamPoint;
    [SerializeField] private Transform playerExamPoint;

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

    [Header("Cấp y quán")]
    [SerializeField] private int clinicLevel = 1;

    private PatientController currentPatient;
    private Transform player;
    private bool canStartExam;
    private bool isExamRunning;

    private void Start()
    {
        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
        {
            player = playerObject.transform;
        }

        ReceiveWaitingPatient();
    }

    private void Update()
    {
        if (currentPatient == null)
            return;

        if (isExamRunning)
            return;

        CheckPlayerReady();
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

        currentPatient = PatientVisitManager.Instance.TakeWaitingPatient();

        if (currentPatient == null)
        {
            Debug.LogWarning("Không lấy được NPC bệnh nhân.");
            return;
        }

        currentPatient.gameObject.SetActive(true);

        if (npcExamPoint == null)
        {
            Debug.LogError("Chưa kéo NPCExamPoint vào ClinicExamManager.");
            return;
        }

        if (playerExamPoint == null)
        {
            Debug.LogError("Chưa kéo PlayerExamPoint vào ClinicExamManager.");
            return;
        }

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

        Debug.Log("NPC bệnh nhân đã đứng đúng điểm khám.");
        Debug.Log("Player hãy đứng vào PlayerExamPoint rồi bấm " + examineKey + " để hỏi bệnh.");
    }

    private void CheckPlayerReady()
    {
        if (!canStartExam)
            return;

        if (player == null)
        {
            GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

            if (playerObject != null)
            {
                player = playerObject.transform;
            }
        }

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
            yield break;
        }

        PatientCase patientCase = currentPatient.PatientCase;

        if (patientCase == null || patientCase.realDisease == null)
        {
            Debug.LogError("NPC bệnh nhân không có ca bệnh.");
            yield break;
        }

        if (medicalDatabase == null)
        {
            Debug.LogError("Chưa kéo MedicalDatabase vào ClinicExamManager.");
            yield break;
        }

        if (diagnosisUIController == null)
        {
            Debug.LogError("Chưa kéo DiagnosisUIController vào ClinicExamManager.");
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

        if (patientCase == null)
        {
            Debug.LogError("Không có PatientCase.");
            return;
        }

        patientCase.selectedDisease = selectedDisease;

        Debug.Log("===== PLAYER ĐÃ CHỌN BỆNH =====");
        Debug.Log("Bệnh player chọn: " + selectedDisease.diseaseName);
        Debug.Log("Bệnh thật: " + patientCase.realDisease.diseaseName);

        if (selectedDisease == patientCase.realDisease)
        {
            Debug.Log("Kết quả đoán bệnh: ĐÚNG");
        }
        else
        {
            Debug.Log("Kết quả đoán bệnh: SAI");
        }

        if (prescriptionUIController == null)
        {
            Debug.LogError("Chưa kéo PrescriptionUIController vào ClinicExamManager.");
            return;
        }

        Debug.Log("Mở UI bốc thuốc.");

        prescriptionUIController.Show();
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
    }
}