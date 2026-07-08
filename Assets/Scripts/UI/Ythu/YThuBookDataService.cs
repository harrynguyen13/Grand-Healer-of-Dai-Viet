using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEngine;

public static class YThuBookDataService
{
    private const int SpecialDiseaseLevel = 5;

    public static List<DiseaseData> GetUnlockedDiseasesForBook(
        MedicalDatabase medicalDatabase,
        bool includeSpecialLevelDiseases
    )
    {
        List<DiseaseData> result = new List<DiseaseData>();

        if (medicalDatabase == null || medicalDatabase.diseases == null)
            return result;

        int currentClinicLevel = GetCurrentClinicLevel();

        for (int i = 0; i < medicalDatabase.diseases.Count; i++)
        {
            DiseaseData disease = medicalDatabase.diseases[i];

            if (disease == null)
                continue;

            if (!ShouldShowDiseaseInBook(disease, includeSpecialLevelDiseases))
                continue;

            if ((int)disease.diseaseLevel <= currentClinicLevel)
            {
                result.Add(disease);
            }
        }

        result.Sort(CompareDiseaseForBook);

        return result;
    }

    public static bool HasLockedDiseasesAboveCurrentLevel(
        MedicalDatabase medicalDatabase,
        bool includeSpecialLevelDiseases
    )
    {
        if (medicalDatabase == null || medicalDatabase.diseases == null)
            return false;

        int currentClinicLevel = GetCurrentClinicLevel();

        for (int i = 0; i < medicalDatabase.diseases.Count; i++)
        {
            DiseaseData disease = medicalDatabase.diseases[i];

            if (disease == null)
                continue;

            if (!ShouldShowDiseaseInBook(disease, includeSpecialLevelDiseases))
                continue;

            if ((int)disease.diseaseLevel > currentClinicLevel)
                return true;
        }

        return false;
    }

    public static int GetNextLockedLevel(
        MedicalDatabase medicalDatabase,
        bool includeSpecialLevelDiseases
    )
    {
        if (medicalDatabase == null || medicalDatabase.diseases == null)
            return -1;

        int currentClinicLevel = GetCurrentClinicLevel();
        int nextLevel = int.MaxValue;

        for (int i = 0; i < medicalDatabase.diseases.Count; i++)
        {
            DiseaseData disease = medicalDatabase.diseases[i];

            if (disease == null)
                continue;

            if (!ShouldShowDiseaseInBook(disease, includeSpecialLevelDiseases))
                continue;

            int diseaseLevel = (int)disease.diseaseLevel;

            if (diseaseLevel > currentClinicLevel && diseaseLevel < nextLevel)
            {
                nextLevel = diseaseLevel;
            }
        }

        if (nextLevel == int.MaxValue)
            return -1;

        return nextLevel;
    }

    public static int CountDiseasesAtLevel(
        MedicalDatabase medicalDatabase,
        int level,
        bool includeSpecialLevelDiseases
    )
    {
        if (medicalDatabase == null || medicalDatabase.diseases == null)
            return 0;

        int count = 0;

        for (int i = 0; i < medicalDatabase.diseases.Count; i++)
        {
            DiseaseData disease = medicalDatabase.diseases[i];

            if (disease == null)
                continue;

            if (!ShouldShowDiseaseInBook(disease, includeSpecialLevelDiseases))
                continue;

            if ((int)disease.diseaseLevel == level)
            {
                count++;
            }
        }

        return count;
    }

    public static List<DiseaseData> FilterDiseases(
        List<DiseaseData> sourceDiseases,
        string keyword
    )
    {
        List<DiseaseData> result = new List<DiseaseData>();

        if (sourceDiseases == null)
            return result;

        keyword = NormalizeSearchText(keyword);

        if (string.IsNullOrWhiteSpace(keyword))
        {
            result.AddRange(sourceDiseases);
            return result;
        }

        for (int i = 0; i < sourceDiseases.Count; i++)
        {
            DiseaseData disease = sourceDiseases[i];

            if (disease == null)
                continue;

            if (DoesDiseaseMatchSearch(disease, keyword))
            {
                result.Add(disease);
            }
        }

        return result;
    }

    public static string NormalizeSearchText(string value)
    {
        if (string.IsNullOrWhiteSpace(value))
            return "";

        string normalized = value
            .Trim()
            .ToLowerInvariant()
            .Normalize(NormalizationForm.FormD);

        StringBuilder builder = new StringBuilder();

        for (int i = 0; i < normalized.Length; i++)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(normalized[i]);

            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(normalized[i]);
            }
        }

        string result = builder.ToString().Normalize(NormalizationForm.FormC);
        result = result.Replace("đ", "d");

        return result;
    }

    private static int GetCurrentClinicLevel()
    {
        int currentLevel = PlayerLevelService.GetCurrentUnlockLevel();

        if (currentLevel > 0)
            return currentLevel;

        return 1;
    }

    private static bool ShouldShowDiseaseInBook(
        DiseaseData disease,
        bool includeSpecialLevelDiseases
    )
    {
        if (disease == null)
            return false;

        int level = (int)disease.diseaseLevel;

        if (!includeSpecialLevelDiseases && level == SpecialDiseaseLevel)
            return false;

        return true;
    }

    private static bool DoesDiseaseMatchSearch(DiseaseData disease, string keyword)
    {
        if (disease == null)
            return false;

        if (NormalizeSearchText(disease.diseaseName).Contains(keyword))
            return true;

        if (NormalizeSearchText(disease.description).Contains(keyword))
            return true;

        if (NormalizeSearchText(disease.patientDialogue).Contains(keyword))
            return true;

        if (disease.symptoms != null)
        {
            for (int i = 0; i < disease.symptoms.Count; i++)
            {
                SymptomData symptom = disease.symptoms[i];

                if (symptom == null)
                    continue;

                if (NormalizeSearchText(symptom.symptomText).Contains(keyword))
                    return true;
            }
        }

        if (disease.requiredHerbs != null)
        {
            for (int i = 0; i < disease.requiredHerbs.Count; i++)
            {
                RequiredHerbAmount required = disease.requiredHerbs[i];

                if (required == null || required.herb == null)
                    continue;

                if (NormalizeSearchText(required.herb.herbName).Contains(keyword))
                    return true;
            }
        }

        return false;
    }

    private static int CompareDiseaseForBook(DiseaseData a, DiseaseData b)
    {
        if (a == null && b == null)
            return 0;

        if (a == null)
            return 1;

        if (b == null)
            return -1;

        int levelCompare = ((int)a.diseaseLevel).CompareTo((int)b.diseaseLevel);

        if (levelCompare != 0)
            return levelCompare;

        return string.Compare(a.diseaseName, b.diseaseName, System.StringComparison.Ordinal);
    }
}