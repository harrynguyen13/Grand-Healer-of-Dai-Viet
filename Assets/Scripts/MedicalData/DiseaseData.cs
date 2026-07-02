using System.Collections.Generic;
using UnityEngine;

public enum DiseaseLevel
{
    [InspectorName("Mức 1 - Dễ")]
    Level1 = 1,

    [InspectorName("Mức 2 - Trung bình")]
    Level2 = 2,

    [InspectorName("Mức 3 - Khó")]
    Level3 = 3,

    [InspectorName("Mức 4 - Cực khó")]
    Level4 = 4,

    [InspectorName("Mức 5 - Nhiệm vụ đặc biệt")]
    Level5 = 5
}

public enum DiseaseGroup
{
    [InspectorName("Hô hấp")]
    HoHap,

    [InspectorName("Tiêu hóa")]
    TieuHoa,

    [InspectorName("Thần kinh")]
    ThanKinh,

    [InspectorName("Tim mạch")]
    TimMach,

    [InspectorName("Cơ xương khớp")]
    CoXuongKhop,

    [InspectorName("Tiết niệu")]
    TietNieu,

    [InspectorName("Da liễu")]
    DaLieu,

    [InspectorName("Ngoại khoa")]
    NgoaiKhoa,

    [InspectorName("Độc tố")]
    DocTo,

    [InspectorName("Khác")]
    Khac
}

public enum ExaminationStep
{
    [InspectorName("Hỏi bệnh")]
    Ask,

    [InspectorName("Bắt mạch")]
    PulseCheck,

    [InspectorName("Ẩn / nâng cao")]
    Hidden
}

[System.Serializable]
public class SymptomData
{
    [TextArea(2, 4)]
    public string symptomText;

    public ExaminationStep showAtStep = ExaminationStep.Ask;
}

[CreateAssetMenu(fileName = "NewDisease", menuName = "Đông Y/Dữ liệu bệnh")]
public class DiseaseData : ScriptableObject
{
    [Header("Thông tin bệnh")]
    public string diseaseName;
    public DiseaseLevel diseaseLevel;
    public DiseaseGroup diseaseGroup;

    [TextArea(3, 8)]
    public string description;

    [Header("Câu NPC kể khi hỏi bệnh")]
    [TextArea(2, 5)]
    public string patientDialogue;

    [Header("Triệu chứng")]
    public List<SymptomData> symptoms = new List<SymptomData>();

    [Header("Dược liệu đúng")]
    public List<HerbData> correctHerbs = new List<HerbData>();

    [Header("Tỷ lệ thuốc đúng tối thiểu")]
    [Range(0f, 1f)]
    public float medicineCorrectRate = 0.7f;

    [Header("Thưởng / phạt")]
    public int perfectReputation = 20;
    public int perfectMoney = 30;

    public int rightDiseaseWrongMedicineReputation = 5;
    public int rightDiseaseWrongMedicineMoney = 10;

    public int wrongDiseaseRightMedicinePenalty = 5;
    public int failedPenalty = 15;

    [Header("Thời gian chờ kết quả")]
    public float resultDelaySeconds = 20f;
}