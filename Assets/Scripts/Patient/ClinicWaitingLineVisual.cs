using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ClinicWaitingLineVisual : MonoBehaviour
{
    [Header("Điểm đứng xếp hàng")]
    [SerializeField] private Transform[] queuePoints;

    [Header("NPC nhìn về điểm này")]
    [SerializeField] private Transform faceTarget;

    [Header("Tần suất cập nhật hàng chờ")]
    [SerializeField] private float refreshInterval = 0.5f;

    [Header("Tùy chọn")]
    [SerializeField] private bool disableCollisionForVisualNPC = true;

    private readonly List<GameObject> spawnedVisualNPCs = new List<GameObject>();
    private string lastQueueSignature = "";

    private Coroutine refreshCoroutine;

    private void OnEnable()
    {
        refreshCoroutine = StartCoroutine(RefreshLoop());
    }

    private void OnDisable()
    {
        if (refreshCoroutine != null)
        {
            StopCoroutine(refreshCoroutine);
            refreshCoroutine = null;
        }

        ClearVisualNPCs();
    }

    private IEnumerator RefreshLoop()
    {
        while (true)
        {
            RefreshQueueVisualIfNeeded();
            yield return new WaitForSeconds(refreshInterval);
        }
    }

    private void RefreshQueueVisualIfNeeded()
    {
        if (PatientVisitManager.Instance == null)
        {
            ClearVisualNPCs();
            lastQueueSignature = "";
            return;
        }

        List<PatientVisitData> waitingPatients = PatientVisitManager.Instance.GetWaitingPatientsSnapshot();

        string currentSignature = BuildQueueSignature(waitingPatients);

        if (currentSignature == lastQueueSignature)
            return;

        lastQueueSignature = currentSignature;

        RebuildQueueVisual(waitingPatients);
    }

    private string BuildQueueSignature(List<PatientVisitData> waitingPatients)
    {
        if (waitingPatients == null || waitingPatients.Count == 0)
            return "EMPTY";

        string signature = "";

        for (int i = 0; i < waitingPatients.Count; i++)
        {
            PatientVisitData data = waitingPatients[i];

            if (data == null)
            {
                signature += "NULL|";
                continue;
            }

            string prefabName = data.patientPrefab != null ? data.patientPrefab.name : "NO_PREFAB";

            string diseaseName = "NO_DISEASE";

            if (data.patientCase != null && data.patientCase.realDisease != null)
            {
                diseaseName = data.patientCase.realDisease.diseaseName;
            }

            signature += prefabName + "_" + diseaseName + "|";
        }

        return signature;
    }

    private void RebuildQueueVisual(List<PatientVisitData> waitingPatients)
    {
        ClearVisualNPCs();

        if (waitingPatients == null || waitingPatients.Count == 0)
            return;

        if (queuePoints == null || queuePoints.Length == 0)
        {
            Debug.LogWarning("ClinicWaitingLineVisual chưa có Queue Points.");
            return;
        }

        int visualCount = Mathf.Min(waitingPatients.Count, queuePoints.Length);

        for (int i = 0; i < visualCount; i++)
        {
            PatientVisitData data = waitingPatients[i];
            Transform queuePoint = queuePoints[i];

            if (data == null)
                continue;

            if (data.patientPrefab == null)
                continue;

            if (queuePoint == null)
                continue;

            GameObject npcObject = Instantiate(
                data.patientPrefab,
                queuePoint.position,
                Quaternion.identity,
                transform
            );

            npcObject.name = "QueueVisual_" + i + "_" + data.patientPrefab.name;

            PatientController patientController = npcObject.GetComponent<PatientController>();

            if (patientController != null)
            {
                if (data.patientCase != null)
                {
                    patientController.PrepareForClinicExam(data.patientCase);
                }

                if (faceTarget != null)
                {
                    patientController.FaceToPosition(faceTarget.position);
                }
            }

            Rigidbody2D rb2d = npcObject.GetComponent<Rigidbody2D>();

            if (rb2d != null)
            {
                rb2d.linearVelocity = Vector2.zero;
                rb2d.angularVelocity = 0f;
                rb2d.bodyType = RigidbodyType2D.Kinematic;
            }

            if (disableCollisionForVisualNPC)
            {
                Collider2D[] colliders = npcObject.GetComponentsInChildren<Collider2D>();

                foreach (Collider2D col in colliders)
                {
                    col.enabled = false;
                }
            }

            spawnedVisualNPCs.Add(npcObject);
        }
    }

    private void ClearVisualNPCs()
    {
        for (int i = spawnedVisualNPCs.Count - 1; i >= 0; i--)
        {
            if (spawnedVisualNPCs[i] != null)
            {
                Destroy(spawnedVisualNPCs[i]);
            }
        }

        spawnedVisualNPCs.Clear();
    }
}