using UnityEngine;

public static partial class ClinicPatientMailService
{
    public static string GetPatientDisplayName(
        GameObject patientObject
    )
    {
        if (patientObject == null)
        {
            return GetRandomPatientName();
        }

        string rawName =
            patientObject.name;

        if (
            string.IsNullOrWhiteSpace(
                rawName
            )
        )
        {
            return GetRandomPatientName();
        }

        string cleanName =
            CleanPatientObjectName(rawName);

        if (
            string.IsNullOrWhiteSpace(
                cleanName
            )
        )
        {
            return GetRandomPatientName();
        }

        string compactKey =
            cleanName
                .ToLowerInvariant()
                .Replace(" ", "");

        if (compactKey.Contains("balao"))
        {
            return
                GetRandomOldFemalePatientName();
        }

        if (compactKey.Contains("laonong"))
        {
            return
                GetRandomOldMalePatientName();
        }

        if (compactKey.Contains("phunu"))
        {
            return
                GetRandomFemalePatientName();
        }

        if (compactKey.Contains("male"))
        {
            return
                GetRandomMalePatientName();
        }

        return GetRandomPatientName();
    }

    private static string CleanPatientObjectName(
        string rawName
    )
    {
        string cleanName =
            rawName;

        cleanName =
            cleanName.Replace(
                "(Clone)",
                ""
            );

        cleanName =
            cleanName.Replace(
                "PatientNPC_",
                ""
            );

        cleanName =
            cleanName.Replace(
                "PatientNPC",
                ""
            );

        cleanName =
            cleanName.Replace(
                "NPC_",
                ""
            );

        cleanName =
            cleanName.Replace(
                "_",
                " "
            );

        cleanName =
            cleanName.Trim();

        while (cleanName.Contains("  "))
        {
            cleanName =
                cleanName.Replace(
                    "  ",
                    " "
                );
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

        return GetRandomName(names);
    }

    private static string
        GetRandomOldMalePatientName()
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

        return GetRandomName(names);
    }

    private static string
        GetRandomOldFemalePatientName()
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

        return GetRandomName(names);
    }

    private static string
        GetRandomMalePatientName()
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

        return GetRandomName(names);
    }

    private static string
        GetRandomFemalePatientName()
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

        return GetRandomName(names);
    }

    private static string GetRandomName(
        string[] names
    )
    {
        if (
            names == null
            || names.Length == 0
        )
        {
            return "Bệnh nhân";
        }

        int index =
            Random.Range(
                0,
                names.Length
            );

        return names[index];
    }
}