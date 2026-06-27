using UnityEngine;

public class PatientSpawnManager : MonoBehaviour
{
    [Header("Danh sách prefab NPC bệnh nhân")]
    [SerializeField] private PatientController[] patientPrefabs;

    [Header("Database bệnh / thuốc")]
    [SerializeField] private MedicalDatabase medicalDatabase;

    [Header("A* tìm đường")]
    [SerializeField] private AStarPathfinder2D pathfinder;

    [Header("Cấp y quán hiện tại")]
    [SerializeField] private int clinicLevel = 1;

    [Header("Điểm sinh NPC bệnh nhân")]
    [SerializeField] private Transform[] spawnPoints;

    [Header("Điểm cửa nhà thuốc")]
    [SerializeField] private Transform clinicDoorPoint;

    [Header("Thời gian sinh bệnh nhân")]
    [SerializeField] private float firstSpawnDelay = 3f;

    private PatientController currentPatient;

    private void Start()
    {
        Invoke(nameof(SpawnPatient), firstSpawnDelay);
    }

    private void SpawnPatient()
    {
        if (currentPatient != null)
            return;

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

        currentPatient = Instantiate(
            randomPatientPrefab,
            randomSpawnPoint.position,
            Quaternion.identity
        );

        currentPatient.InitPatient(
            medicalDatabase,
            pathfinder,
            clinicLevel,
            clinicDoorPoint
        );

        Debug.Log("Đã sinh NPC bệnh nhân loại: " + randomPatientPrefab.name);
        Debug.Log("Vị trí sinh: " + randomSpawnPoint.name);
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