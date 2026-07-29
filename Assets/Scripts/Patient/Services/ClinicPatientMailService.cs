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

public static partial class ClinicPatientMailService
{
    public static PendingPatientMailData PreparePatientMail(
        GameObject patientObject,
        DiseaseData selectedDisease,
        DiseaseData realDisease,
        Dictionary<HerbData, int> prescription,
        bool diagnosisCorrect,
        bool prescriptionCorrect
    )
    {
        PendingPatientMailData data =
            new PendingPatientMailData();

        int medicinePayment =
            ClinicPrescriptionService
                .CalculatePrescriptionPayment(
                    prescription
                );

        if (medicinePayment < 1)
        {
            medicinePayment = 1;
        }

        PayMedicineMoneyNow(medicinePayment);

        int diseaseLevel = 1;

        if (realDisease != null)
        {
            diseaseLevel = Mathf.Max(
                1,
                (int)realDisease.diseaseLevel
            );
        }

        int reputationChange =
            CalculateReputationChange(
                diseaseLevel,
                diagnosisCorrect,
                prescriptionCorrect
            );

        data.hasMail = true;

        data.patientName =
            GetPatientDisplayName(patientObject);

        data.diseaseName =
            realDisease != null
                ? realDisease.diseaseName
                : "Không rõ bệnh";

        data.diagnosisCorrect =
            diagnosisCorrect;

        data.prescriptionCorrect =
            prescriptionCorrect;

        // Tiền thuốc đã được trả ngay sau khi kê đơn.
        data.moneyDelta = 0;

        // Uy tín chỉ được xử lý khi người chơi mở thư.
        data.reputationDelta =
            reputationChange;

        data.medicinePayment =
            medicinePayment;

        data.yThuUsageNote =
            GenerateWrongReasonNote(
                selectedDisease,
                realDisease,
                prescription,
                diagnosisCorrect,
                prescriptionCorrect
            );

        Debug.Log("===== YTHU NOTE =====");
        Debug.Log(data.yThuUsageNote);
        Debug.Log("=====================");

        Debug.Log(
            "Đã chuẩn bị thư bệnh nhân. Người gửi: "
            + data.patientName
            + " | Tiền thuốc đã trả ngay: "
            + medicinePayment
            + " | Uy tín trong mail: "
            + reputationChange
            + " | Ghi chú: "
            + data.yThuUsageNote
        );

        return data;
    }

    public static void SendPatientMail(
        PendingPatientMailData data
    )
    {
        if (data == null || !data.hasMail)
        {
            return;
        }

        if (MailboxManager.Instance == null)
        {
            Debug.LogWarning(
                "Không tìm thấy MailboxManager. "
                + "Không thể gửi thư bệnh nhân."
            );

            return;
        }

        MailboxManager.Instance
            .AddPatientTreatmentMail(
                data.patientName,
                data.diseaseName,
                data.diagnosisCorrect,
                data.prescriptionCorrect,
                data.moneyDelta,
                data.reputationDelta,
                data.yThuUsageNote
            );

        Debug.Log(
            "Đã gửi thư bệnh nhân sau khi bệnh nhân "
            + "rời phòng: "
            + data.patientName
        );
    }
}