using UnityEngine;

public static class SpecialYThuDiseaseService
{
    private const string SpecialDiseaseAddedKey = "SpecialYThu_DiseaseAdded";
    private const string SpecialDiseaseNameKey = "SpecialYThu_SelectedDiseaseName";

    private static SpecialDiseaseCase currentSpecialCase;

    public static void RegisterCase(SpecialDiseaseCase specialCase)
    {
        if (specialCase == null)
            return;

        currentSpecialCase = specialCase;
    }

    public static void AddToYThu(SpecialDiseaseCase specialCase)
    {
        if (specialCase == null)
            return;

        if (specialCase.SpecialDisease == null)
            return;

        if (string.IsNullOrWhiteSpace(specialCase.SelectedDiseaseName))
            return;

        currentSpecialCase = specialCase;

        PlayerPrefs.SetInt(SpecialDiseaseAddedKey, 1);
        PlayerPrefs.SetString(
            SpecialDiseaseNameKey,
            specialCase.SelectedDiseaseName.Trim()
        );

        PlayerPrefs.Save();

        Debug.Log("Đã thêm bệnh đặc biệt vào Y thư: " + specialCase.SelectedDiseaseName);
    }

    public static bool HasSpecialDiseaseInYThu()
    {
        return PlayerPrefs.GetInt(SpecialDiseaseAddedKey, 0) == 1;
    }

    public static bool HasSelectedDiseaseName()
    {
        return !string.IsNullOrWhiteSpace(GetSelectedDiseaseName());
    }

    public static string GetSelectedDiseaseName()
    {
        return PlayerPrefs.GetString(SpecialDiseaseNameKey, "");
    }

    public static SpecialDiseaseCase GetCurrentCase()
    {
        if (currentSpecialCase != null)
            return currentSpecialCase;

        currentSpecialCase = Object.FindAnyObjectByType<SpecialDiseaseCase>();

        return currentSpecialCase;
    }

    public static DiseaseData GetSpecialDisease()
    {
        SpecialDiseaseCase specialCase = GetCurrentCase();

        if (specialCase == null)
            return null;

        return specialCase.SpecialDisease;
    }

    public static void Reset()
    {
        PlayerPrefs.DeleteKey(SpecialDiseaseAddedKey);
        PlayerPrefs.DeleteKey(SpecialDiseaseNameKey);
        PlayerPrefs.Save();

        currentSpecialCase = null;

        Debug.Log("Đã reset bệnh đặc biệt trong Y thư.");
    }
}