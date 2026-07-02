using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.InputSystem;

public partial class ClinicExamManager : MonoBehaviour
{
    [Header("Điểm trong phòng khám")]
    [SerializeField] private Transform npcExamPoint;
    [SerializeField] private Transform playerExamPoint;

    [Header("Điểm NPC rời phòng")]
    [SerializeField] private Transform[] npcLeavePoints;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float playerExamDistance = 1f;

    [Header("Phím bắt đầu / mở lại khám")]
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

    [Tooltip("Nếu trong hàng chờ còn bệnh nhân, người tiếp theo sẽ lên sau thời gian này.")]
    [SerializeField] private float queuedPatientEnterDelay = 1f;

    [Tooltip("Nếu hàng chờ trống, phòng khám nghỉ tối thiểu từng này giây.")]
    [SerializeField] private float minNextPatientDelay = 12f;

    [Tooltip("Nếu hàng chờ trống, phòng khám nghỉ tối đa từng này giây.")]
    [SerializeField] private float maxNextPatientDelay = 20f;

    [Header("Cấp y quán")]
    [SerializeField] private int clinicLevel = 1;

    private PatientVisitData currentVisitData;
    private PatientController currentPatient;
    private Transform player;

    public bool HasCurrentPatient
    {
        get { return currentPatient != null; }
    }

    private bool canStartExam;
    private bool isExamRunning;
    private bool isCurrentDiagnosisCorrect;
    private bool isCurrentPrescriptionCorrect; 

    private bool shouldReturnCurrentPatientToQueueOnExit;
    private bool applicationQuitting;

    private bool isClinicUiTemporarilyClosed;

    /*
     * Dùng khi player đang khám dở / bốc thuốc dở rồi đi ra ngoài scene.
     * Khi quay lại, UI cũ đã bị destroy nên phải gọi Show() lại.
     */
    private bool restoredClinicUiNeedsReopen;

    private ClinicExamStage currentStage = ClinicExamStage.None;

    private float nextPatientEnterTime;
}