using UnityEngine;

public class PatientVisitManager : MonoBehaviour
{
    public static PatientVisitManager Instance { get; private set; }

    public PatientController WaitingPatient { get; private set; }

    public bool HasWaitingPatient
    {
        get { return WaitingPatient != null; }
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

    public void SetWaitingPatient(PatientController patient)
    {
        WaitingPatient = patient;

        if (WaitingPatient != null)
        {
            DontDestroyOnLoad(WaitingPatient.gameObject);
            WaitingPatient.gameObject.SetActive(false);

            Debug.Log("Đã lưu NPC bệnh nhân chờ trong phòng khám: " + WaitingPatient.name);
        }
    }

    public PatientController TakeWaitingPatient()
    {
        PatientController patient = WaitingPatient;
        WaitingPatient = null;
        return patient;
    }
}