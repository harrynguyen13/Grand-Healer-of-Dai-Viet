using System.Collections.Generic;
using UnityEngine;

public static partial class YThuBookTextFormatter
{
    public static string BuildPrescriptionText(DiseaseData disease)
    {
        List<string> pages = BuildPrescriptionPages(disease, true);

        if (pages == null || pages.Count == 0)
            return "";

        return pages[0];
    }

    public static List<string> BuildPrescriptionPages(DiseaseData disease)
    {
        return BuildPrescriptionPages(disease, true);
    }

    public static List<string> BuildPrescriptionPages(
        DiseaseData disease,
        bool showPrescription
    )
    {
        List<string> pages = new List<string>();

        if (disease == null)
            return pages;

        List<string> currentLines = new List<string>();
        int currentLineCount = 0;

        if (showPrescription)
        {
            AddPageLine(
                pages,
                currentLines,
                ref currentLineCount,
                "<align=\"center\"><b>Phương thuốc</b></align>"
            );

            AddPageLine(
                pages,
                currentLines,
                ref currentLineCount,
                ""
            );

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

                    string herbLine =
                        "<align=\"left\">- "
                        + required.herb.herbName
                        + " x"
                        + Mathf.Max(1, required.amount)
                        + "</align>";

                    AddPageLine(
                        pages,
                        currentLines,
                        ref currentLineCount,
                        herbLine
                    );
                }
            }
        }

        AddTreatmentRoleSummaryPageLines(
            disease,
            pages,
            currentLines,
            ref currentLineCount,
            showPrescription
        );

        FlushPrescriptionPage(
            pages,
            currentLines
        );

        return pages;
    }

    private static void AddTreatmentRoleSummaryPageLines(
        DiseaseData disease,
        List<string> pages,
        List<string> currentLines,
        ref int currentLineCount,
        bool hasContentBefore
    )
    {
        string roleSummary = BuildTreatmentRoleSummary(disease);

        if (!string.IsNullOrWhiteSpace(roleSummary) || !hasContentBefore)
        {
            if (currentLines.Count > 0)
            {
                AddPageLine(
                    pages,
                    currentLines,
                    ref currentLineCount,
                    ""
                );
            }

            AddPageLine(
                pages,
                currentLines,
                ref currentLineCount,
                "<align=\"center\"><b>Dược tính cần có</b></align>"
            );

            AddPageLine(
                pages,
                currentLines,
                ref currentLineCount,
                ""
            );

            if (string.IsNullOrWhiteSpace(roleSummary))
            {
                AddPageLine(
                    pages,
                    currentLines,
                    ref currentLineCount,
                    "<align=\"left\">- Chưa có dữ liệu dược tính.</align>"
                );
            }
            else
            {
                string[] roleLines = roleSummary.Split('\n');

                for (int i = 0; i < roleLines.Length; i++)
                {
                    string line = roleLines[i];

                    if (string.IsNullOrWhiteSpace(line))
                    {
                        AddPageLine(
                            pages,
                            currentLines,
                            ref currentLineCount,
                            ""
                        );

                        continue;
                    }

                    string roleLine =
                        "<align=\"left\">"
                        + line.Trim()
                        + "</align>";

                    AddPageLine(
                        pages,
                        currentLines,
                        ref currentLineCount,
                        roleLine
                    );
                }
            }
        }
    }
}