using System.Collections.Generic;

public static partial class YThuBookTextFormatter
{
    public static List<string> BuildSpecialPrescriptionPages(DiseaseData disease)
    {
        bool hasCorrectPrescription =
            SpecialYThuPrescriptionRecordService.HasCorrectPrescription();

        string correctPrescriptionText =
            SpecialYThuPrescriptionRecordService.GetCorrectPrescriptionText();

        return BuildSpecialPrescriptionPages(
            disease,
            hasCorrectPrescription,
            correctPrescriptionText
        );
    }

    public static List<string> BuildSpecialPrescriptionPages(
        DiseaseData disease,
        bool hasCorrectPrescription,
        string correctPrescriptionText
    )
    {
        if (!hasCorrectPrescription || string.IsNullOrWhiteSpace(correctPrescriptionText))
        {
            return BuildPrescriptionPages(disease, false);
        }

        List<string> pages = new List<string>();

        if (disease == null)
            return pages;

        List<string> currentLines = new List<string>();
        int currentLineCount = 0;

        AddPageLine(
            pages,
            currentLines,
            ref currentLineCount,
            "<align=\"center\"><b>Phương thuốc đã ghi nhận</b></align>"
        );

        AddPageLine(pages, currentLines, ref currentLineCount, "");

        string[] prescriptionLines = correctPrescriptionText.Split('\n');

        for (int i = 0; i < prescriptionLines.Length; i++)
        {
            string line = prescriptionLines[i];

            if (string.IsNullOrWhiteSpace(line))
                continue;

            AddPageLine(
                pages,
                currentLines,
                ref currentLineCount,
                "<align=\"left\">" + line.Trim() + "</align>"
            );
        }

        AddTreatmentRoleSummaryPageLines(
            disease,
            pages,
            currentLines,
            ref currentLineCount,
            true
        );

        FlushPrescriptionPage(pages, currentLines);

        return pages;
    }
}