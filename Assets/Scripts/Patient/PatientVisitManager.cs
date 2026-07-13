using System.Collections.Generic;
using UnityEngine;

public class PatientVisitManager : MonoBehaviour
{
    public static PatientVisitManager Instance { get; private set; }

    [Header("Số bệnh nhân chờ tối đa")]
    [SerializeField] private int maxWaitingPatients = 3;

    private readonly Queue<PatientVisitData> waitingPatients = new Queue<PatientVisitData>();

    private PatientVisitData suspendedClinicVisitData;
    private ClinicExamStage suspendedClinicStage = ClinicExamStage.None;
    private bool suspendedDiagnosisCorrect;

    public bool HasWaitingPatient
    {
        get { return waitingPatients.Count > 0; }
    }

    public int WaitingCount
    {
        get { return waitingPatients.Count; }
    }

    public int MaxWaitingPatients
    {
        get { return maxWaitingPatients; }
    }

    public bool CanAcceptMorePatients
    {
        get { return waitingPatients.Count < maxWaitingPatients; }
    }

    public bool HasSuspendedClinicSession
    {
        get { return suspendedClinicVisitData != null; }
    }


    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool AddWaitingPatient(GameObject patientPrefab, PatientCase patientCase)
    {
        if (patientPrefab == null)
        {
            Debug.LogWarning("Không thể thêm bệnh nhân chờ vì patientPrefab null.");
            return false;
        }

        if (patientCase == null || patientCase.realDisease == null)
        {
            Debug.LogWarning("Không thể thêm bệnh nhân chờ vì PatientCase null hoặc chưa có bệnh.");
            return false;
        }

        if (waitingPatients.Count >= maxWaitingPatients)
        {
            Debug.LogWarning("Hàng đợi bệnh nhân đã đầy: " + waitingPatients.Count + "/" + maxWaitingPatients);
            return false;
        }

        PatientVisitData visitData = new PatientVisitData(patientPrefab, patientCase);
        waitingPatients.Enqueue(visitData);

        Debug.Log("Đã thêm bệnh nhân vào hàng đợi. Bệnh: "
            + patientCase.realDisease.diseaseName
            + ". Số người chờ: "
            + waitingPatients.Count
            + "/"
            + maxWaitingPatients);

        return true;
    }

    public PatientVisitData TakeWaitingPatient()
    {
        if (waitingPatients.Count <= 0)
        {
            Debug.Log("Không có bệnh nhân nào trong hàng đợi.");
            return null;
        }

        PatientVisitData visitData = waitingPatients.Dequeue();

        if (visitData != null && visitData.patientCase != null && visitData.patientCase.realDisease != null)
        {
            Debug.Log("Lấy bệnh nhân khỏi hàng đợi. Bệnh: "
                + visitData.patientCase.realDisease.diseaseName
                + ". Còn chờ: "
                + waitingPatients.Count
                + "/"
                + maxWaitingPatients);
        }

        return visitData;
    }

    public void ReturnPatientToFront(PatientVisitData visitData)
    {
        if (visitData == null)
        {
            Debug.LogWarning("Không thể trả bệnh nhân về hàng chờ vì visitData null.");
            return;
        }

        if (visitData.patientPrefab == null)
        {
            Debug.LogWarning("Không thể trả bệnh nhân về hàng chờ vì patientPrefab null.");
            return;
        }

        if (visitData.patientCase == null || visitData.patientCase.realDisease == null)
        {
            Debug.LogWarning("Không thể trả bệnh nhân về hàng chờ vì PatientCase không hợp lệ.");
            return;
        }

        Queue<PatientVisitData> newQueue = new Queue<PatientVisitData>();

        newQueue.Enqueue(visitData);

        while (waitingPatients.Count > 0)
        {
            newQueue.Enqueue(waitingPatients.Dequeue());
        }

        while (newQueue.Count > 0)
        {
            waitingPatients.Enqueue(newQueue.Dequeue());
        }

        Debug.Log("Đã trả bệnh nhân hiện tại về đầu hàng chờ. Bệnh: "
            + visitData.patientCase.realDisease.diseaseName
            + ". Số người chờ: "
            + waitingPatients.Count
            + "/"
            + maxWaitingPatients);
    }

    public void SaveSuspendedClinicSession(
        PatientVisitData visitData,
        ClinicExamStage stage,
        bool isDiagnosisCorrect
    )
    {
        if (visitData == null)
        {
            Debug.LogWarning("Không thể lưu phiên khám dở vì visitData null.");
            return;
        }

        if (visitData.patientPrefab == null)
        {
            Debug.LogWarning("Không thể lưu phiên khám dở vì patientPrefab null.");
            return;
        }

        if (visitData.patientCase == null || visitData.patientCase.realDisease == null)
        {
            Debug.LogWarning("Không thể lưu phiên khám dở vì PatientCase không hợp lệ.");
            return;
        }

        suspendedClinicVisitData = visitData;
        suspendedClinicStage = stage;
        suspendedDiagnosisCorrect = isDiagnosisCorrect;

        Debug.Log("Đã lưu phiên khám dở. Bệnh: "
            + visitData.patientCase.realDisease.diseaseName
            + ", Stage: "
            + stage);
    }

    public PatientVisitData TakeSuspendedClinicSession(
        out ClinicExamStage stage,
        out bool isDiagnosisCorrect
    )
    {
        stage = suspendedClinicStage;
        isDiagnosisCorrect = suspendedDiagnosisCorrect;

        PatientVisitData visitData = suspendedClinicVisitData;

        suspendedClinicVisitData = null;
        suspendedClinicStage = ClinicExamStage.None;
        suspendedDiagnosisCorrect = false;

        if (visitData != null && visitData.patientCase != null && visitData.patientCase.realDisease != null)
        {
            Debug.Log("Khôi phục phiên khám dở. Bệnh: "
                + visitData.patientCase.realDisease.diseaseName
                + ", Stage: "
                + stage);
        }

        return visitData;
    }

    public void ClearSuspendedClinicSession()
    {
        suspendedClinicVisitData = null;
        suspendedClinicStage = ClinicExamStage.None;
        suspendedDiagnosisCorrect = false;

        Debug.Log("Đã xóa phiên khám dở.");
    }

    public List<PatientVisitData> GetWaitingPatientsSnapshot()
    {
        return new List<PatientVisitData>(waitingPatients);
    }

    public void ClearAllWaitingPatients()
    {
        waitingPatients.Clear();

        suspendedClinicVisitData = null;
        suspendedClinicStage = ClinicExamStage.None;
        suspendedDiagnosisCorrect = false;

        Debug.Log("Đã xóa toàn bộ dữ liệu bệnh nhân: hàng chờ và phiên khám dở.");
    }
}