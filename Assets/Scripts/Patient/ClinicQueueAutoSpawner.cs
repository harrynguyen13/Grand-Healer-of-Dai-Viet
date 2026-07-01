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

    [Header("Cấp y quán hiện tại")]
    [SerializeField] private int clinicLevel = 1;

    [Header("Thời gian tạo bệnh nhân")]
    [SerializeField] private float firstSpawnDelay = 6f;
    [SerializeField] private float minSpawnInterval = 12f;
    [SerializeField] private float maxSpawnInterval = 20f;

    [Header("Giới hạn hàng chờ")]
    [Tooltip("Trong phòng khám chỉ giữ khoảng từng này bệnh nhân đang chờ.")]
    [SerializeField] private int targetWaitingPatients = 1;

    private Coroutine spawnCoroutine;

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
    }

    private IEnumerator SpawnLoop()
    {
        yield return new WaitForSeconds(firstSpawnDelay);

        while (true)
        {
            TryAddPatientToQueue();

            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void TryAddPatientToQueue()
    {
        string currentSceneName = SceneManager.GetActiveScene().name;

        if (currentSceneName != clinicSceneName)
            return;

        if (PatientVisitManager.Instance == null)
        {
            Debug.LogWarning("Không có PatientVisitManager để thêm bệnh nhân vào hàng chờ.");
            return;
        }

        if (PatientVisitManager.Instance.WaitingCount >= targetWaitingPatients)
        {
            Debug.Log("Hàng chờ phòng khám đã có " + PatientVisitManager.Instance.WaitingCount + " bệnh nhân, chưa thêm bệnh nhân mới.");
            return;
        }

        if (!PatientVisitManager.Instance.CanAcceptMorePatients)
        {
            Debug.Log("Hàng đợi bệnh nhân đã đầy, không thể thêm bệnh nhân mới.");
            return;
        }

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

        PatientController randomPrefab = GetRandomPatientPrefab();

        if (randomPrefab == null)
        {
            Debug.LogError("Không tìm được prefab bệnh nhân hợp lệ.");
            return;
        }

        DiseaseData randomDisease = medicalDatabase.GetRandomDisease(clinicLevel);

        if (randomDisease == null)
        {
            Debug.LogError("Không random được bệnh cho bệnh nhân trong phòng khám.");
            return;
        }

        PatientCase patientCase = new PatientCase(randomDisease);

        bool added = PatientVisitManager.Instance.AddWaitingPatient(
            randomPrefab.gameObject,
            patientCase
        );

        if (added)
        {
            Debug.Log("Đã tự thêm bệnh nhân mới vào hàng chờ trong phòng khám.");
            Debug.Log("NPC prefab: " + randomPrefab.name);
            Debug.Log("Bệnh thật: " + randomDisease.diseaseName);
        }
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