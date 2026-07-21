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

    public int medicinePayment;

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

        int medicinePayment = ClinicPrescriptionService.CalculatePrescriptionPayment(prescription);

        if (medicinePayment < 1)
            medicinePayment = 1;

        PayMedicineMoneyNow(medicinePayment);

        int diseaseLevel = 1;

        if (realDisease != null)
        {
            diseaseLevel = Mathf.Max(1, (int)realDisease.diseaseLevel);
        }

        int reputationChange;

        if (diagnosisCorrect && prescriptionCorrect)
        {
            reputationChange = GetCorrectTreatmentReward(diseaseLevel);

            Debug.Log("Kết quả: ĐÚNG BỆNH + ĐÚNG THUỐC.");
        }
        else if (diagnosisCorrect && !prescriptionCorrect)
        {
            reputationChange = -GetWrongPrescriptionPenalty(diseaseLevel);

            Debug.Log("Kết quả: ĐÚNG BỆNH nhưng SAI THUỐC.");
        }
        else if (!diagnosisCorrect && prescriptionCorrect)
        {
            reputationChange = -GetWrongDiagnosisPenalty(diseaseLevel);

            Debug.Log("Kết quả: SAI BỆNH nhưng THUỐC ĐÚNG BỆNH THẬT.");
        }
        else
        {
            reputationChange = -GetWrongTreatmentPenalty(diseaseLevel);

            Debug.Log("Kết quả: SAI BỆNH + SAI THUỐC.");
        }

        data.hasMail = true;
        data.patientName = GetPatientDisplayName(patientObject);
        data.diseaseName = realDisease != null ? realDisease.diseaseName : "Không rõ bệnh";
        data.diagnosisCorrect = diagnosisCorrect;
        data.prescriptionCorrect = prescriptionCorrect;

        // Tiền thuốc đã trả ngay sau khi kê đơn, không nhận trong mail nữa.
        data.moneyDelta = 0;

        // Uy tín vẫn xử lý qua mail sau khi biết kết quả.
        data.reputationDelta = reputationChange;

        data.medicinePayment = medicinePayment;
        data.yThuUsageNote = "";

        Debug.Log(
            "Đã chuẩn bị thư bệnh nhân. Người gửi: "
            + data.patientName
            + " | Tiền thuốc đã trả ngay: "
            + medicinePayment
            + " | Uy tín trong mail: "
            + reputationChange
        );

        return data;
    }

    private static void PayMedicineMoneyNow(int medicinePayment)
    {
        if (medicinePayment <= 0)
            return;

        if (PlayerEconomy.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy PlayerEconomy. Không thể cộng tiền thuốc ngay.");
            return;
        }

        PlayerEconomy.Instance.AddMoney(medicinePayment);

        Debug.Log("Bệnh nhân đã trả tiền thuốc ngay: " + medicinePayment);
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
            return GetRandomPatientName();

        string rawName = patientObject.name;

        if (string.IsNullOrWhiteSpace(rawName))
            return GetRandomPatientName();

        string cleanName = CleanPatientObjectName(rawName);

        if (string.IsNullOrWhiteSpace(cleanName))
            return GetRandomPatientName();

        string compactKey = cleanName.ToLower().Replace(" ", "");

        if (compactKey.Contains("balao"))
            return GetRandomOldFemalePatientName();

        if (compactKey.Contains("laonong"))
            return GetRandomOldMalePatientName();

        if (compactKey.Contains("phunu"))
            return GetRandomFemalePatientName();

        if (compactKey.Contains("male"))
            return GetRandomMalePatientName();

        return GetRandomPatientName();
    }

    private static string CleanPatientObjectName(string rawName)
    {
        string cleanName = rawName;

        cleanName = cleanName.Replace("(Clone)", "");
        cleanName = cleanName.Replace("PatientNPC_", "");
        cleanName = cleanName.Replace("PatientNPC", "");
        cleanName = cleanName.Replace("NPC_", "");
        cleanName = cleanName.Replace("_", " ");
        cleanName = cleanName.Trim();

        while (cleanName.Contains("  "))
        {
            cleanName = cleanName.Replace("  ", " ");
        }

        return cleanName;
    }

    private static string GetRandomPatientName()
    {
        string[] names =
        {
            "Ông Phúc",
            "Bà Lụa",
            "Chú Bình",
            "Cô Sen",
            "Anh Hòa",
            "Chị Mùi",
            "Bác Đình",
            "Cụ Thành",
            "Thím Hạnh",
            "Dì Xuân",
            "Cậu Minh",
            "Mợ Lan"
        };

        int index = UnityEngine.Random.Range(0, names.Length);
        return names[index];
    }

    private static string GetRandomOldMalePatientName()
    {
        string[] names =
        {
            "Ông Phúc",
            "Ông Khang",
            "Ông Lộc",
            "Bác Đình",
            "Bác Thành",
            "Cụ An"
        };

        int index = UnityEngine.Random.Range(0, names.Length);
        return names[index];
    }

    private static string GetRandomOldFemalePatientName()
    {
        string[] names =
        {
            "Bà Lụa",
            "Bà Hạnh",
            "Bà Mận",
            "Bà Tảo",
            "Bà Xuân",
            "Cụ Lan"
        };

        int index = UnityEngine.Random.Range(0, names.Length);
        return names[index];
    }

    private static string GetRandomMalePatientName()
    {
        string[] names =
        {
            "Anh Hòa",
            "Anh Lâm",
            "Anh Khang",
            "Cậu Minh",
            "Cậu Bình",
            "Chú Nhân"
        };

        int index = UnityEngine.Random.Range(0, names.Length);
        return names[index];
    }

    private static string GetRandomFemalePatientName()
    {
        string[] names =
        {
            "Cô Sen",
            "Cô Mùi",
            "Cô Nụ",
            "Chị Lụa",
            "Chị Hạnh",
            "Mợ Lan"
        };

        int index = UnityEngine.Random.Range(0, names.Length);
        return names[index];
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