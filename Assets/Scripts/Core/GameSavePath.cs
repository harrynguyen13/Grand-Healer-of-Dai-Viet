using System;
using System.IO;
using UnityEngine;

public static class GameSavePath
{
    private const string FallbackFolderName = "GrandHealerOfDaiViet";

    public static string GetSaveFolderPath()
    {
        string folderPath = Application.persistentDataPath;

        if (string.IsNullOrWhiteSpace(folderPath))
        {
            string localAppDataPath =
                Environment.GetFolderPath(Environment.SpecialFolder.LocalApplicationData);

            if (string.IsNullOrWhiteSpace(localAppDataPath))
                localAppDataPath = Directory.GetCurrentDirectory();

            folderPath = Path.Combine(localAppDataPath, FallbackFolderName);
        }

        if (!Directory.Exists(folderPath))
            Directory.CreateDirectory(folderPath);

        return folderPath;
    }

    public static string GetSavePath(string fileName)
    {
        return Path.Combine(GetSaveFolderPath(), fileName);
    }

    public static string GetLegacyRootSavePath(string fileName)
    {
        return Path.Combine(Directory.GetCurrentDirectory(), fileName);
    }

    public static void MigrateLegacyRootSave(string fileName)
    {
        string newPath = GetSavePath(fileName);
        string legacyPath = GetLegacyRootSavePath(fileName);

        if (newPath == legacyPath)
            return;

        if (File.Exists(newPath))
            return;

        if (!File.Exists(legacyPath))
            return;

        File.Copy(legacyPath, newPath, true);

        Debug.Log("Đã chuyển save cũ: " + legacyPath + " -> " + newPath);
    }

    public static void DeleteSaveAndLegacy(string fileName)
    {
        string newPath = GetSavePath(fileName);
        string legacyPath = GetLegacyRootSavePath(fileName);

        if (File.Exists(newPath))
        {
            File.Delete(newPath);
            Debug.Log("Đã xóa save: " + newPath);
        }

        if (legacyPath != newPath && File.Exists(legacyPath))
        {
            File.Delete(legacyPath);
            Debug.Log("Đã xóa save cũ ở root project: " + legacyPath);
        }
    }
}