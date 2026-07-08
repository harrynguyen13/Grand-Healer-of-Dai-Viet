using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class PatientSpawnManager : MonoBehaviour
{
    [Header("Danh sách prefab NPC bệnh nhân")]
    [SerializeField] private PatientController[] patientPrefabs;

    [Header("Database bệnh / thuốc")]
    [SerializeField] private MedicalDatabase medicalDatabase;

    [Header("A* tìm đường")]
    [SerializeField] private AStarPathfinder2D pathfinder;

    [Header("Điểm sinh NPC bệnh nhân")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Điểm cửa nhà thuốc")]
    [SerializeField] private Transform clinicDoorPoint;

    [Header("Thời gian sinh bệnh nhân")]
    [SerializeField] private float firstSpawnDelay = 5f;

    [Tooltip("Thời gian ngắn nhất giữa 2 lần sinh NPC.")]
    [SerializeField] private float minSpawnInterval = 15f;

    [Tooltip("Thời gian dài nhất giữa 2 lần sinh NPC.")]
    [SerializeField] private float maxSpawnInterval = 25f;

    [Header("Giới hạn NPC ngoài map")]
    [SerializeField] private int maxActivePatientsOnMap = 2;

    [Header("Giới hạn hàng chờ")]
    [Tooltip("Nếu hàng chờ đã có số bệnh nhân này thì tạm dừng sinh thêm.")]
    [SerializeField] private int targetWaitingPatients = 2;

    private readonly List<PatientController> activePatients = new List<PatientController>();
    private Coroutine spawnCoroutine;

    private void Start()
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
            TrySpawnPatient();

            float waitTime = Random.Range(minSpawnInterval, maxSpawnInterval);
            yield return new WaitForSeconds(waitTime);
        }
    }

    private void TrySpawnPatient()
    {
        CleanupDestroyedPatients();

        if (PatientVisitManager.Instance != null)
        {
            if (PatientVisitManager.Instance.WaitingCount >= targetWaitingPatients)
            {
                Debug.Log("Hàng chờ đã có " + PatientVisitManager.Instance.WaitingCount + " bệnh nhân, tạm dừng sinh thêm.");
                return;
            }

            if (!PatientVisitManager.Instance.CanAcceptMorePatients)
            {
                Debug.Log("Hàng đợi bệnh nhân đã đầy, tạm dừng sinh NPC.");
                return;
            }
        }

        if (activePatients.Count >= maxActivePatientsOnMap)
        {
            Debug.Log("Đang có " + activePatients.Count + " NPC bệnh nhân ngoài map, tạm dừng sinh thêm.");
            return;
        }

        SpawnPatient();
    }

    private void SpawnPatient()
    {
        if (patientPrefabs == null || patientPrefabs.Length == 0)
        {
            Debug.LogError("Chưa kéo danh sách prefab NPC bệnh nhân vào PatientSpawnManager.");
            return;
        }

        if (medicalDatabase == null)
        {
            Debug.LogError("Chưa kéo MedicalDatabase vào PatientSpawnManager.");
            return;
        }

        if (pathfinder == null)
        {
            Debug.LogError("Chưa kéo AStarPathfinder2D vào PatientSpawnManager.");
            return;
        }

        if (clinicDoorPoint == null)
        {
            Debug.LogError("Chưa kéo ClinicDoorPoint vào PatientSpawnManager.");
            return;
        }

        if (spawnPoints == null || spawnPoints.Length == 0)
        {
            Debug.LogError("Chưa có điểm spawn NPC bệnh nhân.");
            return;
        }

        PatientController randomPatientPrefab = GetRandomPatientPrefab();

        if (randomPatientPrefab == null)
        {
            Debug.LogError("Không tìm được prefab NPC bệnh nhân hợp lệ.");
            return;
        }

        Transform randomSpawnPoint = GetRandomSpawnPoint();

        if (randomSpawnPoint == null)
        {
            Debug.LogError("Không tìm được điểm spawn hợp lệ.");
            return;
        }

        PatientController newPatient = Instantiate(
            randomPatientPrefab,
            randomSpawnPoint.position,
            Quaternion.identity
        );

        if (newPatient == null)
        {
            Debug.LogError("Sinh NPC bệnh nhân thất bại.");
            return;
        }

        int currentClinicLevel = PlayerLevelService.GetCurrentUnlockLevel();

        if (currentClinicLevel < 1)
            currentClinicLevel = 1;

        newPatient.SetSourcePrefab(randomPatientPrefab.gameObject);

        newPatient.InitPatient(
            medicalDatabase,
            pathfinder,
            clinicDoorPoint
        );

        activePatients.Add(newPatient);

        Debug.Log("Đã sinh NPC bệnh nhân loại: " + randomPatientPrefab.name);
        Debug.Log("Vị trí sinh: " + randomSpawnPoint.name);
        Debug.Log("Sinh bệnh nhân theo cấp hiện tại: " + currentClinicLevel);
        Debug.Log("Số NPC bệnh nhân đang đi ngoài map: " + activePatients.Count);
    }

    private void CleanupDestroyedPatients()
    {
        for (int i = activePatients.Count - 1; i >= 0; i--)
        {
            if (activePatients[i] == null)
            {
                activePatients.RemoveAt(i);
            }
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

    private Transform GetRandomSpawnPoint()
    {
        int safeLoop = 0;

        while (safeLoop < 30)
        {
            int randomIndex = Random.Range(0, spawnPoints.Length);
            Transform spawnPoint = spawnPoints[randomIndex];

            if (spawnPoint != null)
                return spawnPoint;

            safeLoop++;
        }

        return null;
    }
}