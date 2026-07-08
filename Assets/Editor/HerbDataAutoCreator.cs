using System.IO;
using UnityEditor;
using UnityEngine;

public class HerbDataAutoCreator : EditorWindow
{
    private string iconFolderPath = "Assets/Icon_Thuoc";
    private string outputFolderPath = "Assets/Data/Medical/Herbs";

    [MenuItem("Tools/DongY/Create Herb Data From Icons")]
    public static void ShowWindow()
    {
        GetWindow<HerbDataAutoCreator>("Herb Data Creator");
    }

    private void OnGUI()
    {
        GUILayout.Label("Tạo HerbData tự động từ icon thuốc", EditorStyles.boldLabel);

        iconFolderPath = EditorGUILayout.TextField("Icon Folder", iconFolderPath);
        outputFolderPath = EditorGUILayout.TextField("Output Folder", outputFolderPath);

        if (GUILayout.Button("Create HerbData"))
        {
            CreateHerbDataAssets();
        }
    }

    private void CreateHerbDataAssets()
    {
        if (!Directory.Exists(iconFolderPath))
        {
            Debug.LogError("Không tìm thấy folder icon: " + iconFolderPath);
            return;
        }

        if (!Directory.Exists(outputFolderPath))
        {
            Directory.CreateDirectory(outputFolderPath);
        }

        string[] pngFiles = Directory.GetFiles(iconFolderPath, "*.png", SearchOption.AllDirectories);

        int createdCount = 0;
        int skippedCount = 0;
        int failedCount = 0;

        foreach (string pngPath in pngFiles)
        {
            string fixedPath = pngPath.Replace("\\", "/");

            Sprite iconSprite = AssetDatabase.LoadAssetAtPath<Sprite>(fixedPath);

            if (iconSprite == null)
            {
                Debug.LogWarning("Không load được sprite: " + fixedPath);
                failedCount++;
                continue;
            }

            string fileName = Path.GetFileNameWithoutExtension(fixedPath);
            string assetPath = outputFolderPath + "/" + fileName + ".asset";

            HerbData existingHerb = AssetDatabase.LoadAssetAtPath<HerbData>(assetPath);

            if (existingHerb != null)
            {
                skippedCount++;
                continue;
            }

            HerbData herbData = ScriptableObject.CreateInstance<HerbData>();

            herbData.herbName = fileName;
            herbData.description = "";
            herbData.category = HerbCategory.Khac;
            herbData.rarity = HerbRarity.Common;

            // Đây là cấp mở khóa của dược liệu, KHÔNG phải cấp hiện tại của người chơi.
            herbData.unlockClinicLevel = 1;

            herbData.icon = iconSprite;

            herbData.autoCalculateBalance = true;
            herbData.AutoCalculateBalance();

            AssetDatabase.CreateAsset(herbData, assetPath);
            EditorUtility.SetDirty(herbData);

            createdCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "Tạo HerbData xong. Created: "
            + createdCount
            + " | Skipped: "
            + skippedCount
            + " | Failed: "
            + failedCount
        );
    }
}