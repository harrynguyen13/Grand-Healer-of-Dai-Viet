using UnityEngine;

public class ClinicPatientReceiver : MonoBehaviour
{
    [Header("Điểm NPC xuất hiện trong phòng thuốc")]
    [SerializeField] private Transform clinicInsideSpawnPoint;

    private void Start()
    {
        if (PatientVisitManager.Instance == null)
        {
            Debug.LogWarning("Không có PatientVisitManager.");
            return;
        }

        if (!PatientVisitManager.Instance.HasWaitingPatient)
        {
            Debug.Log("Không có NPC bệnh nhân nào đang chờ trong phòng thuốc.");
            return;
        }

        PatientController patient = PatientVisitManager.Instance.TakeWaitingPatient();

        if (patient == null)
        {
            Debug.LogWarning("Không lấy được NPC bệnh nhân.");
            return;
        }

        if (clinicInsideSpawnPoint == null)
        {
            Debug.LogError("Chưa kéo ClinicInsideSpawnPoint vào ClinicPatientReceiver.");
            return;
        }

        patient.gameObject.SetActive(true);
        patient.transform.position = clinicInsideSpawnPoint.position;

        Rigidbody2D rb2d = patient.GetComponent<Rigidbody2D>();

        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
        }

        Debug.Log("Đã đưa NPC bệnh nhân vào phòng thuốc: " + patient.name);
    }
}