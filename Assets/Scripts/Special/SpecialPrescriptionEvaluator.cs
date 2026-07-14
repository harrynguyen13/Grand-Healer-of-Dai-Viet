using System;
using System.Collections.Generic;
using UnityEngine;

public class SpecialPrescriptionEvaluationResult
{
    public bool isCorrect;
    public string message;

    public SpecialPrescriptionEvaluationResult(bool isCorrect, string message)
    {
        this.isCorrect = isCorrect;
        this.message = message;
    }
}

public static class SpecialPrescriptionEvaluator
{
    private const int MaxRolesPerGroup = 6;
    private const int MaxRolesPerHerb = 2;

    private enum MedicineRoleGroup
    {
        None,
        Chief,
        Assistant,
        Harmony
    }

    private class RoleRequirement
    {
        public int requiredCount;
        public List<string> roles = new List<string>();
        public bool hasMoreRoles;
    }

    public static SpecialPrescriptionEvaluationResult Evaluate(
        DiseaseData disease,
        Dictionary<HerbData, int> selectedPrescription
    )
    {
        if (disease == null)
        {
            return new SpecialPrescriptionEvaluationResult(
                false,
                "Không có dữ liệu bệnh đặc biệt."
            );
        }

        if (selectedPrescription == null || selectedPrescription.Count == 0)
        {
            return new SpecialPrescriptionEvaluationResult(
                false,
                "Chưa chọn vị thuốc nào."
            );
        }

        RoleRequirement chiefRequirement;
        RoleRequirement assistantRequirement;
        RoleRequirement harmonyRequirement;

        BuildRequirementsFromDisease(
            disease,
            out chiefRequirement,
            out assistantRequirement,
            out harmonyRequirement
        );

        int selectedChiefCount = 0;
        int selectedAssistantCount = 0;
        int selectedHarmonyCount = 0;

        foreach (KeyValuePair<HerbData, int> pair in selectedPrescription)
        {
            HerbData herb = pair.Key;
            int amount = pair.Value;

            if (herb == null || amount <= 0)
                continue;

            MedicineRoleGroup selectedGroup = GetRoleGroupByAmount(amount);

            if (selectedGroup == MedicineRoleGroup.None)
            {
                return new SpecialPrescriptionEvaluationResult(
                    false,
                    herb.herbName + " dùng x" + amount + " không thuộc khoảng hợp lệ 1-8."
                );
            }

            if (selectedGroup == MedicineRoleGroup.Chief)
            {
                selectedChiefCount++;

                if (!DoesHerbMatchRequirement(herb, chiefRequirement))
                {
                    return new SpecialPrescriptionEvaluationResult(
                        false,
                        herb.herbName + " chưa phù hợp vai trò Chủ dược."
                    );
                }
            }
            else if (selectedGroup == MedicineRoleGroup.Assistant)
            {
                selectedAssistantCount++;

                if (!DoesHerbMatchRequirement(herb, assistantRequirement))
                {
                    return new SpecialPrescriptionEvaluationResult(
                        false,
                        herb.herbName + " chưa phù hợp vai trò Phụ dược."
                    );
                }
            }
            else if (selectedGroup == MedicineRoleGroup.Harmony)
            {
                selectedHarmonyCount++;

                if (!DoesHerbMatchRequirement(herb, harmonyRequirement))
                {
                    return new SpecialPrescriptionEvaluationResult(
                        false,
                        herb.herbName + " chưa phù hợp vai trò Điều hòa."
                    );
                }
            }
        }

        if (selectedChiefCount != chiefRequirement.requiredCount)
        {
            return new SpecialPrescriptionEvaluationResult(
                false,
                "Số vị Chủ dược chưa đúng. Cần "
                + chiefRequirement.requiredCount
                + " vị, hiện có "
                + selectedChiefCount
                + " vị."
            );
        }

        if (selectedAssistantCount != assistantRequirement.requiredCount)
        {
            return new SpecialPrescriptionEvaluationResult(
                false,
                "Số vị Phụ dược chưa đúng. Cần "
                + assistantRequirement.requiredCount
                + " vị, hiện có "
                + selectedAssistantCount
                + " vị."
            );
        }

        if (selectedHarmonyCount != harmonyRequirement.requiredCount)
        {
            return new SpecialPrescriptionEvaluationResult(
                false,
                "Số vị Điều hòa chưa đúng. Cần "
                + harmonyRequirement.requiredCount
                + " vị, hiện có "
                + selectedHarmonyCount
                + " vị."
            );
        }

        return new SpecialPrescriptionEvaluationResult(
            true,
            "Đơn thuốc phù hợp dược tính cần có."
        );
    }

    private static void BuildRequirementsFromDisease(
        DiseaseData disease,
        out RoleRequirement chiefRequirement,
        out RoleRequirement assistantRequirement,
        out RoleRequirement harmonyRequirement
    )
    {
        chiefRequirement = new RoleRequirement();
        assistantRequirement = new RoleRequirement();
        harmonyRequirement = new RoleRequirement();

        if (disease == null || disease.requiredHerbs == null)
            return;

        for (int i = 0; i < disease.requiredHerbs.Count; i++)
        {
            RequiredHerbAmount required = disease.requiredHerbs[i];

            if (required == null || required.herb == null)
                continue;

            HerbData herb = required.herb;
            int amount = Mathf.Max(1, required.amount);

            List<string> herbRoles = ExtractImportantRolesFromHerb(herb);

            MedicineRoleGroup roleGroup = GetRoleGroupByAmount(amount);

            if (roleGroup == MedicineRoleGroup.Chief)
            {
                chiefRequirement.requiredCount++;
                AddRolesLimited(chiefRequirement, herbRoles);
            }
            else if (roleGroup == MedicineRoleGroup.Assistant)
            {
                assistantRequirement.requiredCount++;
                AddRolesLimited(assistantRequirement, herbRoles);
            }
            else if (roleGroup == MedicineRoleGroup.Harmony)
            {
                harmonyRequirement.requiredCount++;
                AddRolesLimited(harmonyRequirement, herbRoles);
            }
        }
    }

    private static MedicineRoleGroup GetRoleGroupByAmount(int amount)
    {
        if (amount >= 6 && amount <= 8)
            return MedicineRoleGroup.Chief;

        if (amount >= 3 && amount <= 5)
            return MedicineRoleGroup.Assistant;

        if (amount >= 1 && amount <= 2)
            return MedicineRoleGroup.Harmony;

        return MedicineRoleGroup.None;
    }

    private static bool DoesHerbMatchRequirement(
        HerbData selectedHerb,
        RoleRequirement requirement
    )
    {
        if (selectedHerb == null || requirement == null)
            return false;

        if (requirement.requiredCount <= 0)
            return false;

        if (requirement.roles == null || requirement.roles.Count == 0)
            return false;

        List<string> selectedRoles = ExtractImportantRolesFromHerb(selectedHerb);

        if (selectedRoles == null || selectedRoles.Count == 0)
            return false;

        for (int i = 0; i < selectedRoles.Count; i++)
        {
            string selectedRole = selectedRoles[i];

            for (int j = 0; j < requirement.roles.Count; j++)
            {
                string requiredRole = requirement.roles[j];

                if (NormalizeRoleForCompare(selectedRole) == NormalizeRoleForCompare(requiredRole))
                    return true;
            }
        }

        return false;
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
        RoleRequirement requirement,
        List<string> sourceRoles
    )
    {
        if (requirement == null || sourceRoles == null)
            return;

        for (int i = 0; i < sourceRoles.Count; i++)
        {
            string role = sourceRoles[i];

            if (string.IsNullOrWhiteSpace(role))
                continue;

            if (ContainsRole(requirement.roles, role))
                continue;

            if (requirement.roles.Count < MaxRolesPerGroup)
            {
                requirement.roles.Add(role);
            }
            else
            {
                requirement.hasMoreRoles = true;
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
}