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

    private const int SpecialDiseaseLevel = 5;

    // =========================================================
    // API CHUẨN: TẤT CẢ ĐỀU TỰ LẤY CẤP TỪ PlayerLevelService
    // =========================================================

    public List<DiseaseData> GetUnlockedDiseases()
    {
        int currentLevel = GetCurrentClinicLevel();
        return GetUnlockedDiseasesByLevel(currentLevel);
    }

    public List<HerbData> GetUnlockedHerbs()
    {
        int currentLevel = GetCurrentClinicLevel();
        return GetUnlockedHerbsByLevel(currentLevel);
    }

    public DiseaseData GetRandomDisease()
    {
        int currentLevel = GetCurrentClinicLevel();

        List<DiseaseData> unlockedDiseases = GetUnlockedDiseasesByLevel(currentLevel);

        if (unlockedDiseases.Count == 0)
        {
            Debug.LogWarning("Không có bệnh nào được mở khóa ở cấp: " + currentLevel);
            return null;
        }

        DiseaseData randomDisease = unlockedDiseases[Random.Range(0, unlockedDiseases.Count)];

        Debug.Log("MedicalDatabase random bệnh theo cấp: " + currentLevel);
        Debug.Log("Bệnh random được: " + randomDisease.diseaseName);

        return randomDisease;
    }

    public List<DiseaseData> GetDiagnosisOptions(DiseaseData realDisease, int optionCount)
    {
        int currentLevel = GetCurrentClinicLevel();

        optionCount = Mathf.Max(1, optionCount);

        if (realDisease == null)
        {
            Debug.LogWarning("RealDisease bị null.");
            return new List<DiseaseData>();
        }

        List<DiseaseData> unlockedDiseases = GetUnlockedDiseasesByLevel(currentLevel);

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

    // =========================================================
    // HÀM NỘI BỘ: CHỈ MedicalDatabase ĐƯỢC DÙNG LEVEL
    // File khác không gọi trực tiếp nữa
    // =========================================================

    private List<DiseaseData> GetUnlockedDiseasesByLevel(int clinicLevel)
    {
        clinicLevel = Mathf.Max(1, clinicLevel);

        return diseases
            .Where(disease =>
                disease != null &&
                (int)disease.diseaseLevel <= clinicLevel &&
                !IsSpecialDisease(disease)
            )
            .ToList();
    }

    private List<HerbData> GetUnlockedHerbsByLevel(int clinicLevel)
    {
        clinicLevel = Mathf.Max(1, clinicLevel);

        return herbs
            .Where(herb =>
                herb != null &&
                herb.unlockClinicLevel <= clinicLevel
            )
            .ToList();
    }

    // =========================================================
    // BỆNH ĐẶC BIỆT
    // =========================================================

    public DiseaseData GetSpecialDiseaseByAssetName(string assetName)
    {
        if (string.IsNullOrWhiteSpace(assetName))
        {
            Debug.LogWarning("AssetName bệnh đặc biệt bị rỗng.");
            return null;
        }

        DiseaseData disease = diseases.FirstOrDefault(d =>
            d != null &&
            d.name == assetName &&
            IsSpecialDisease(d)
        );

        if (disease == null)
        {
            Debug.LogWarning("Không tìm thấy bệnh đặc biệt có asset name: " + assetName);
        }

        return disease;
    }

    public DiseaseData GetSpecialDiseaseByDiseaseName(string diseaseName)
    {
        if (string.IsNullOrWhiteSpace(diseaseName))
        {
            Debug.LogWarning("DiseaseName bệnh đặc biệt bị rỗng.");
            return null;
        }

        DiseaseData disease = diseases.FirstOrDefault(d =>
            d != null &&
            d.diseaseName == diseaseName &&
            IsSpecialDisease(d)
        );

        if (disease == null)
        {
            Debug.LogWarning("Không tìm thấy bệnh đặc biệt có tên: " + diseaseName);
        }

        return disease;
    }

    private bool IsSpecialDisease(DiseaseData disease)
    {
        if (disease == null)
            return false;

        return (int)disease.diseaseLevel == SpecialDiseaseLevel;
    }

    public List<DiseaseData> GetUnlockedDiseases(int currentClinicLevel)
    {
        List<DiseaseData> unlockedDiseases = new List<DiseaseData>();

        if (diseases == null)
            return unlockedDiseases;

        currentClinicLevel = Mathf.Max(1, currentClinicLevel);

        for (int i = 0; i < diseases.Count; i++)
        {
            DiseaseData disease = diseases[i];

            if (disease == null)
                continue;

            int diseaseLevel = Mathf.Max(1, (int)disease.diseaseLevel);

            if (diseaseLevel <= currentClinicLevel)
            {
                unlockedDiseases.Add(disease);
            }
        }

        return unlockedDiseases;
    }

    // =========================================================
    // NGUỒN CẤP GỐC
    // =========================================================

    private int GetCurrentClinicLevel()
    {
        int currentLevel = PlayerLevelService.GetCurrentUnlockLevel();

        if (currentLevel > 0)
            return currentLevel;

        return 1;
    }
}

