public static partial class YThuBookTextFormatter
{
    public static string BuildDiseaseInfoText(DiseaseData disease)
    {
        return BuildDiseaseInfoText(disease, "");
    }

    public static string BuildDiseaseInfoText(
        DiseaseData disease,
        string overrideDiseaseName
    )
    {
        if (disease == null)
            return "Không có dữ liệu bệnh.";

        string displayName = disease.diseaseName;

        if (!string.IsNullOrWhiteSpace(overrideDiseaseName))
            displayName = overrideDiseaseName.Trim();

        string text = "";

        text += "<align=\"center\"><b>" + displayName.ToUpper() + "</b>\n";
        text += "Cấp " + (int)disease.diseaseLevel + "\n";
        text += "Nhóm: " + GetDiseaseGroupName(disease.diseaseGroup) + "</align>\n\n";

        if (!string.IsNullOrWhiteSpace(disease.description))
        {
            text += disease.description.Trim() + "\n\n";
        }

        text += "<b>Triệu chứng:</b>\n";

        if (disease.symptoms == null || disease.symptoms.Count == 0)
        {
            text += "- Chưa có dữ liệu triệu chứng.";
            return text;
        }

        for (int i = 0; i < disease.symptoms.Count; i++)
        {
            SymptomData symptom = disease.symptoms[i];

            if (symptom == null || string.IsNullOrWhiteSpace(symptom.symptomText))
                continue;

            text += "- " + symptom.symptomText.Trim() + "\n";
        }

        return text.TrimEnd();
    }

    private static string GetDiseaseGroupName(DiseaseGroup group)
    {
        switch (group)
        {
            case DiseaseGroup.HoHap:
                return "Hô hấp";

            case DiseaseGroup.TieuHoa:
                return "Tiêu hóa";

            case DiseaseGroup.ThanKinh:
                return "Thần kinh";

            case DiseaseGroup.TimMach:
                return "Tim mạch";

            case DiseaseGroup.CoXuongKhop:
                return "Cơ xương khớp";

            case DiseaseGroup.TietNieu:
                return "Tiết niệu";

            case DiseaseGroup.DaLieu:
                return "Da liễu";

            case DiseaseGroup.NgoaiKhoa:
                return "Ngoại khoa";

            case DiseaseGroup.DocTo:
                return "Độc tố";

            case DiseaseGroup.Khac:
                return "Khác";

            default:
                return "Không rõ";
        }
    }
}