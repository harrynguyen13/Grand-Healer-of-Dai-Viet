using System.Collections.Generic;
using UnityEngine;

public static class YThuBookTextFormatter
{
    private const int MaxRolesPerGroup = 6;
    private const int MaxRolesPerHerb = 2;

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

        List<string> chiefRoles = new List<string>();
        List<string> assistantRoles = new List<string>();
        List<string> harmonyRoles = new List<string>();
        List<string> strongRoles = new List<string>();

        bool hasMoreChiefRoles = false;
        bool hasMoreAssistantRoles = false;
        bool hasMoreHarmonyRoles = false;
        bool hasMoreStrongRoles = false;

        for (int i = 0; i < disease.requiredHerbs.Count; i++)
        {
            RequiredHerbAmount required = disease.requiredHerbs[i];

            if (required == null || required.herb == null)
                continue;

            HerbData herb = required.herb;
            int amount = Mathf.Max(1, required.amount);

            List<string> herbRoles = ExtractImportantRolesFromHerb(herb);

            if (herbRoles.Count == 0)
                continue;

            if (IsStrongOrToxicHerb(herb))
            {
                AddRolesLimited(strongRoles, herbRoles, ref hasMoreStrongRoles);
                continue;
            }

            if (amount <= 8 && amount >= 6)
            {
                AddRolesLimited(chiefRoles, herbRoles, ref hasMoreChiefRoles);
            }
            else if (amount >= 3 && amount <= 5)
            {
                AddRolesLimited(assistantRoles, herbRoles, ref hasMoreAssistantRoles);
            }
            else
            {
                AddRolesLimited(harmonyRoles, herbRoles, ref hasMoreHarmonyRoles);
            }
        }

        RemoveDuplicateRoles(chiefRoles, assistantRoles);
        RemoveDuplicateRoles(chiefRoles, harmonyRoles);
        RemoveDuplicateRoles(assistantRoles, harmonyRoles);
        RemoveDuplicateRoles(chiefRoles, strongRoles);
        RemoveDuplicateRoles(assistantRoles, strongRoles);
        RemoveDuplicateRoles(harmonyRoles, strongRoles);

        string result = "";

        if (chiefRoles.Count > 0)
        {
            result += "<b>Chủ dược:</b>\n";
            result += BuildRoleLine(chiefRoles, hasMoreChiefRoles);
        }

        if (assistantRoles.Count > 0)
        {
            if (!string.IsNullOrWhiteSpace(result))
                result += "\n\n";

            result += "<b>Phụ dược:</b>\n";
            result += BuildRoleLine(assistantRoles, hasMoreAssistantRoles);
        }

        if (harmonyRoles.Count > 0)
        {
            if (!string.IsNullOrWhiteSpace(result))
                result += "\n\n";

            result += "<b>Điều hòa:</b>\n";
            result += BuildRoleLine(harmonyRoles, hasMoreHarmonyRoles);
        }

        if (strongRoles.Count > 0)
        {
            if (!string.IsNullOrWhiteSpace(result))
                result += "\n\n";

            result += "<b>Dược mạnh / độc:</b>\n";
            result += BuildRoleLine(strongRoles, hasMoreStrongRoles);
        }

        return result.TrimEnd();
    }

    private static List<string> ExtractImportantRolesFromHerb(HerbData herb)
    {
        List<string> roles = new List<string>();

        if (herb == null)
            return roles;

        string roleText = herb.treatmentRoleText;

        if (string.IsNullOrWhiteSpace(roleText))
            return roles;

        roleText = NormalizeRoleText(roleText);

        char[] splitChars = new char[]
        {
            ',',
            '/',
            ';',
            '\n'
        };

        string[] parts = roleText.Split(splitChars);

        for (int i = 0; i < parts.Length; i++)
        {
            string role = CleanRoleName(parts[i]);

            if (string.IsNullOrWhiteSpace(role))
                continue;

            if (ContainsRole(roles, role))
                continue;

            roles.Add(role);

            if (roles.Count >= MaxRolesPerHerb)
                break;
        }

        return roles;
    }

    private static string NormalizeRoleText(string roleText)
    {
        if (string.IsNullOrWhiteSpace(roleText))
            return "";

        string result = roleText;

        result = result.Replace("\r", "\n");

        result = result.Replace("<b>", "");
        result = result.Replace("</b>", "");

        result = result.Replace("Chính:", "\n");
        result = result.Replace("Chính :", "\n");
        result = result.Replace("Phụ:", "\n");
        result = result.Replace("Phụ :", "\n");

        result = result.Replace("chính:", "\n");
        result = result.Replace("chính :", "\n");
        result = result.Replace("phụ:", "\n");
        result = result.Replace("phụ :", "\n");

        return result;
    }

    private static void AddRolesLimited(List<string> targetRoles, List<string> sourceRoles, ref bool hasMoreRoles)
    {
        if (targetRoles == null || sourceRoles == null)
            return;

        for (int i = 0; i < sourceRoles.Count; i++)
        {
            string role = sourceRoles[i];

            if (string.IsNullOrWhiteSpace(role))
                continue;

            if (ContainsRole(targetRoles, role))
                continue;

            if (targetRoles.Count < MaxRolesPerGroup)
            {
                targetRoles.Add(role);
            }
            else
            {
                hasMoreRoles = true;
            }
        }
    }

    private static void RemoveDuplicateRoles(List<string> sourceRoles, List<string> targetRoles)
    {
        if (sourceRoles == null || targetRoles == null)
            return;

        for (int i = targetRoles.Count - 1; i >= 0; i--)
        {
            if (ContainsRole(sourceRoles, targetRoles[i]))
            {
                targetRoles.RemoveAt(i);
            }
        }
    }

    private static string CleanRoleName(string role)
    {
        if (string.IsNullOrWhiteSpace(role))
            return "";

        string result = role.Trim();

        result = result.TrimStart('-', '+', ' ');
        result = result.TrimEnd('.', ',', ';', ':', ' ');

        while (result.Contains("  "))
        {
            result = result.Replace("  ", " ");
        }

        return result.Trim();
    }

    private static bool IsStrongOrToxicHerb(HerbData herb)
    {
        if (herb == null)
            return false;

        if (herb.category == HerbCategory.DocTinh)
            return true;

        if (herb.rarity == HerbRarity.Toxic)
            return true;

        return false;
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
            result += " và tác dụng khác.";
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