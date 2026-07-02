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

        /*
         * Quan trọng:
         * Nếu player đang khám dở / bốc thuốc dở rồi đi ra ngoài,
         * khi quay lại phòng khám thì phải khôi phục ca khám đó trước.
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

        if (isExamRunning)
        {
            if (Keyboard.current != null && Keyboard.current[examineKey].wasPressedThisFrame)
            {
                ResumeTemporarilyClosedClinicUi();
            }

            return;
        }

        CheckPlayerReady();
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

        FindPlayerIfMissing();

        if (player == null)
            return;

        if (playerExamPoint == null)
        {
            Debug.LogError("Chưa kéo PlayerExamPoint vào ClinicExamManager.");
            return;
        }

        float distance = Vector2.Distance(player.position, playerExamPoint.position);

        if (distance > playerExamDistance)
            return;

        if (Keyboard.current != null && Keyboard.current[examineKey].wasPressedThisFrame)
        {
            StartCoroutine(StartExaminationFlow());
        }
    }

    private void FindPlayerIfMissing()
    {
        if (player != null)
            return;

        GameObject playerObject = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObject != null)
        {
            player = playerObject.transform;
        }
    }

    private void OnDrawGizmosSelected()
    {
        if (playerExamPoint != null)
        {
            Gizmos.color = Color.green;
            Gizmos.DrawWireSphere(playerExamPoint.position, playerExamDistance);
        }

        if (npcExamPoint != null)
        {
            Gizmos.color = Color.yellow;
            Gizmos.DrawWireSphere(npcExamPoint.position, 0.35f);
        }

        if (npcLeavePoints != null)
        {
            Gizmos.color = Color.cyan;

            foreach (Transform point in npcLeavePoints)
            {
                if (point != null)
                {
                    Gizmos.DrawWireSphere(point.position, 0.15f);
                }
            }
        }
    }
}