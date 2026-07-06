using UnityEngine;

public static class ClinicQuestProgressService
{
    public static void RecordTreatmentProgress(
        DiseaseData realDisease,
        bool diagnosisCorrect,
        bool prescriptionCorrect
    )
    {
        if (realDisease == null)
        {
            Debug.LogWarning("Không ghi nhiệm vụ được vì realDisease bị null.");
            return;
        }

        if (QuestProgressManager.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy QuestProgressManager để ghi tiến độ nhiệm vụ.");
            return;
        }

        QuestProgressManager.Instance.RecordTreatmentResult(
            realDisease,
            diagnosisCorrect,
            prescriptionCorrect
        );

        Debug.Log(
            "Đã ghi tiến độ nhiệm vụ: "
            + realDisease.diseaseName
            + " | Cấp bệnh: "
            + (int)realDisease.diseaseLevel
            + " | Chẩn đoán đúng: "
            + diagnosisCorrect
            + " | Kê đơn đúng: "
            + prescriptionCorrect
        );
    }
}