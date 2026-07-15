using System.Collections.Generic;
using UnityEngine;

public static partial class YThuBookTextFormatter
{
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

    private static void AddRolesLimited(
        List<string> targetRoles,
        List<string> sourceRoles,
        ref bool hasMoreRoles
    )
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
}