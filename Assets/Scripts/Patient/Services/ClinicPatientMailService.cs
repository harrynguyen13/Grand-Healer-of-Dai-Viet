using System.Collections.Generic;
using UnityEngine;

public class PendingPatientMailData
{
    public bool hasMail;

    public string patientName;
    public string diseaseName;

    public bool diagnosisCorrect;
    public bool prescriptionCorrect;

    public int moneyDelta;
    public int reputationDelta;

    public string yThuUsageNote;
}

public static class ClinicPatientMailService
{
    public static PendingPatientMailData PreparePatientMail(
        GameObject patientObject,
        DiseaseData realDisease,
        Dictionary<HerbData, int> prescription,
        bool diagnosisCorrect,
        bool prescriptionCorrect
    )
    {
        PendingPatientMailData data = new PendingPatientMailData();

        int payment = ClinicPrescriptionService.CalculatePrescriptionPayment(prescription);

        int diseaseLevel = 1;

        if (realDisease != null)
        {
            diseaseLevel = Mathf.Max(1, (int)realDisease.diseaseLevel);
        }

        int reputationChange;
        float paymentMultiplier;

        if (diagnosisCorrect && prescriptionCorrect)
        {
            reputationChange = GetCorrectTreatmentReward(diseaseLevel);
            paymentMultiplier = 1f;

            Debug.Log("Kết quả: ĐÚNG BỆNH + ĐÚNG THUỐC.");
        }
        else if (diagnosisCorrect && !prescriptionCorrect)
        {
            reputationChange = -GetWrongPrescriptionPenalty(diseaseLevel);
            paymentMultiplier = 0.4f;

            Debug.Log("Kết quả: ĐÚNG BỆNH nhưng SAI THUỐC.");
        }
        else if (!diagnosisCorrect && prescriptionCorrect)
        {
            reputationChange = -GetWrongDiagnosisPenalty(diseaseLevel);
            paymentMultiplier = 0.5f;

            Debug.Log("Kết quả: SAI BỆNH nhưng THUỐC ĐÚNG BỆNH THẬT.");
        }
        else
        {
            reputationChange = -GetWrongTreatmentPenalty(diseaseLevel);
            paymentMultiplier = 0.2f;

            Debug.Log("Kết quả: SAI BỆNH + SAI THUỐC.");
        }

        payment = Mathf.RoundToInt(payment * paymentMultiplier);

        if (payment < 1)
            payment = 1;

        data.hasMail = true;
        data.patientName = GetPatientDisplayName(patientObject);
        data.diseaseName = realDisease != null ? realDisease.diseaseName : "Không rõ bệnh";
        data.diagnosisCorrect = diagnosisCorrect;
        data.prescriptionCorrect = prescriptionCorrect;
        data.moneyDelta = payment;
        data.reputationDelta = reputationChange;
        data.yThuUsageNote = "";

        Debug.Log("Đã chuẩn bị thư bệnh nhân. Người gửi: " + data.patientName);

        return data;
    }

    public static void SendPatientMail(PendingPatientMailData data)
    {
        if (data == null || !data.hasMail)
            return;

        if (MailboxManager.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy MailboxManager. Không thể gửi thư bệnh nhân.");
            return;
        }

        MailboxManager.Instance.AddPatientTreatmentMail(
            data.patientName,
            data.diseaseName,
            data.diagnosisCorrect,
            data.prescriptionCorrect,
            data.moneyDelta,
            data.reputationDelta,
            data.yThuUsageNote
        );

        Debug.Log("Đã gửi thư bệnh nhân sau khi bệnh nhân rời phòng: " + data.patientName);
    }

    public static string GetPatientDisplayName(GameObject patientObject)
    {
        if (patientObject == null)
            return "Bệnh nhân";

        string rawName = patientObject.name;

        if (string.IsNullOrWhiteSpace(rawName))
            return "Bệnh nhân";

        string cleanName = rawName;

        cleanName = cleanName.Replace("(Clone)", "");
        cleanName = cleanName.Replace("PatientNPC_", "");
        cleanName = cleanName.Replace("PatientNPC", "");
        cleanName = cleanName.Replace("NPC_", "");
        cleanName = cleanName.Replace("_", " ");
        cleanName = cleanName.Trim();

        string key = cleanName.ToLower();

        while (key.Contains("  "))
        {
            key = key.Replace("  ", " ");
        }

        string compactKey = key.Replace(" ", "");

        if (compactKey.Contains("balao") || key.Contains("bà lão"))
            return "Bà lão";

        if (compactKey.Contains("laonong") || key.Contains("lão nông"))
            return "Lão nông";

        if (compactKey.Contains("onglao") || key.Contains("ông lão"))
            return "Ông lão";

        if (compactKey.Contains("thuongnhan") || key.Contains("thương nhân"))
            return "Thương nhân";

        if (compactKey.Contains("nam"))
            return "Nam bệnh nhân";

        if (compactKey.Contains("nu") || key.Contains("nữ"))
            return "Nữ bệnh nhân";

        return "Bệnh nhân";
    }

    private static int GetCorrectTreatmentReward(int diseaseLevel)
    {
        if (diseaseLevel <= 1) return 10;
        if (diseaseLevel == 2) return 15;
        if (diseaseLevel == 3) return 22;
        if (diseaseLevel == 4) return 30;

        return 45;
    }

    private static int GetWrongPrescriptionPenalty(int diseaseLevel)
    {
        if (diseaseLevel <= 1) return 3;
        if (diseaseLevel == 2) return 5;
        if (diseaseLevel == 3) return 8;
        if (diseaseLevel == 4) return 12;

        return 18;
    }

    private static int GetWrongDiagnosisPenalty(int diseaseLevel)
    {
        if (diseaseLevel <= 1) return 2;
        if (diseaseLevel == 2) return 4;
        if (diseaseLevel == 3) return 6;
        if (diseaseLevel == 4) return 9;

        return 14;
    }

    private static int GetWrongTreatmentPenalty(int diseaseLevel)
    {
        if (diseaseLevel <= 1) return 5;
        if (diseaseLevel == 2) return 8;
        if (diseaseLevel == 3) return 12;
        if (diseaseLevel == 4) return 18;

        return 25;
    }
}