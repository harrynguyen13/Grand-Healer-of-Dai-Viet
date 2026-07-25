using System.Collections.Generic;
using System.Text;
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

    // Overload đầy đủ:
    // selectedDisease = bệnh người chơi chọn.
    // realDisease = bệnh thật của bệnh nhân.
    public static PendingPatientMailData PreparePatientMail(
        GameObject patientObject,
        DiseaseData selectedDisease,
        DiseaseData realDisease,
        Dictionary<HerbData, int> prescription,
        bool diagnosisCorrect,
        bool prescriptionCorrect
    )
    {
        PendingPatientMailData data = new PendingPatientMailData();

        int medicinePayment =
            ClinicPrescriptionService.CalculatePrescriptionPayment(prescription);

        if (medicinePayment < 1)
            medicinePayment = 1;

        PayMedicineMoneyNow(medicinePayment);

        int diseaseLevel = 1;

        if (realDisease != null)
        {
            diseaseLevel = Mathf.Max(
                1,
                (int)realDisease.diseaseLevel
            );
        }

        int reputationChange;

        if (diagnosisCorrect && prescriptionCorrect)
        {
            reputationChange =
                GetCorrectTreatmentReward(diseaseLevel);

            Debug.Log("Kết quả: ĐÚNG BỆNH + ĐÚNG THUỐC.");
        }
        else if (diagnosisCorrect && !prescriptionCorrect)
        {
            reputationChange =
                -GetWrongPrescriptionPenalty(diseaseLevel);

            Debug.Log("Kết quả: ĐÚNG BỆNH nhưng SAI THUỐC.");
        }
        else if (!diagnosisCorrect && prescriptionCorrect)
        {
            reputationChange =
                -GetWrongDiagnosisPenalty(diseaseLevel);

            Debug.Log(
                "Kết quả: SAI BỆNH nhưng THUỐC ĐÚNG BỆNH THẬT."
            );
        }
        else
        {
            reputationChange =
                -GetWrongTreatmentPenalty(diseaseLevel);

            Debug.Log("Kết quả: SAI BỆNH + SAI THUỐC.");
        }

        data.hasMail = true;
        data.patientName = GetPatientDisplayName(patientObject);

        data.diseaseName =
            realDisease != null
                ? realDisease.diseaseName
                : "Không rõ bệnh";

        data.diagnosisCorrect = diagnosisCorrect;
        data.prescriptionCorrect = prescriptionCorrect;

        // Tiền thuốc đã trả ngay sau khi kê đơn.
        data.moneyDelta = 0;

        // Tín nhiệm xử lý khi người chơi mở thư.
        data.reputationDelta = reputationChange;

        data.medicinePayment = medicinePayment;

        data.yThuUsageNote = GenerateWrongReasonNote(
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

    private static string GenerateWrongReasonNote(
        DiseaseData selectedDisease,
        DiseaseData realDisease,
        Dictionary<HerbData, int> prescription,
        bool diagnosisCorrect,
        bool prescriptionCorrect
    )
    {
        // Đúng cả bệnh và đơn thuốc thì không hiện nhắc nhở.
        if (diagnosisCorrect && prescriptionCorrect)
        {
            return "";
        }

        List<string> notes = new List<string>();

        // Chỉ hiện phần bệnh khi người chơi chẩn đoán sai.
        if (!diagnosisCorrect)
        {
            string realDiseaseName =
                realDisease != null
                    ? realDisease.diseaseName
                    : "Không xác định";

            string selectedDiseaseName =
                selectedDisease != null
                    ? selectedDisease.diseaseName
                    : "Chưa chọn bệnh";

            notes.Add(
                "<b>Tên bệnh đúng:</b> "
                + realDiseaseName
            );

            notes.Add(
                "<b>Tên bệnh đã chọn:</b> "
                + selectedDiseaseName
            );
        }

        // Chỉ hiện phần dược liệu khi đơn thuốc sai.
        if (!prescriptionCorrect)
        {
            if (notes.Count > 0)
            {
                notes.Add("");
            }

            AppendHerbErrors(
                notes,
                realDisease,
                prescription
            );
        }

        if (notes.Count == 0)
        {
            return "";
        }

        StringBuilder builder = new StringBuilder();

        builder.Append(
            string.Join("\n", notes)
        );

        return builder.ToString();
    }

    private static void AppendHerbErrors(
        List<string> notes,
        DiseaseData realDisease,
        Dictionary<HerbData, int> prescription
    )
    {
        if (notes == null)
        {
            return;
        }

        if (
            realDisease == null
            || realDisease.requiredHerbs == null
            || realDisease.requiredHerbs.Count == 0
        )
        {
            notes.Add(
                "Không xác định được dược liệu cần dùng."
            );

            return;
        }

        Dictionary<string, int> requiredAmounts =
            new Dictionary<string, int>();

        Dictionary<string, string> requiredDisplayNames =
            new Dictionary<string, string>();

        // Tổng hợp dược liệu và số lượng chuẩn của bệnh.
        for (int i = 0; i < realDisease.requiredHerbs.Count; i++)
        {
            RequiredHerbAmount required =
                realDisease.requiredHerbs[i];

            if (required == null || required.herb == null)
            {
                continue;
            }

            string herbKey =
                NormalizeHerbName(required.herb.herbName);

            if (string.IsNullOrEmpty(herbKey))
            {
                continue;
            }

            int requiredAmount =
                Mathf.Max(1, required.amount);

            if (!requiredAmounts.ContainsKey(herbKey))
            {
                requiredAmounts.Add(
                    herbKey,
                    0
                );

                requiredDisplayNames.Add(
                    herbKey,
                    required.herb.herbName
                );
            }

            requiredAmounts[herbKey] +=
                requiredAmount;
        }

        Dictionary<string, int> selectedAmounts =
            new Dictionary<string, int>();

        Dictionary<string, string> selectedDisplayNames =
            new Dictionary<string, string>();

        // Tổng hợp dược liệu người chơi đã kê.
        if (prescription != null)
        {
            foreach (
                KeyValuePair<HerbData, int> selected
                in prescription
            )
            {
                if (
                    selected.Key == null
                    || selected.Value <= 0
                )
                {
                    continue;
                }

                string herbKey =
                    NormalizeHerbName(selected.Key.herbName);

                if (string.IsNullOrEmpty(herbKey))
                {
                    continue;
                }

                if (!selectedAmounts.ContainsKey(herbKey))
                {
                    selectedAmounts.Add(
                        herbKey,
                        0
                    );

                    selectedDisplayNames.Add(
                        herbKey,
                        selected.Key.herbName
                    );
                }

                selectedAmounts[herbKey] +=
                    selected.Value;
            }
        }

        List<string> requiredHerbs =
            new List<string>();

        List<string> missingHerbs =
            new List<string>();

        List<string> excessHerbs =
            new List<string>();

        // Ghi dược liệu cần và tính đúng phần thiếu/thừa.
        foreach (
            KeyValuePair<string, int> required
            in requiredAmounts
        )
        {
            string herbKey = required.Key;
            int requiredAmount = required.Value;
            string herbName =
                requiredDisplayNames[herbKey];

            requiredHerbs.Add(
                herbName + " x" + requiredAmount
            );

            selectedAmounts.TryGetValue(
                herbKey,
                out int selectedAmount
            );

            if (selectedAmount < requiredAmount)
            {
                int missingAmount =
                    requiredAmount - selectedAmount;

                missingHerbs.Add(
                    herbName + " x" + missingAmount
                );
            }
            else if (selectedAmount > requiredAmount)
            {
                int excessAmount =
                    selectedAmount - requiredAmount;

                excessHerbs.Add(
                    herbName + " x" + excessAmount
                );
            }
        }

        // Vị thuốc không thuộc đơn chuẩn được tính là thừa.
        foreach (
            KeyValuePair<string, int> selected
            in selectedAmounts
        )
        {
            if (requiredAmounts.ContainsKey(selected.Key))
            {
                continue;
            }

            string herbName =
                selectedDisplayNames.ContainsKey(selected.Key)
                    ? selectedDisplayNames[selected.Key]
                    : selected.Key;

            excessHerbs.Add(
                herbName + " x" + selected.Value
            );
        }

        notes.Add(
            "<b>Dược liệu cần:</b> "
            + string.Join(", ", requiredHerbs)
        );

        if (missingHerbs.Count > 0)
        {
            notes.Add(
                "<b>Thiếu:</b> "
                + string.Join(", ", missingHerbs)
            );
        }

        if (excessHerbs.Count > 0)
        {
            notes.Add(
                "<b>Thừa:</b> "
                + string.Join(", ", excessHerbs)
            );
        }

        // Phòng trường hợp kết quả đơn thuốc báo sai
        // nhưng dữ liệu chênh lệch không xác định được.
        if (
            missingHerbs.Count == 0
            && excessHerbs.Count == 0
        )
        {
            notes.Add(
                "Đơn thuốc chưa khớp với phương thuốc chuẩn."
            );
        }
    }

    private static string NormalizeHerbName(
        string herbName
    )
    {
        if (string.IsNullOrWhiteSpace(herbName))
            return "";

        return herbName.Trim().ToLower();
    }

    private static void PayMedicineMoneyNow(
        int medicinePayment
    )
    {
        if (medicinePayment <= 0)
            return;

        if (PlayerEconomy.Instance == null)
        {
            Debug.LogWarning(
                "Không tìm thấy PlayerEconomy. "
                + "Không thể cộng tiền thuốc ngay."
            );

            return;
        }

        PlayerEconomy.Instance.AddMoney(
            medicinePayment
        );

        Debug.Log(
            "Bệnh nhân đã trả tiền thuốc ngay: "
            + medicinePayment
        );
    }

    public static void SendPatientMail(
        PendingPatientMailData data
    )
    {
        if (data == null || !data.hasMail)
            return;

        if (MailboxManager.Instance == null)
        {
            Debug.LogWarning(
                "Không tìm thấy MailboxManager. "
                + "Không thể gửi thư bệnh nhân."
            );

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

        Debug.Log(
            "Đã gửi thư bệnh nhân sau khi bệnh nhân "
            + "rời phòng: "
            + data.patientName
        );
    }

    public static string GetPatientDisplayName(
        GameObject patientObject
    )
    {
        if (patientObject == null)
            return GetRandomPatientName();

        string rawName = patientObject.name;

        if (string.IsNullOrWhiteSpace(rawName))
            return GetRandomPatientName();

        string cleanName =
            CleanPatientObjectName(rawName);

        if (string.IsNullOrWhiteSpace(cleanName))
            return GetRandomPatientName();

        string compactKey =
            cleanName
                .ToLower()
                .Replace(" ", "");

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

    private static string CleanPatientObjectName(
        string rawName
    )
    {
        string cleanName = rawName;

        cleanName =
            cleanName.Replace("(Clone)", "");

        cleanName =
            cleanName.Replace("PatientNPC_", "");

        cleanName =
            cleanName.Replace("PatientNPC", "");

        cleanName =
            cleanName.Replace("NPC_", "");

        cleanName =
            cleanName.Replace("_", " ");

        cleanName = cleanName.Trim();

        while (cleanName.Contains("  "))
        {
            cleanName =
                cleanName.Replace("  ", " ");
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

        int index =
            UnityEngine.Random.Range(
                0,
                names.Length
            );

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

        int index =
            UnityEngine.Random.Range(
                0,
                names.Length
            );

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

        int index =
            UnityEngine.Random.Range(
                0,
                names.Length
            );

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

        int index =
            UnityEngine.Random.Range(
                0,
                names.Length
            );

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

        int index =
            UnityEngine.Random.Range(
                0,
                names.Length
            );

        return names[index];
    }

    private static int GetCorrectTreatmentReward(
        int diseaseLevel
    )
    {
        if (diseaseLevel <= 1)
            return 10;

        if (diseaseLevel == 2)
            return 15;

        if (diseaseLevel == 3)
            return 22;

        if (diseaseLevel == 4)
            return 30;

        return 45;
    }

    private static int GetWrongPrescriptionPenalty(
        int diseaseLevel
    )
    {
        if (diseaseLevel <= 1)
            return 3;

        if (diseaseLevel == 2)
            return 5;

        if (diseaseLevel == 3)
            return 8;

        if (diseaseLevel == 4)
            return 12;

        return 18;
    }

    private static int GetWrongDiagnosisPenalty(
        int diseaseLevel
    )
    {
        if (diseaseLevel <= 1)
            return 2;

        if (diseaseLevel == 2)
            return 4;

        if (diseaseLevel == 3)
            return 6;

        if (diseaseLevel == 4)
            return 9;

        return 14;
    }

    private static int GetWrongTreatmentPenalty(
        int diseaseLevel
    )
    {
        if (diseaseLevel <= 1)
            return 5;

        if (diseaseLevel == 2)
            return 8;

        if (diseaseLevel == 3)
            return 12;

        if (diseaseLevel == 4)
            return 18;

        return 25;
    }
}