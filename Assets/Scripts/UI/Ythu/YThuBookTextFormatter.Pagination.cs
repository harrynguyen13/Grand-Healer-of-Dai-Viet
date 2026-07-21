using System.Collections.Generic;
using UnityEngine;

public static partial class YThuBookTextFormatter
{
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

        string[] manualLines = cleanText.Split('\n');

        int totalLineCount = 0;

        for (int i = 0; i < manualLines.Length; i++)
        {
            string line = manualLines[i];

            if (string.IsNullOrWhiteSpace(line))
            {
                totalLineCount += 1;
                continue;
            }

            int estimatedLineCount =
                Mathf.CeilToInt((float)line.Trim().Length / MaxCharacterPerLine);

            if (estimatedLineCount < 1)
                estimatedLineCount = 1;

            totalLineCount += estimatedLineCount;
        }

        if (totalLineCount < 1)
            totalLineCount = 1;

        return totalLineCount;
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
}