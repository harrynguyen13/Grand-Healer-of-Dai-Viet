using UnityEngine;

public static class YThuBookTextFormatter
{
    public static string BuildDiseaseInfoText(DiseaseData disease)
    {
        if (disease == null)
            return "Không có dữ liệu bệnh.";

        string text = "";

        text += "<align=\"center\"><b>" + disease.diseaseName.ToUpper() + "</b>\n";
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

    public static string BuildPrescriptionText(DiseaseData disease)
    {
        if (disease == null)
            return "";

        string text = "";

        text += "<align=\"center\"><b>PHƯƠNG THUỐC</b></align>\n\n";

        if (disease.requiredHerbs == null || disease.requiredHerbs.Count == 0)
        {
            text += "- Chưa có dữ liệu vị thuốc.";
            return text;
        }

        for (int i = 0; i < disease.requiredHerbs.Count; i++)
        {
            RequiredHerbAmount required = disease.requiredHerbs[i];

            if (required == null || required.herb == null)
                continue;

            text += "- "
                + required.herb.herbName
                + " x"
                + Mathf.Max(1, required.amount)
                + "\n";
        }

        return text.TrimEnd();
    }

    public static string BuildEmptyDiseaseInfoText()
    {
        return
            "<align=\"center\"><b>Y THƯ</b></align>\n\n" +
            "Chưa có bệnh nào được mở khóa hoặc không tìm thấy kết quả phù hợp.";
    }

    public static string BuildLockedDiseaseInfoText(int nextLevel)
    {
        string text = "";

        text += "<align=\"center\"><b>TRANG CHƯA MỞ KHÓA</b></align>\n\n";
        text += "Kiến thức trong Y thư hiện chỉ ghi lại những bệnh phù hợp với cấp bậc hiện tại.\n\n";

        if (nextLevel > 0)
        {
            text += "Hãy lên <b>Cấp "
                + nextLevel
                + "</b> để mở khóa thêm kiến thức chữa bệnh.";
        }
        else
        {
            text += "Hãy tiếp tục nâng cao tay nghề để mở rộng Y thư.";
        }

        return text;
    }

    public static string BuildLockedPrescriptionText(int nextLevel, int nextLevelDiseaseCount)
    {
        string text = "";

        text += "<align=\"center\"><b>GỢI Ý</b></align>\n\n";

        if (nextLevel > 0)
        {
            text += "Cấp tiếp theo sẽ mở thêm:\n\n";
            text += "- " + nextLevelDiseaseCount + " bệnh mới\n";
            text += "- Nhiều chứng bệnh khó hơn\n";
            text += "- Các phương thuốc phức tạp hơn\n\n";
        }

        text += "Hãy chữa bệnh chính xác để tăng tín nhiệm và thăng cấp.";

        return text;
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