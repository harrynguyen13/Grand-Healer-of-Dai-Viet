using System.Collections.Generic;
using System.Linq;
using UnityEngine;

[CreateAssetMenu(fileName = "MedicalDatabase", menuName = "Đông Y/Dữ liệu tổng y học")]
public class MedicalDatabase : ScriptableObject
{
    [Header("Toàn bộ bệnh")]
    public List<DiseaseData> diseases = new List<DiseaseData>();

    [Header("Toàn bộ dược liệu")]
    public List<HerbData> herbs = new List<HerbData>();

    public List<DiseaseData> GetUnlockedDiseases(int clinicLevel)
    {
        return diseases
            .Where(disease => disease != null && (int)disease.diseaseLevel <= clinicLevel)
            .ToList();
    }

    public List<HerbData> GetUnlockedHerbs(int clinicLevel)
    {
        return herbs
            .Where(herb => herb != null && herb.unlockClinicLevel <= clinicLevel)
            .ToList();
    }

    public DiseaseData GetRandomDisease(int clinicLevel)
    {
        List<DiseaseData> unlockedDiseases = GetUnlockedDiseases(clinicLevel);

        if (unlockedDiseases.Count == 0)
        {
            Debug.LogWarning("Không có bệnh nào được mở khóa.");
            return null;
        }

        return unlockedDiseases[Random.Range(0, unlockedDiseases.Count)];
    }

    public List<DiseaseData> GetDiagnosisOptions(DiseaseData realDisease, int optionCount, int clinicLevel)
    {
        List<DiseaseData> unlockedDiseases = GetUnlockedDiseases(clinicLevel);

        List<DiseaseData> options = unlockedDiseases
            .Where(disease => disease != null && disease != realDisease)
            .OrderBy(disease => Random.value)
            .Take(optionCount - 1)
            .ToList();

        options.Add(realDisease);

        return options
            .OrderBy(disease => Random.value)
            .ToList();
    }
}