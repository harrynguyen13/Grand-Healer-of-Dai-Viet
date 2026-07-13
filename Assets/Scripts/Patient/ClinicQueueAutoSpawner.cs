using System.Collections;
using System.Collections.Generic;
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
    [SerializeField] private float firstCheckDelay = 20f;
    [SerializeField] private float checkInterval = 10f;

    [Header("Số NPC muốn giữ trong hàng chờ")]
    [Tooltip("Khi đang có bệnh nhân ở quầy khám, giữ từng này NPC đứng xếp hàng.")]
    [SerializeField] private int targetWaitingWhileExam = 2;

    [Tooltip("Khi quầy khám đang trống, giữ từng này NPC trong hàng chờ để gọi vào.")]
    [SerializeField] private int targetWaitingWhenIdle = 3;

    [Header("Delay trước khi tự bổ sung bệnh nhân")]
    [Tooltip("Thời gian tối thiểu trước khi thêm 1 bệnh nhân mới vào hàng chờ.")]
    [SerializeField] private float minRefillDelay = 10f;

    [Tooltip("Thời gian tối đa trước khi thêm 1 bệnh nhân mới vào hàng chờ.")]
    [SerializeField] private float maxRefillDelay = 15f;

    [Header("Tỉ lệ random bệnh theo cấp đã mở")]
    [Tooltip("Càng thấp thì bệnh cấp cũ càng ít xuất hiện. 0.35 nghĩa là mỗi cấp thấp hơn sẽ giảm còn 35%.")]
    [SerializeField] private float lowerLevelWeightMultiplier = 0.35f;

    private Coroutine spawnCoroutine;

    private float queueBelowTargetStartTime = -1f;
    private float currentRefillDelay = 0f;

    private void OnEnable()
    {
        ResetRefillTimer();
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
            currentRefillDelay = GetRandomRefillDelay();

            Debug.Log(
                "Hàng chờ đang thiếu người: "
                + currentWaitingCount
                + "/"
                + targetWaitingCount
                + ". Sẽ bổ sung 1 bệnh nhân sau "
                + currentRefillDelay
                + " giây."
            );

            return;
        }

        float waitingTime = Time.time - queueBelowTargetStartTime;

        if (waitingTime < currentRefillDelay)
            return;

        bool added = AddOneBackupPatientToQueue();

        if (added)
        {
            Debug.Log(
                "Đã bổ sung 1 bệnh nhân vào hàng chờ. Hiện có: "
                + PatientVisitManager.Instance.WaitingCount
                + "/"
                + PatientVisitManager.Instance.MaxWaitingPatients
            );
        }

        ResetRefillTimer();
    }

    private float GetRandomRefillDelay()
    {
        float minDelay = Mathf.Max(1f, minRefillDelay);
        float maxDelay = Mathf.Max(minDelay, maxRefillDelay);

        return Random.Range(minDelay, maxDelay);
    }

    private int GetTargetWaitingCount()
    {
        bool hasCurrentPatient = clinicExamManager != null && clinicExamManager.HasCurrentPatient;

        if (hasCurrentPatient)
            return Mathf.Max(0, targetWaitingWhileExam);

        return Mathf.Max(0, targetWaitingWhenIdle);
    }

    private bool AddOneBackupPatientToQueue()
    {
        PatientController randomPrefab = GetRandomPatientPrefab();

        if (randomPrefab == null)
        {
            Debug.LogError("Không tìm được prefab bệnh nhân hợp lệ.");
            return false;
        }

        if (medicalDatabase == null)
        {
            Debug.LogError("ClinicQueueAutoSpawner chưa kéo MedicalDatabase.");
            return false;
        }

        int currentClinicLevel = PlayerLevelService.GetCurrentUnlockLevel();

        DiseaseData randomDisease = GetWeightedRandomDiseaseByUnlockedLevel(currentClinicLevel);

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
            Debug.Log("Đã thêm 1 bệnh nhân vào hàng chờ trong phòng khám.");
            Debug.Log("NPC prefab: " + randomPrefab.name);
            Debug.Log("Cấp hiện tại: " + currentClinicLevel);
            Debug.Log("Bệnh thật: " + randomDisease.diseaseName);
            Debug.Log("Cấp bệnh: " + (int)randomDisease.diseaseLevel);
        }

        return added;
    }

    private DiseaseData GetWeightedRandomDiseaseByUnlockedLevel(int currentClinicLevel)
    {
        if (medicalDatabase == null)
            return null;

        currentClinicLevel = Mathf.Max(1, currentClinicLevel);

        List<DiseaseData> unlockedDiseases = medicalDatabase.GetUnlockedDiseases(currentClinicLevel);

        if (unlockedDiseases == null || unlockedDiseases.Count == 0)
        {
            Debug.LogWarning("Không có bệnh mở khóa ở cấp hiện tại: " + currentClinicLevel);
            return null;
        }

        List<DiseaseData> validDiseases = new List<DiseaseData>();
        List<float> weights = new List<float>();

        float totalWeight = 0f;
        float safeLowerLevelMultiplier = Mathf.Clamp(lowerLevelWeightMultiplier, 0.01f, 1f);

        for (int i = 0; i < unlockedDiseases.Count; i++)
        {
            DiseaseData disease = unlockedDiseases[i];

            if (disease == null)
                continue;

            int diseaseLevel = Mathf.Max(1, (int)disease.diseaseLevel);

            if (diseaseLevel > currentClinicLevel)
                continue;

            int levelDistance = currentClinicLevel - diseaseLevel;

            float weight = Mathf.Pow(safeLowerLevelMultiplier, levelDistance);

            validDiseases.Add(disease);
            weights.Add(weight);

            totalWeight += weight;
        }

        if (validDiseases.Count == 0)
            return null;

        if (totalWeight <= 0f)
        {
            int randomIndex = Random.Range(0, validDiseases.Count);
            return validDiseases[randomIndex];
        }

        float randomValue = Random.Range(0f, totalWeight);
        float currentWeight = 0f;

        for (int i = 0; i < validDiseases.Count; i++)
        {
            currentWeight += weights[i];

            if (randomValue <= currentWeight)
                return validDiseases[i];
        }

        return validDiseases[validDiseases.Count - 1];
    }

    private void ResetRefillTimer()
    {
        queueBelowTargetStartTime = -1f;
        currentRefillDelay = 0f;
    }

    private bool IsPlayerInClinicScene()
    {
        GameObject player = GameObject.FindGameObjectWithTag(playerTag);
        return player != null && player.activeInHierarchy;
    }

    private PatientController GetRandomPatientPrefab()
    {
        if (patientPrefabs == null || patientPrefabs.Length == 0)
            return null;

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