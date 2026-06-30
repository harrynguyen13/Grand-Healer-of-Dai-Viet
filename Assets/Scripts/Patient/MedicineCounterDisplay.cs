using UnityEngine;

public class MedicineCounterDisplay : MonoBehaviour
{
    [Header("Object thuốc trên quầy")]
    [SerializeField] private GameObject medicineOnCounterObject;
    [SerializeField] private SpriteRenderer medicineOnCounterRenderer;

    [Header("Icon thuốc mặc định")]
    [SerializeField] private Sprite[] defaultMedicineSprites;

    [Header("Icon thuốc dạng thang/gói")]
    [SerializeField] private Sprite[] packageMedicineSprites;

    [Header("Icon thuốc dạng lọ viên")]
    [SerializeField] private Sprite[] pillBottleMedicineSprites;

    [Header("Icon thuốc dạng lọ bôi")]
    [SerializeField] private Sprite[] topicalMedicineSprites;

    private void Awake()
    {
        Hide();
    }

    public void ShowForDisease(DiseaseData disease)
    {
        Sprite selectedSprite = GetRandomMedicineSpriteByDisease(disease);

        if (medicineOnCounterRenderer != null && selectedSprite != null)
        {
            medicineOnCounterRenderer.sprite = selectedSprite;
        }

        if (medicineOnCounterObject != null)
        {
            medicineOnCounterObject.SetActive(true);
        }

        if (disease != null)
        {
            Debug.Log("Đã đặt thuốc lên quầy theo bệnh: " + disease.diseaseName);
        }
        else
        {
            Debug.Log("Đã đặt thuốc lên quầy bằng icon mặc định.");
        }
    }

    public void Hide()
    {
        if (medicineOnCounterObject != null)
        {
            medicineOnCounterObject.SetActive(false);
        }
    }

    private Sprite GetRandomMedicineSpriteByDisease(DiseaseData disease)
    {
        if (disease == null)
            return GetRandomSprite(defaultMedicineSprites);

        string searchText = BuildDiseaseSearchText(disease);

        // Bệnh ngoài da / cần thuốc bôi
        if (ContainsAny(searchText,
            "mụn", "nhọt", "lở", "loét", "ngứa", "da", "ngoài da", "sưng", "trĩ", "bôi", "ghẻ"))
        {
            return GetRandomSprite(topicalMedicineSprites, defaultMedicineSprites);
        }

        // Bệnh hư yếu / bổ / thuốc hoàn viên
        if (ContainsAny(searchText,
            "hư", "yếu", "suy", "thận", "can", "tâm", "mất ngủ", "bổ",
            "đau lưng", "mỏi gối", "hoa mắt", "chóng mặt",
            "di tinh", "liệt dương", "khí huyết"))
        {
            return GetRandomSprite(pillBottleMedicineSprites, defaultMedicineSprites);
        }

        // Bệnh tiêu hóa
        if (ContainsAny(searchText,
            "tiêu chảy", "tả", "lỵ", "đau bụng", "đầy bụng",
            "ăn không tiêu", "táo bón", "buồn nôn", "nôn",
            "dạ dày", "tỳ vị", "phân lỏng"))
        {
            return GetRandomSprite(packageMedicineSprites, pillBottleMedicineSprites, defaultMedicineSprites);
        }

        // Bệnh cảm ho sốt
        if (ContainsAny(searchText,
            "cảm", "ho", "sốt", "phong hàn", "phong nhiệt",
            "đau họng", "sổ mũi", "ngạt mũi", "đờm", "khó thở"))
        {
            return GetRandomSprite(packageMedicineSprites, defaultMedicineSprites);
        }

        return GetRandomSprite(defaultMedicineSprites, packageMedicineSprites, pillBottleMedicineSprites);
    }

    private string BuildDiseaseSearchText(DiseaseData disease)
    {
        string result = "";

        if (disease == null)
            return result;

        result += " " + disease.diseaseName;
        result += " " + disease.patientDialogue;

        if (disease.symptoms != null)
        {
            foreach (SymptomData symptom in disease.symptoms)
            {
                if (symptom == null)
                    continue;

                result += " " + symptom.symptomText;
            }
        }

        return result.ToLowerInvariant();
    }

    private bool ContainsAny(string text, params string[] keywords)
    {
        if (string.IsNullOrEmpty(text))
            return false;

        foreach (string keyword in keywords)
        {
            if (string.IsNullOrEmpty(keyword))
                continue;

            if (text.Contains(keyword.ToLowerInvariant()))
                return true;
        }

        return false;
    }

    private Sprite GetRandomSprite(params Sprite[][] spriteGroups)
    {
        foreach (Sprite[] sprites in spriteGroups)
        {
            if (sprites == null || sprites.Length == 0)
                continue;

            int randomIndex = Random.Range(0, sprites.Length);
            return sprites[randomIndex];
        }

        return null;
    }
}