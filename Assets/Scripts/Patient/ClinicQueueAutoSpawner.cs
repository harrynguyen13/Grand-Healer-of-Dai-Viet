using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;

public class ClinicQueueAutoSpawner : MonoBehaviour
{
    [Header("Chỉ chạy trong scene phòng khám")]
    [SerializeField] private string clinicSceneName = "ClinicInterior";

    [Header("Danh sách prefab NPC bệnh nhân")]
    [SerializeField] private PatientController[] patientPrefabs;

    [Header("Database bệnh / thuốc")]
    [SerializeField] private MedicalDatabase medicalDatabase;

    [Header("Clinic Exam Manager")]
    [SerializeField] private ClinicExamManager clinicExamManager;

    [Header("Điều kiện Player")]
    [SerializeField] private bool requirePlayerInClinic = true;
    [SerializeField] private string playerTag = "Player";

    [Header("Nhịp kiểm tra hàng chờ")]
    [SerializeField] private float firstCheckDelay = 5f;
    [SerializeField] private float checkInterval = 3f;

    [Header("Số NPC muốn giữ trong hàng chờ")]
    [Tooltip("Khi đang có bệnh nhân ở quầy khám, giữ từng này NPC đứng xếp hàng.")]
    [SerializeField] private int targetWaitingWhileExam = 2;

    [Tooltip("Khi quầy khám đang trống, giữ từng này NPC trong hàng chờ để gọi vào.")]
    [SerializeField] private int targetWaitingWhenIdle = 3;

    [Header("Delay trước khi tự bổ sung bệnh nhân")]
    [Tooltip("Khi hàng chờ thiếu người, chờ từng này giây rồi mới tự bổ sung.")]
    [SerializeField] private float refillDelay = 8f;

    private Coroutine spawnCoroutine;
    private float queueBelowTargetStartTime = -1f;

    private void OnEnable()
    {
        spawnCoroutine = StartCoroutine(SpawnLoop());
    }

    private void OnDisable()
    {
        if (spawnCoroutine != null)
        {
            StopCoroutine(spawnCoroutine);
            spawnCoroutine = null;
        }

        ResetRefillTimer();
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(firstCheckDelay);

        while (true)
        {
            TryRefillWaitingQueue();

            yield return new WaitForSeconds(checkInterval);
        }
    }

    private void TryRefillWaitingQueue()
    {
        if (SceneManager.GetActiveScene().name != clinicSceneName)
            return;

        if (requirePlayerInClinic && !IsPlayerInClinicScene())
        {
            ResetRefillTimer();
            return;
        }

        if (PatientVisitManager.Instance == null)
        {
            Debug.LogWarning("Không có PatientVisitManager để thêm bệnh nhân vào hàng chờ.");
            ResetRefillTimer();
            return;
        }

        if (clinicExamManager == null)
        {
            clinicExamManager = FindAnyObjectByType<ClinicExamManager>();
        }

        int targetWaitingCount = GetTargetWaitingCount();
        int currentWaitingCount = PatientVisitManager.Instance.WaitingCount;

        if (currentWaitingCount >= targetWaitingCount)
        {
            ResetRefillTimer();
            return;
        }

        if (!PatientVisitManager.Instance.CanAcceptMorePatients)
        {
            ResetRefillTimer();
            return;
        }

        if (queueBelowTargetStartTime < 0f)
        {
            queueBelowTargetStartTime = Time.time;

            Debug.Log("Hàng chờ đang thiếu người: "
                + currentWaitingCount
                + "/"
                + targetWaitingCount
                + ". Bắt đầu đếm thời gian bổ sung bệnh nhân.");

            return;
        }

        float waitingTime = Time.time - queueBelowTargetStartTime;

        if (waitingTime < refillDelay)
            return;

        RefillQueueToTarget(targetWaitingCount);

        ResetRefillTimer();
    }

    private int GetTargetWaitingCount()
    {
        bool hasCurrentPatient = clinicExamManager != null && clinicExamManager.HasCurrentPatient;

        if (hasCurrentPatient)
        {
            return Mathf.Max(0, targetWaitingWhileExam);
        }

        return Mathf.Max(0, targetWaitingWhenIdle);
    }

    private void RefillQueueToTarget(int targetWaitingCount)
    {
        if (patientPrefabs == null || patientPrefabs.Length == 0)
        {
            Debug.LogError("ClinicQueueAutoSpawner chưa kéo danh sách Patient Prefabs.");
            return;
        }

        if (medicalDatabase == null)
        {
            Debug.LogError("ClinicQueueAutoSpawner chưa kéo MedicalDatabase.");
            return;
        }

        int addedCount = 0;

        while (PatientVisitManager.Instance.WaitingCount < targetWaitingCount &&
               PatientVisitManager.Instance.CanAcceptMorePatients)
        {
            bool added = AddOneBackupPatientToQueue();

            if (!added)
                break;

            addedCount++;
        }

        if (addedCount > 0)
        {
            Debug.Log("Đã bổ sung " + addedCount + " bệnh nhân vào hàng chờ. Hiện có: "
                + PatientVisitManager.Instance.WaitingCount
                + "/"
                + PatientVisitManager.Instance.MaxWaitingPatients);
        }
    }

    private bool AddOneBackupPatientToQueue()
    {
        PatientController randomPrefab = GetRandomPatientPrefab();

        if (randomPrefab == null)
        {
            Debug.LogError("Không tìm được prefab bệnh nhân hợp lệ.");
            return false;
        }

        int currentClinicLevel = PlayerLevelService.GetCurrentUnlockLevel();

        DiseaseData randomDisease = medicalDatabase.GetRandomDisease();

        if (randomDisease == null)
        {
            Debug.LogError("Không random được bệnh cho bệnh nhân trong phòng khám. Cấp hiện tại: " + currentClinicLevel);
            return false;
        }

        PatientCase patientCase = new PatientCase(randomDisease);

        bool added = PatientVisitManager.Instance.AddWaitingPatient(
            randomPrefab.gameObject,
            patientCase
        );

        if (added)
        {
            Debug.Log("Đã thêm bệnh nhân vào hàng chờ trong phòng khám.");
            Debug.Log("NPC prefab: " + randomPrefab.name);
            Debug.Log("Cấp hiện tại: " + currentClinicLevel);
            Debug.Log("Bệnh thật: " + randomDisease.diseaseName);
        }

        return added;
    }

    private void ResetRefillTimer()
    {
        queueBelowTargetStartTime = -1f;
    }

    private bool IsPlayerInClinicScene()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        return player != null && player.activeInHierarchy;
    }

    private PatientController GetRandomPatientPrefab()
    {
        int safeLoop = 0;

        while (safeLoop < 30)
        {
            int randomIndex = Random.Range(0, patientPrefabs.Length);
            PatientController prefab = patientPrefabs[randomIndex];

            if (prefab != null)
                return prefab;

            safeLoop++;
        }

        return null;
    }
}