using System.Collections.Generic;
using UnityEngine;

public static class SpecialYThuPrescriptionRecordService
{
    private const string HasCorrectPrescriptionKey = "SpecialYThu_HasCorrectPrescription";
    private const string CorrectPrescriptionTextKey = "SpecialYThu_CorrectPrescriptionText";

    public static void SaveCorrectPrescription(Dictionary<HerbData, int> prescription)
    {
        if (prescription == null || prescription.Count == 0)
            return;

        string text = "";

        foreach (KeyValuePair<HerbData, int> pair in prescription)
        {
            HerbData herb = pair.Key;
            int amount = pair.Value;

            if (herb == null || amount <= 0)
                continue;

            text += "- " + herb.herbName + " x" + amount + "\n";
        }

        text = text.TrimEnd();

        if (string.IsNullOrWhiteSpace(text))
            return;

        PlayerPrefs.SetInt(HasCorrectPrescriptionKey, 1);
        PlayerPrefs.SetString(CorrectPrescriptionTextKey, text);
        PlayerPrefs.Save();

        Debug.Log("Đã lưu phương thuốc chữa khỏi Quan Huyện vào Y thư:\n" + text);
    }

    public static bool HasCorrectPrescription()
    {
        return PlayerPrefs.GetInt(HasCorrectPrescriptionKey, 0) == 1;
    }

    public static string GetCorrectPrescriptionText()
    {
        return PlayerPrefs.GetString(CorrectPrescriptionTextKey, "");
    }

    public static void Reset()
    {
        PlayerPrefs.DeleteKey(HasCorrectPrescriptionKey);
        PlayerPrefs.DeleteKey(CorrectPrescriptionTextKey);
        PlayerPrefs.Save();

        Debug.Log("Đã reset phương thuốc chữa khỏi Quan Huyện trong Y thư.");
    }
}