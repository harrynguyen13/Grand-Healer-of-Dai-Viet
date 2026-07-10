using System.Collections.Generic;
using UnityEngine;

public static class YThuBookTextFormatter
{
    private const int MaxMainRoles = 6;
    private const int MaxSubRoles = 4;

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

        text += "<align=\"center\"><b>Phương thuốc</b></align>\n\n";

        if (disease.requiredHerbs == null || disease.requiredHerbs.Count == 0)
        {
            text += "<align=\"left\">- Chưa có dữ liệu vị thuốc.</align>";
            return text;
        }

        text += "<align=\"left\">";

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

        text += "</align>";

        text += "\n\n";
        text += "<align=\"center\"><b>Dược tính cần có</b></align>\n\n";

        string roleSummary = BuildTreatmentRoleSummary(disease);

        text += "<align=\"left\">";

        if (string.IsNullOrWhiteSpace(roleSummary))
        {
            text += "- Chưa có dữ liệu dược tính.";
        }
        else
        {
            text += roleSummary;
        }

        text += "</align>";

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

    private static string BuildTreatmentRoleSummary(DiseaseData disease)
    {
        if (disease == null || disease.requiredHerbs == null)
            return "";

        List<string> mainRoles = new List<string>();
        List<string> subRoles = new List<string>();

        bool hasMoreMainRoles = false;
        bool hasMoreSubRoles = false;

        for (int i = 0; i < disease.requiredHerbs.Count; i++)
        {
            RequiredHerbAmount required = disease.requiredHerbs[i];

            if (required == null || required.herb == null)
                continue;

            string roleText = required.herb.treatmentRoleText;

            if (string.IsNullOrWhiteSpace(roleText))
                roleText = GetFallbackTreatmentRole(required.herb);

            AddMainAndSubRolesLimited(
                mainRoles,
                subRoles,
                roleText,
                ref hasMoreMainRoles,
                ref hasMoreSubRoles
            );
        }

        RemoveDuplicateSubRolesAlreadyInMain(mainRoles, subRoles);

        if (mainRoles.Count == 0 && subRoles.Count == 0)
            return "";

        string result = "";

        if (mainRoles.Count > 0)
        {
            result += "<b>Chính:</b>\n";
            result += BuildRoleLine(mainRoles, hasMoreMainRoles);
        }

        if (subRoles.Count > 0)
        {
            if (!string.IsNullOrWhiteSpace(result))
                result += "\n\n";

            result += "<b>Phụ:</b>\n";
            result += BuildRoleLine(subRoles, hasMoreSubRoles);
        }

        return result.TrimEnd();
    }

    private static void AddMainAndSubRolesLimited(
        List<string> mainRoles,
        List<string> subRoles,
        string roleText,
        ref bool hasMoreMainRoles,
        ref bool hasMoreSubRoles
    )
    {
        if (mainRoles == null || subRoles == null)
            return;

        if (string.IsNullOrWhiteSpace(roleText))
            return;

        string normalizedText = roleText.Replace("/", ",");

        string[] parts = normalizedText.Split(',');

        if (parts.Length == 0)
            return;

        string mainRole = parts[0].Trim();

        if (!string.IsNullOrWhiteSpace(mainRole))
        {
            if (!ContainsRole(mainRoles, mainRole))
            {
                if (mainRoles.Count < MaxMainRoles)
                {
                    mainRoles.Add(mainRole);

                    // Nếu vai trò này trước đó từng bị thêm vào Phụ,
                    // thì xóa khỏi Phụ để tránh trùng Chính / Phụ.
                    RemoveRole(subRoles, mainRole);
                }
                else
                {
                    hasMoreMainRoles = true;
                }
            }
        }

        // Chỉ lấy 1 dược tính phụ đầu tiên của mỗi vị thuốc để tránh tràn trang Y thư.
        if (parts.Length >= 2)
        {
            string subRole = parts[1].Trim();

            if (string.IsNullOrWhiteSpace(subRole))
                return;

            // Nếu Phụ đã có trong Chính thì không thêm.
            if (ContainsRole(mainRoles, subRole))
                return;

            // Nếu Phụ đã có trong danh sách Phụ thì không thêm.
            if (ContainsRole(subRoles, subRole))
                return;

            if (subRoles.Count < MaxSubRoles)
            {
                subRoles.Add(subRole);
            }
            else
            {
                hasMoreSubRoles = true;
            }
        }
    }

    private static void RemoveDuplicateSubRolesAlreadyInMain(List<string> mainRoles, List<string> subRoles)
    {
        if (mainRoles == null || subRoles == null)
            return;

        for (int i = subRoles.Count - 1; i >= 0; i--)
        {
            if (ContainsRole(mainRoles, subRoles[i]))
            {
                subRoles.RemoveAt(i);
            }
        }
    }

    private static void RemoveRole(List<string> roles, string targetRole)
    {
        if (roles == null)
            return;

        if (string.IsNullOrWhiteSpace(targetRole))
            return;

        string normalizedTarget = NormalizeRoleForCompare(targetRole);

        for (int i = roles.Count - 1; i >= 0; i--)
        {
            if (NormalizeRoleForCompare(roles[i]) == normalizedTarget)
            {
                roles.RemoveAt(i);
            }
        }
    }

    private static string BuildRoleLine(List<string> roles, bool hasMoreRoles)
    {
        if (roles == null || roles.Count == 0)
            return "";

        string result = "";

        for (int i = 0; i < roles.Count; i++)
        {
            result += ToLowerFirstLetter(roles[i]);

            if (i < roles.Count - 1)
                result += ", ";
        }

        if (hasMoreRoles)
            result += " và các tác dụng phụ trợ khác.";
        else
            result += ".";

        return result;
    }

    private static bool ContainsRole(List<string> roles, string targetRole)
    {
        if (roles == null)
            return false;

        if (string.IsNullOrWhiteSpace(targetRole))
            return false;

        string normalizedTarget = NormalizeRoleForCompare(targetRole);

        for (int i = 0; i < roles.Count; i++)
        {
            if (NormalizeRoleForCompare(roles[i]) == normalizedTarget)
                return true;
        }

        return false;
    }

    private static string NormalizeRoleForCompare(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return "";

        string result = role.Trim().ToLower();

        while (result.Contains("  "))
        {
            result = result.Replace("  ", " ");
        }

        return result;
    }

    private static string ToLowerFirstLetter(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";

        text = text.Trim();

        if (text.Length == 1)
            return text.ToLower();

        return char.ToLower(text[0]) + text.Substring(1);
    }

    private static string GetFallbackTreatmentRole(HerbData herb)
    {
        if (herb == null)
            return "hỗ trợ điều trị";

        switch (herb.category)
        {
            case HerbCategory.GiaiBieu:
                return "giải biểu, khu phong";

            case HerbCategory.ThanhNhiet:
                return "thanh nhiệt, giải độc";

            case HerbCategory.HoaDamChiHo:
                return "hóa đờm, chỉ ho";

            case HerbCategory.LyKhi:
                return "lý khí, hành khí";

            case HerbCategory.TieuThuc:
                return "tiêu thực, hỗ trợ tiêu hóa";

            case HerbCategory.HoatHuyet:
                return "hoạt huyết, hóa ứ";

            case HerbCategory.LoiThuy:
                return "lợi thủy, thông tiểu";

            case HerbCategory.BoKhiHuyet:
                return "bổ khí huyết, phục hồi";

            case HerbCategory.BoThan:
                return "bổ thận, mạnh gân cốt";

            case HerbCategory.AnThan:
                return "an thần, dưỡng tâm";

            case HerbCategory.DocTinh:
                return "dược tính mạnh, dùng thận trọng";

            case HerbCategory.Khac:
                return "hỗ trợ điều trị theo phối ngũ";

            default:
                return "hỗ trợ điều trị theo phối ngũ";
        }
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