using System.Collections.Generic;
using UnityEngine;

public class PatientVisitManager : MonoBehaviour
{
    public static PatientVisitManager Instance { get; private set; }

    [Header("Số bệnh nhân chờ tối đa")]
    [SerializeField] private int maxWaitingPatients = 5;

    private readonly Queue<PatientVisitData> waitingPatients = new Queue<PatientVisitData>();

    public bool HasWaitingPatient
    {
        get { return waitingPatients.Count > 0; }
    }

    public int WaitingCount
    {
        get { return waitingPatients.Count; }
    }

    public bool CanAcceptMorePatients
    {
        get { return waitingPatients.Count < maxWaitingPatients; }
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

        Debug.Log("Đã thêm bệnh nhân vào hàng đợi. Bệnh: " + patientCase.realDisease.diseaseName + ". Số người chờ: " + waitingPatients.Count);

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
            Debug.Log("Lấy bệnh nhân khỏi hàng đợi. Bệnh: " + visitData.patientCase.realDisease.diseaseName + ". Còn chờ: " + waitingPatients.Count);
        }

        return visitData;
    }

    public void ClearAllWaitingPatients()
    {
        waitingPatients.Clear();
        Debug.Log("Đã xóa toàn bộ hàng đợi bệnh nhân.");
    }
}