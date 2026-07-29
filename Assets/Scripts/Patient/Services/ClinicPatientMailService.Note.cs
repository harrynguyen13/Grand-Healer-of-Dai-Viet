using System.Collections.Generic;
using System.Text;
using UnityEngine;

public static partial class ClinicPatientMailService
{
    private static string GenerateWrongReasonNote(
        DiseaseData selectedDisease,
        DiseaseData realDisease,
        Dictionary<HerbData, int> prescription,
        bool diagnosisCorrect,
        bool prescriptionCorrect
    )
    {
        if (diagnosisCorrect && prescriptionCorrect)
        {
            return "";
        }

        List<string> notes =
            new List<string>();

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

        StringBuilder builder =
            new StringBuilder();

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

        Dictionary<string, string>
            requiredDisplayNames =
                new Dictionary<string, string>();

        for (
            int i = 0;
            i < realDisease.requiredHerbs.Count;
            i++
        )
        {
            RequiredHerbAmount required =
                realDisease.requiredHerbs[i];

            if (
                required == null
                || required.herb == null
            )
            {
                continue;
            }

            string herbKey =
                NormalizeHerbName(
                    required.herb.herbName
                );

            if (string.IsNullOrEmpty(herbKey))
            {
                continue;
            }

            int requiredAmount =
                Mathf.Max(1, required.amount);

            if (
                !requiredAmounts.ContainsKey(
                    herbKey
                )
            )
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

        Dictionary<string, string>
            selectedDisplayNames =
                new Dictionary<string, string>();

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
                    NormalizeHerbName(
                        selected.Key.herbName
                    );

                if (
                    string.IsNullOrEmpty(
                        herbKey
                    )
                )
                {
                    continue;
                }

                if (
                    !selectedAmounts.ContainsKey(
                        herbKey
                    )
                )
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

        foreach (
            KeyValuePair<string, int> required
            in requiredAmounts
        )
        {
            string herbKey =
                required.Key;

            int requiredAmount =
                required.Value;

            string herbName =
                requiredDisplayNames[herbKey];

            requiredHerbs.Add(
                herbName
                + " x"
                + requiredAmount
            );

            selectedAmounts.TryGetValue(
                herbKey,
                out int selectedAmount
            );

            if (
                selectedAmount
                < requiredAmount
            )
            {
                int missingAmount =
                    requiredAmount
                    - selectedAmount;

                missingHerbs.Add(
                    herbName
                    + " x"
                    + missingAmount
                );
            }
            else if (
                selectedAmount
                > requiredAmount
            )
            {
                int excessAmount =
                    selectedAmount
                    - requiredAmount;

                excessHerbs.Add(
                    herbName
                    + " x"
                    + excessAmount
                );
            }
        }

        foreach (
            KeyValuePair<string, int> selected
            in selectedAmounts
        )
        {
            if (
                requiredAmounts.ContainsKey(
                    selected.Key
                )
            )
            {
                continue;
            }

            string herbName =
                selectedDisplayNames.ContainsKey(
                    selected.Key
                )
                    ? selectedDisplayNames[
                        selected.Key
                    ]
                    : selected.Key;

            excessHerbs.Add(
                herbName
                + " x"
                + selected.Value
            );
        }

        notes.Add(
            "<b>Dược liệu cần:</b> "
            + string.Join(
                ", ",
                requiredHerbs
            )
        );

        if (missingHerbs.Count > 0)
        {
            notes.Add(
                "<b>Thiếu:</b> "
                + string.Join(
                    ", ",
                    missingHerbs
                )
            );
        }

        if (excessHerbs.Count > 0)
        {
            notes.Add(
                "<b>Thừa:</b> "
                + string.Join(
                    ", ",
                    excessHerbs
                )
            );
        }

        if (
            missingHerbs.Count == 0
            && excessHerbs.Count == 0
        )
        {
            notes.Add(
                "Đơn thuốc chưa khớp "
                + "với phương thuốc chuẩn."
            );
        }
    }

    private static string NormalizeHerbName(
        string herbName
    )
    {
        if (
            string.IsNullOrWhiteSpace(
                herbName
            )
        )
        {
            return "";
        }

        return herbName
            .Trim()
            .ToLowerInvariant();
    }
}