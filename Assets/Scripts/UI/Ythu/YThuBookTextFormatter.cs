using System.Collections.Generic;
using UnityEngine;

public static class YThuBookTextFormatter
{
    private const int MaxRolesPerGroup = 6;
    private const int MaxRolesPerHerb = 2;

    private const int MaxPrescriptionLinePerPage = 20;
    private const int MaxCharacterPerLine = 30;

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
        List<string> pages = BuildPrescriptionPages(disease);

        if (pages == null || pages.Count == 0)
            return "";

        return pages[0];
    }

    public static List<string> BuildPrescriptionPages(DiseaseData disease)
    {
        List<string> pages = new List<string>();

        if (disease == null)
            return pages;

        List<string> currentLines = new List<string>();
        int currentLineCount = 0;

        AddPageLine(
            pages,
            currentLines,
            ref currentLineCount,
            "<align=\"center\"><b>Phương thuốc</b></align>"
        );

        AddPageLine(pages, currentLines, ref currentLineCount, "");

        if (disease.requiredHerbs == null || disease.requiredHerbs.Count == 0)
        {
            AddPageLine(
                pages,
                currentLines,
                ref currentLineCount,
                "<align=\"left\">- Chưa có dữ liệu vị thuốc.</align>"
            );
        }
        else
        {
            for (int i = 0; i < disease.requiredHerbs.Count; i++)
            {
                RequiredHerbAmount required = disease.requiredHerbs[i];

                if (required == null || required.herb == null)
                    continue;

                AddPageLine(
                    pages,
                    currentLines,
                    ref currentLineCount,
                    "<align=\"left\">- "
                    + required.herb.herbName
                    + " x"
                    + Mathf.Max(1, required.amount)
                    + "</align>"
                );
            }
        }

        string roleSummary = BuildTreatmentRoleSummary(disease);

        if (!string.IsNullOrWhiteSpace(roleSummary))
        {
            AddPageLine(pages, currentLines, ref currentLineCount, "");
            AddPageLine(
                pages,
                currentLines,
                ref currentLineCount,
                "<align=\"center\"><b>Dược tính cần có</b></align>"
            );
            AddPageLine(pages, currentLines, ref currentLineCount, "");

            string[] roleLines = roleSummary.Split('\n');

            for (int i = 0; i < roleLines.Length; i++)
            {
                string line = roleLines[i];

                if (string.IsNullOrWhiteSpace(line))
                {
                    AddPageLine(pages, currentLines, ref currentLineCount, "");
                    continue;
                }

                AddPageLine(
                    pages,
                    currentLines,
                    ref currentLineCount,
                    "<align=\"left\">" + line.Trim() + "</align>"
                );
            }
        }

        FlushPrescriptionPage(pages, currentLines);

        return pages;
    }

    private static void AddPageLine(
        List<string> pages,
        List<string> currentLines,
        ref int currentLineCount,
        string line
    )
    {
        bool isBlankLine = string.IsNullOrWhiteSpace(line);

        if (isBlankLine && currentLines.Count == 0)
            return;

        int lineCost = isBlankLine ? 1 : EstimateLineCount(line);

        if (currentLineCount > 0 && currentLineCount + lineCost > MaxPrescriptionLinePerPage)
        {
            FlushPrescriptionPage(pages, currentLines);
            currentLineCount = 0;

            if (isBlankLine)
                return;
        }

        currentLines.Add(line);
        currentLineCount += lineCost;
    }

    private static void FlushPrescriptionPage(
        List<string> pages,
        List<string> currentLines
    )
    {
        if (currentLines == null || currentLines.Count == 0)
            return;

        while (currentLines.Count > 0 && string.IsNullOrWhiteSpace(currentLines[currentLines.Count - 1]))
        {
            currentLines.RemoveAt(currentLines.Count - 1);
        }

        if (currentLines.Count == 0)
            return;

        string pageText = string.Join("\n", currentLines).TrimEnd();

        if (!string.IsNullOrWhiteSpace(pageText))
            pages.Add(pageText);

        currentLines.Clear();
    }

    private static int EstimateLineCount(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return 1;

        string cleanText = StripRichTextTags(text);

        if (string.IsNullOrWhiteSpace(cleanText))
            return 1;

        int estimatedLineCount = Mathf.CeilToInt((float)cleanText.Length / MaxCharacterPerLine);

        if (estimatedLineCount < 1)
            estimatedLineCount = 1;

        return estimatedLineCount;
    }

    private static string StripRichTextTags(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";

        string result = text;

        result = result.Replace("<b>", "");
        result = result.Replace("</b>", "");

        result = result.Replace("<align=\"center\">", "");
        result = result.Replace("<align=\"left\">", "");
        result = result.Replace("</align>", "");

        return result.Trim();
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

        bool hasMoreChiefRoles = false;
        bool hasMoreAssistantRoles = false;
        bool hasMoreHarmonyRoles = false;

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

            if (amount >= 6 && amount <= 8)
            {
                AddRolesLimited(chiefRoles, herbRoles, ref hasMoreChiefRoles);
            }
            
            else if (amount >= 3 && amount <= 5)
            {
                AddRolesLimited(assistantRoles, herbRoles, ref hasMoreAssistantRoles);
            }
            
            else if (amount >= 1 && amount <= 2)
            {
                AddRolesLimited(harmonyRoles, herbRoles, ref hasMoreHarmonyRoles);
            }
        }

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