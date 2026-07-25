using System.Collections.Generic;
using UnityEngine;

public static class ClinicPrescriptionService
{
    public static int CalculatePrescriptionPayment(Dictionary<HerbData, int> prescription)
    {
        if (prescription == null)
            return 0;

        int total = 0;

        foreach (KeyValuePair<HerbData, int> pair in prescription)
        {
            HerbData herb = pair.Key;
            int quantity = pair.Value;

            if (herb == null || quantity <= 0)
                continue;

            total += herb.sellPrice * quantity;
        }

        return Mathf.Max(1, total);
    }

    public static bool IsPrescriptionCorrectForDisease(
        DiseaseData disease,
        Dictionary<HerbData, int> selectedPrescription
    )
    {
        if (disease == null || disease.requiredHerbs == null)
            return false;

        if (selectedPrescription == null)
            return false;

        Dictionary<string, int> requiredHerbs = new Dictionary<string, int>();

        foreach (RequiredHerbAmount required in disease.requiredHerbs)
        {
            if (required == null || required.herb == null)
                continue;

            string herbKey = GetHerbKey(required.herb);

            if (string.IsNullOrEmpty(herbKey))
                continue;

            if (!requiredHerbs.ContainsKey(herbKey))
                requiredHerbs.Add(herbKey, 0);

            requiredHerbs[herbKey] += Mathf.Max(1, required.amount);
        }

        Dictionary<string, int> selectedHerbs = new Dictionary<string, int>();

        foreach (KeyValuePair<HerbData, int> pair in selectedPrescription)
        {
            HerbData herb = pair.Key;
            int quantity = pair.Value;

            if (herb == null || quantity <= 0)
                continue;

            string herbKey = GetHerbKey(herb);

            if (string.IsNullOrEmpty(herbKey))
                continue;

            if (!selectedHerbs.ContainsKey(herbKey))
                selectedHerbs.Add(herbKey, 0);

            selectedHerbs[herbKey] += quantity;
        }

        if (requiredHerbs.Count <= 0)
        {
            Debug.LogWarning("Bệnh này chưa có requiredHerbs.");
            return false;
        }

        if (selectedHerbs.Count != requiredHerbs.Count)
        {
            Debug.Log("Sai số vị thuốc. Cần: "
                + requiredHerbs.Count
                + ", đã kê: "
                + selectedHerbs.Count);

            return false;
        }

        foreach (KeyValuePair<string, int> requiredPair in requiredHerbs)
        {
            string requiredHerbKey = requiredPair.Key;
            int requiredAmount = requiredPair.Value;

            if (!selectedHerbs.TryGetValue(requiredHerbKey, out int selectedAmount))
            {
                Debug.Log("Thiếu vị thuốc: " + requiredHerbKey);
                return false;
            }

            if (selectedAmount != requiredAmount)
            {
                Debug.Log(
                    "Sai số lượng thuốc: " + requiredHerbKey
                    + ". Cần: " + requiredAmount
                    + ", đã kê: " + selectedAmount
                );

                return false;
            }
        }

        return true;
    }

    public static string GetRequiredHerbNames(DiseaseData disease)
    {
        if (disease == null || disease.requiredHerbs == null)
            return "Không có dữ liệu thuốc.";

        List<string> herbNames = new List<string>();

        foreach (RequiredHerbAmount required in disease.requiredHerbs)
        {
            if (required == null || required.herb == null)
                continue;

            herbNames.Add(required.herb.herbName + " x" + required.amount);
        }

        if (herbNames.Count <= 0)
            return "Không có dữ liệu thuốc.";

        return string.Join(", ", herbNames);
    }

    private static string GetHerbKey(HerbData herb)
    {
        if (herb == null || string.IsNullOrWhiteSpace(herb.herbName))
            return string.Empty;

        return herb.herbName.Trim().ToLower();
    }

    public static string BuildPrescriptionReminder(
        DiseaseData disease,
        Dictionary<HerbData, int> selectedPrescription
    )
    {
        if (disease == null || disease.requiredHerbs == null)
        {
            return "";
        }

        Dictionary<string, int> requiredAmounts =
            new Dictionary<string, int>();

        Dictionary<string, string> requiredNames =
            new Dictionary<string, string>();

        foreach (RequiredHerbAmount required in disease.requiredHerbs)
        {
            if (required == null || required.herb == null)
            {
                continue;
            }

            string herbKey = GetHerbKey(required.herb);

            if (string.IsNullOrEmpty(herbKey))
            {
                continue;
            }

            if (!requiredAmounts.ContainsKey(herbKey))
            {
                requiredAmounts.Add(herbKey, 0);
                requiredNames.Add(
                    herbKey,
                    required.herb.herbName
                );
            }

            requiredAmounts[herbKey] +=
                Mathf.Max(1, required.amount);
        }

        Dictionary<string, int> selectedAmounts =
            new Dictionary<string, int>();

        Dictionary<string, string> selectedNames =
            new Dictionary<string, string>();

        if (selectedPrescription != null)
        {
            foreach (
                KeyValuePair<HerbData, int> pair
                in selectedPrescription
            )
            {
                HerbData herb = pair.Key;
                int amount = pair.Value;

                if (herb == null || amount <= 0)
                {
                    continue;
                }

                string herbKey = GetHerbKey(herb);

                if (string.IsNullOrEmpty(herbKey))
                {
                    continue;
                }

                if (!selectedAmounts.ContainsKey(herbKey))
                {
                    selectedAmounts.Add(herbKey, 0);
                    selectedNames.Add(
                        herbKey,
                        herb.herbName
                    );
                }

                selectedAmounts[herbKey] += amount;
            }
        }

        List<string> missingHerbs = new List<string>();
        List<string> extraHerbs = new List<string>();

        // Kiểm tra thuốc thiếu hoặc thừa số lượng.
        foreach (
            KeyValuePair<string, int> requiredPair
            in requiredAmounts
        )
        {
            string herbKey = requiredPair.Key;
            int requiredAmount = requiredPair.Value;
            string herbName = requiredNames[herbKey];

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
                int extraAmount =
                    selectedAmount - requiredAmount;

                extraHerbs.Add(
                    herbName + " x" + extraAmount
                );
            }
        }

        // Thuốc người chơi kê nhưng không nằm trong đơn chuẩn.
        foreach (
            KeyValuePair<string, int> selectedPair
            in selectedAmounts
        )
        {
            if (requiredAmounts.ContainsKey(selectedPair.Key))
            {
                continue;
            }

            string herbName =
                selectedNames[selectedPair.Key];

            extraHerbs.Add(
                herbName + " x" + selectedPair.Value
            );
        }

        List<string> reminderLines = new List<string>();

        reminderLines.Add(
            "<b>Dược liệu cần:</b> "
            + GetRequiredHerbNames(disease)
        );

        if (missingHerbs.Count > 0)
        {
            reminderLines.Add(
                "<b>Thiếu:</b> "
                + string.Join(", ", missingHerbs)
            );
        }

        if (extraHerbs.Count > 0)
        {
            reminderLines.Add(
                "<b>Thừa:</b> "
                + string.Join(", ", extraHerbs)
            );
        }

        return string.Join("\n", reminderLines);
    }
}