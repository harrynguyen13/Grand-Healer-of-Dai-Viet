using System.Collections.Generic;
using System.Linq;
using UnityEditor;
using UnityEngine;

public class MedicalDatabaseAutoFiller : EditorWindow
{
    private MedicalDatabase medicalDatabase;

    private string databasePath = "Assets/Data/Medical/Databases/MedicalDatabase.asset";
    private string diseaseFolderPath = "Assets/Data/Medical/Diseases";
    private string herbFolderPath = "Assets/Data/Medical/Herbs";

    [MenuItem("Tools/DongY/Fill Medical Database")]
    public static void ShowWindow()
    {
        GetWindow<MedicalDatabaseAutoFiller>("Fill Medical Database");
    }

    private void OnGUI()
    {
        GUILayout.Label("Tự động cập nhật MedicalDatabase", EditorStyles.boldLabel);

        databasePath = EditorGUILayout.TextField("Database Path", databasePath);
        diseaseFolderPath = EditorGUILayout.TextField("Disease Folder", diseaseFolderPath);
        herbFolderPath = EditorGUILayout.TextField("Herb Folder", herbFolderPath);

        medicalDatabase = (MedicalDatabase)EditorGUILayout.ObjectField(
            "Medical Database",
            medicalDatabase,
            typeof(MedicalDatabase),
            false
        );

        if (GUILayout.Button("Auto Find MedicalDatabase"))
        {
            medicalDatabase = AssetDatabase.LoadAssetAtPath<MedicalDatabase>(databasePath);

            if (medicalDatabase == null)
            {
                Debug.LogError("Không tìm thấy MedicalDatabase tại: " + databasePath);
            }
            else
            {
                Debug.Log("Đã tìm thấy MedicalDatabase.");
            }
        }

        if (GUILayout.Button("Fill Database"))
        {
            FillDatabase();
        }
    }

    private void FillDatabase()
    {
        if (medicalDatabase == null)
        {
            medicalDatabase = AssetDatabase.LoadAssetAtPath<MedicalDatabase>(databasePath);
        }

        if (medicalDatabase == null)
        {
            Debug.LogError("Chưa có MedicalDatabase. Hãy kéo MedicalDatabase vào ô hoặc bấm Auto Find.");
            return;
        }

        DiseaseData[] diseases = LoadAssetsFromFolder<DiseaseData>(diseaseFolderPath);
        HerbData[] herbs = LoadAssetsFromFolder<HerbData>(herbFolderPath);

        medicalDatabase.diseases = diseases
            .Where(disease => disease != null)
            .OrderBy(disease => disease.name)
            .ToList();

        medicalDatabase.herbs = herbs
            .Where(herb => herb != null)
            .OrderBy(herb => herb.name)
            .ToList();

        EditorUtility.SetDirty(medicalDatabase);
        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Đã cập nhật MedicalDatabase.");
        Debug.Log("Tổng số bệnh: " + medicalDatabase.diseases.Count);
        Debug.Log("Tổng số dược liệu: " + medicalDatabase.herbs.Count);
    }

    private T[] LoadAssetsFromFolder<T>(string folderPath) where T : Object
    {
        string[] guids = AssetDatabase.FindAssets("t:" + typeof(T).Name, new[] { folderPath });
        List<T> assets = new List<T>();

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            T asset = AssetDatabase.LoadAssetAtPath<T>(path);

            if (asset != null)
            {
                assets.Add(asset);
            }
        }

        return assets.ToArray();
    }
}