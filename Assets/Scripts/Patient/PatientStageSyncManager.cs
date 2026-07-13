using UnityEngine;

public class PatientStageSyncManager : MonoBehaviour
{
    private int lastUnlockLevel;

    private void Start()
    {
        lastUnlockLevel = PlayerLevelService.GetCurrentUnlockLevel();
    }

    private void Update()
    {
        int currentUnlockLevel = PlayerLevelService.GetCurrentUnlockLevel();

        if (currentUnlockLevel == lastUnlockLevel)
            return;

        // Chỉ khi bị hạ cấp mới xóa bệnh nhân
        if (currentUnlockLevel < lastUnlockLevel)
        {
            ClearCurrentPatientsInScene();
            ClearWaitingPatientsData();

            Debug.Log(
                "Player bị hạ cấp từ "
                + lastUnlockLevel
                + " xuống "
                + currentUnlockLevel
                + ". Đã xóa NPC bệnh hiện tại và hàng chờ."
            );
        }
        else
        {
            Debug.Log(
                "Player thăng cấp từ "
                + lastUnlockLevel
                + " lên "
                + currentUnlockLevel
                + ". Không xóa NPC bệnh."
            );
        }

        lastUnlockLevel = currentUnlockLevel;
    }

    private void ClearCurrentPatientsInScene()
    {
        PatientController[] patients =
            Object.FindObjectsByType<PatientController>(FindObjectsInactive.Include);

        for (int i = 0; i < patients.Length; i++)
        {
            if (patients[i] == null)
                continue;

            Destroy(patients[i].gameObject);
        }
    }

    private void ClearWaitingPatientsData()
    {
        if (PatientVisitManager.Instance == null)
        {
            Debug.LogWarning("PatientStageSyncManager: Không tìm thấy PatientVisitManager.");
            return;
        }

        PatientVisitManager.Instance.ClearAllWaitingPatients();
    }
}