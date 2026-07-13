using UnityEngine;
using UnityEngine.InputSystem;

public partial class ClinicExamManager
{
    private void Start()
    {
        if (medicineCounterDisplay != null)
        {
            medicineCounterDisplay.Hide();
        }

        FindPlayerIfMissing();
        CachePlayerAndExamArea();

        /*
         * Nếu player đang khám dở / bốc thuốc dở rồi đi ra ngoài,
         * khi quay lại phòng khám thì khôi phục ca khám đó trước.
         * Không gọi bệnh nhân mới ngay.
         */
        if (!TryRestoreSuspendedClinicSession())
        {
            ScheduleNextPatientEnter(firstPatientEnterDelay);
        }
    }

    private void Update()
    {
        if (currentPatient == null)
        {
            if (Time.time >= nextPatientEnterTime)
            {
                TryReceiveWaitingPatient();
            }

            return;
        }

        if (Keyboard.current == null)
            return;

        if (isExamRunning)
        {
            if (Keyboard.current[examineKey].wasPressedThisFrame)
            {
                if (!IsPlayerInExamArea())
                    return;

                if (!isClinicUiTemporarilyClosed && !IsClinicUiActuallyClosed())
                    return;

                isClinicUiTemporarilyClosed = true;
                ResumeTemporarilyClosedClinicUi();
            }

            return;
        }

        CheckPlayerReady();
    }

    private bool IsClinicUiActuallyClosed()
    {
        bool diagnosisClosed =
            diagnosisUIController != null &&
            (
                !diagnosisUIController.gameObject.activeInHierarchy ||
                IsUIHiddenByCanvasGroup(diagnosisUIController.gameObject)
            );

        bool prescriptionClosed =
            prescriptionUIController != null &&
            (
                !prescriptionUIController.gameObject.activeInHierarchy ||
                IsUIHiddenByCanvasGroup(prescriptionUIController.gameObject)
            );

        return diagnosisClosed || prescriptionClosed;
    }

    private void TryReceiveWaitingPatient()
    {
        if (currentPatient != null)
            return;

        if (PatientVisitManager.Instance == null)
            return;

        if (!PatientVisitManager.Instance.HasWaitingPatient)
            return;

        ReceiveWaitingPatient();
    }

    private void CheckPlayerReady()
    {
        if (!canStartExam)
            return;

        if (!IsPlayerInExamArea())
            return;

        if (Keyboard.current != null && Keyboard.current[examineKey].wasPressedThisFrame)
        {
            StartCoroutine(StartExaminationFlow());
        }
    }

    private void FindPlayerIfMissing()
    {
        if (player != null && playerCollider != null)
            return;

        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject == null)
            return;

        player = playerObject.transform;
        playerCollider = playerObject.GetComponent<Collider2D>();
    }

    private void CachePlayerAndExamArea()
    {
        if (playerExamArea != null)
            return;

        if (playerExamPoint == null)
            return;

        playerExamArea = playerExamPoint.GetComponent<Collider2D>();
    }

    private bool IsPlayerInExamArea()
    {
        FindPlayerIfMissing();
        CachePlayerAndExamArea();

        if (player == null)
            return false;

        if (playerCollider == null)
        {
            Debug.LogError("Player chưa có Collider2D nên không thể kiểm tra vùng khám.");
            return false;
        }

        if (playerExamPoint == null)
        {
            Debug.LogError("Chưa kéo PlayerExamPoint vào ClinicExamManager.");
            return false;
        }

        if (playerExamArea == null)
        {
            Debug.LogError("PlayerExamPoint chưa có Collider2D. Hãy gắn BoxCollider2D cho PlayerExamPoint.");
            return false;
        }

        return playerExamArea.bounds.Intersects(playerCollider.bounds);
    }

    private void SetUIVisibleWithoutDisabling(GameObject targetObject, bool visible)
    {
        if (targetObject == null)
            return;

        CanvasGroup canvasGroup = targetObject.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
        {
            canvasGroup = targetObject.AddComponent<CanvasGroup>();
        }

        canvasGroup.alpha = visible ? 1f : 0f;
        canvasGroup.interactable = visible;
        canvasGroup.blocksRaycasts = visible;
    }

    private bool IsUIHiddenByCanvasGroup(GameObject targetObject)
    {
        if (targetObject == null)
            return false;

        CanvasGroup canvasGroup = targetObject.GetComponent<CanvasGroup>();

        if (canvasGroup == null)
            return false;

        return canvasGroup.alpha <= 0.01f;
    }

    private void OnDrawGizmosSelected()
    {
        if (playerExamPoint != null)
        {
            Collider2D examArea = playerExamPoint.GetComponent<Collider2D>();

            if (examArea != null)
            {
                Gizmos.color = Color.green;
                Gizmos.DrawWireCube(examArea.bounds.center, examArea.bounds.size);
            }
        }

        if (npcExamPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(npcExamPoint.position, 0.35f);
        }

        if (npcLeavePoints != null)
        {
            Gizmos.color = Color.cyan;

            for (int i = 0; i < npcLeavePoints.Length; i++)
            {
                if (npcLeavePoints[i] != null)
                {
                    Gizmos.DrawWireSphere(npcLeavePoints[i].position, 0.15f);
                }
            }
        }
    }
}