using System;
using System.Collections.Generic;
using UnityEngine;

public class PatientController : BaseMove
{
    [Header("A*")]
    [SerializeField] private float reachDistance = 0.2f;

    [Header("Rời phòng thuốc")]
    [SerializeField] private float leaveReachDistance = 0.8f;
    [SerializeField] private float maxLeaveTime = 6f;

    [Header("Phát hiện / va chạm vật cản")]
    [SerializeField] private LayerMask obstacleDetectLayer;
    [SerializeField] private int blockRadius = 3;
    [SerializeField] private float collisionRepathCooldown = 0.25f;
    [SerializeField] private float pushBackDistance = 0.3f;

    [Header("Prefab gốc của NPC")]
    [SerializeField] private GameObject sourcePrefab;

    private MedicalDatabase medicalDatabase;
    private AStarPathfinder2D pathfinder;
    private Transform clinicDoorPoint;

    [SerializeField] private PatientCase patientCase;
    private PatientState currentState = PatientState.Done;

    private List<Vector2> currentPath = new List<Vector2>();
    private int currentPathIndex;

    private List<Vector2> leavePath = new List<Vector2>();
    private int leavePathIndex;
    private Action onLeaveFinished;
    private float leaveStartTime;

    private float lastCollisionRepathTime;

    public PatientCase PatientCase
    {
        get { return patientCase; }
    }

    public void SetSourcePrefab(GameObject prefab)
    {
        sourcePrefab = prefab;
    }

    public void SetPatientCase(PatientCase newPatientCase)
    {
        patientCase = newPatientCase;
    }

    public void PrepareForClinicExam(PatientCase newPatientCase)
    {
        SetPatientCase(newPatientCase);

        currentState = PatientState.Done;

        currentPath.Clear();
        currentPathIndex = 0;

        leavePath.Clear();
        leavePathIndex = 0;
        onLeaveFinished = null;
        leaveStartTime = 0f;

        StopMoving();

        if (patientCase != null && patientCase.realDisease != null)
        {
            Debug.Log("NPC trong phòng khám đã nhận PatientCase: " + patientCase.realDisease.diseaseName);
        }
        else
        {
            Debug.LogWarning("NPC trong phòng khám chưa có PatientCase hợp lệ.");
        }
    }

    public void InitPatient(
        MedicalDatabase database,
        AStarPathfinder2D astarPathfinder,
        int clinicLevel,
        Transform targetClinicDoorPoint
    )
    {
        medicalDatabase = database;
        pathfinder = astarPathfinder;
        clinicDoorPoint = targetClinicDoorPoint;

        currentPath.Clear();
        currentPathIndex = 0;

        leavePath.Clear();
        leavePathIndex = 0;
        onLeaveFinished = null;
        leaveStartTime = 0f;

        if (medicalDatabase == null)
        {
            Debug.LogError("Chưa có MedicalDatabase.");
            currentState = PatientState.Done;
            return;
        }

        DiseaseData randomDisease = medicalDatabase.GetRandomDisease(clinicLevel);

        if (randomDisease == null)
        {
            Debug.LogError("Không lấy được bệnh cho NPC bệnh nhân.");
            currentState = PatientState.Done;
            return;
        }

        patientCase = new PatientCase(randomDisease);

        Debug.Log("NPC bệnh nhân được sinh ra.");
        Debug.Log("Bệnh thật: " + patientCase.realDisease.diseaseName);

        currentState = PatientState.GoingToClinicDoor;

        FindPathToClinicDoor();
    }

    private void Update()
    {
        if (currentState == PatientState.GoingToClinicDoor)
        {
            FollowPathToClinicDoor();
        }
        else if (currentState == PatientState.LeavingClinic)
        {
            FollowLeavePath();
        }
        else
        {
            StopMoving();
        }

        UpdateAnimation();
    }

    private void FindPathToClinicDoor()
    {
        if (pathfinder == null)
        {
            Debug.LogError("Chưa có AStarPathfinder2D.");
            return;
        }

        if (clinicDoorPoint == null)
        {
            Debug.LogError("Chưa có ClinicDoorPoint.");
            return;
        }

        currentPath = pathfinder.FindPath(transform.position, clinicDoorPoint.position);
        currentPathIndex = 0;

        if (currentPath == null || currentPath.Count == 0)
        {
            Debug.LogWarning("NPC không tìm được đường tới cửa nhà thuốc.");
            StopMoving();
            return;
        }

        Debug.Log("NPC đã tìm được đường tới cửa nhà thuốc. Số điểm: " + currentPath.Count);
    }

    private void FollowPathToClinicDoor()
    {
        if (currentPath == null || currentPath.Count == 0)
        {
            StopMoving();
            return;
        }

        if (currentPathIndex >= currentPath.Count)
        {
            ArriveAtClinicDoor();
            return;
        }

        Vector2 currentPosition = transform.position;
        Vector2 targetPosition = currentPath[currentPathIndex];

        Vector2 direction = targetPosition - currentPosition;
        float distance = direction.magnitude;

        if (distance <= reachDistance)
        {
            currentPathIndex++;

            if (currentPathIndex >= currentPath.Count)
            {
                ArriveAtClinicDoor();
            }

            return;
        }

        moveInput = direction.normalized;
    }

    private void ArriveAtClinicDoor()
    {
        StopMoving();

        currentState = PatientState.WaitingAtClinicDoor;

        Debug.Log("NPC bệnh nhân đã tới cửa nhà thuốc.");

        AddToClinicWaitingQueueAndDisappear();
    }

    private void AddToClinicWaitingQueueAndDisappear()
    {
        if (PatientVisitManager.Instance == null)
        {
            Debug.LogWarning("Chưa có PatientVisitManager trong scene.");
            return;
        }

        if (sourcePrefab == null)
        {
            Debug.LogWarning("NPC chưa có sourcePrefab. Cần set sourcePrefab trong script spawn NPC.");
            return;
        }

        if (patientCase == null || patientCase.realDisease == null)
        {
            Debug.LogWarning("NPC chưa có PatientCase hoặc chưa có bệnh thật.");
            return;
        }

        bool added = PatientVisitManager.Instance.AddWaitingPatient(sourcePrefab, patientCase);

        if (!added)
        {
            Debug.LogWarning("Không thêm được NPC vào hàng đợi bệnh nhân.");
            return;
        }

        Debug.Log("NPC đã được đưa vào hàng đợi phòng khám: " + patientCase.realDisease.diseaseName);
        Debug.Log("NPC ngoài map sẽ bị xóa để tránh lỗi khi chuyển scene.");

        Destroy(gameObject);
    }

    public void LeaveClinic(Transform[] leavePoints, Action finishedCallback)
    {
        StopMoving();

        leavePath.Clear();
        leavePathIndex = 0;
        onLeaveFinished = finishedCallback;
        leaveStartTime = Time.time;

        if (leavePoints == null || leavePoints.Length == 0)
        {
            Debug.LogWarning("Chưa có điểm rời phòng. Tạm ẩn NPC bệnh nhân.");
            FinishLeavingClinic();
            return;
        }

        foreach (Transform point in leavePoints)
        {
            if (point == null)
                continue;

            leavePath.Add(point.position);
        }

        if (leavePath.Count == 0)
        {
            Debug.LogWarning("Danh sách điểm rời phòng rỗng. Tạm ẩn NPC bệnh nhân.");
            FinishLeavingClinic();
            return;
        }

        currentState = PatientState.LeavingClinic;

        Debug.Log("NPC bắt đầu rời phòng thuốc. Số điểm: " + leavePath.Count);
    }

    private void FollowLeavePath()
    {
        if (Time.time - leaveStartTime >= maxLeaveTime)
        {
            Debug.LogWarning("NPC rời phòng quá lâu, tự ẩn để tránh kẹt cửa.");
            FinishLeavingClinic();
            return;
        }

        if (leavePath == null || leavePath.Count == 0)
        {
            FinishLeavingClinic();
            return;
        }

        if (leavePathIndex >= leavePath.Count)
        {
            FinishLeavingClinic();
            return;
        }

        Vector2 currentPosition = transform.position;
        Vector2 targetPosition = leavePath[leavePathIndex];

        Vector2 direction = targetPosition - currentPosition;
        float distance = direction.magnitude;

        if (distance <= leaveReachDistance)
        {
            leavePathIndex++;

            if (leavePathIndex >= leavePath.Count)
            {
                FinishLeavingClinic();
            }

            return;
        }

        moveInput = direction.normalized;
    }

    private void FinishLeavingClinic()
    {
        StopMoving();

        currentState = PatientState.Done;

        Debug.Log("NPC bệnh nhân đã rời khỏi phòng thuốc.");

        gameObject.SetActive(false);

        onLeaveFinished?.Invoke();
        onLeaveFinished = null;
    }

    private void OnCollisionEnter2D(Collision2D collision)
    {
        HandleObstacleCollision(collision);
    }

    private void OnCollisionStay2D(Collision2D collision)
    {
        HandleObstacleCollision(collision);
    }

    private void HandleObstacleCollision(Collision2D collision)
    {
        if (currentState != PatientState.GoingToClinicDoor)
            return;

        if (!IsLayerInMask(collision.gameObject.layer, obstacleDetectLayer))
            return;

        if (Time.time - lastCollisionRepathTime < collisionRepathCooldown)
            return;

        lastCollisionRepathTime = Time.time;

        ContactPoint2D contact = collision.GetContact(0);

        Debug.LogWarning("NPC đụng vật cản: " + collision.collider.name + " → lùi ra và tính lại đường.");

        StopMoving();

        Vector2 pushDirection = contact.normal;

        if (rb2d != null)
        {
            rb2d.position += pushDirection * pushBackDistance;
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
        }
        else
        {
            transform.position += (Vector3)(pushDirection * pushBackDistance);
        }

        if (pathfinder != null)
        {
            pathfinder.BlockArea(contact.point, blockRadius);
        }

        FindPathToClinicDoor();
    }

    private bool IsLayerInMask(int layer, LayerMask mask)
    {
        return (mask.value & (1 << layer)) != 0;
    }

    private void StopMoving()
    {
        moveInput = Vector2.zero;

        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
        }
    }

    public void FaceToPosition(Vector2 targetPosition)
    {
        Vector2 direction = targetPosition - (Vector2)transform.position;
        FaceDirection(direction);
    }

    public void FaceDirection(Vector2 direction)
    {
        if (direction.sqrMagnitude <= 0.01f)
            return;

        StopMoving();

        lastDirection = direction.normalized;

        if (animator != null)
        {
            animator.SetBool("isMoving", false);
            animator.SetFloat("x", lastDirection.x);
            animator.SetFloat("y", lastDirection.y);
            animator.SetFloat("speed", 0f);
        }
    }
}