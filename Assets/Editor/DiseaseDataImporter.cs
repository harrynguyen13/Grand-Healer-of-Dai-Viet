using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Text;
using UnityEditor;
using UnityEngine;

public class DiseaseDataImporter : EditorWindow
{
    private string tsvFilePath = "Assets/Data/Medical/disease_import.tsv";
    private string diseaseOutputFolder = "Assets/Data/Medical/Diseases";
    private string herbFolder = "Assets/Data/Medical/Herbs";

    [MenuItem("Tools/DongY/Import Disease Data")]
    public static void ShowWindow()
    {
        GetWindow<DiseaseDataImporter>("Import Disease Data");
    }

    private void OnGUI()
    {
        GUILayout.Label("Import hàng loạt DiseaseData từ file TSV", EditorStyles.boldLabel);

        tsvFilePath = EditorGUILayout.TextField("TSV File Path", tsvFilePath);
        diseaseOutputFolder = EditorGUILayout.TextField("Disease Output Folder", diseaseOutputFolder);
        herbFolder = EditorGUILayout.TextField("Herb Folder", herbFolder);

        if (GUILayout.Button("Import Disease Data"))
        {
            ImportDiseases();
        }
    }

    private void ImportDiseases()
    {
        if (!File.Exists(tsvFilePath))
        {
            Debug.LogError("Không tìm thấy file TSV: " + tsvFilePath);
            return;
        }

        if (!Directory.Exists(diseaseOutputFolder))
        {
            Directory.CreateDirectory(diseaseOutputFolder);
        }

        Dictionary<string, HerbData> herbLookup = BuildHerbLookup();

        string[] lines = File.ReadAllLines(tsvFilePath);

        if (lines.Length <= 1)
        {
            Debug.LogError("File TSV chưa có dữ liệu bệnh.");
            return;
        }

        string[] headers = lines[0].Split('\t');
        Dictionary<string, int> headerIndex = BuildHeaderIndex(headers);

        int createdCount = 0;
        int updatedCount = 0;

        for (int i = 1; i < lines.Length; i++)
        {
            if (string.IsNullOrWhiteSpace(lines[i]))
                continue;

            string[] columns = lines[i].Split('\t');

            string assetName = GetValue(columns, headerIndex, "assetName");
            string diseaseName = GetValue(columns, headerIndex, "diseaseName");

            if (string.IsNullOrWhiteSpace(assetName))
            {
                assetName = CreateSafeAssetName(diseaseName);
            }

            if (string.IsNullOrWhiteSpace(assetName) || string.IsNullOrWhiteSpace(diseaseName))
            {
                Debug.LogWarning("Bỏ qua dòng " + (i + 1) + " vì thiếu assetName hoặc diseaseName.");
                continue;
            }

            string assetPath = diseaseOutputFolder + "/" + assetName + ".asset";

            DiseaseData disease = AssetDatabase.LoadAssetAtPath<DiseaseData>(assetPath);

            bool isNew = false;

            if (disease == null)
            {
                disease = ScriptableObject.CreateInstance<DiseaseData>();
                AssetDatabase.CreateAsset(disease, assetPath);
                isNew = true;
            }

            disease.diseaseName = diseaseName;
            disease.diseaseLevel = ParseDiseaseLevel(GetValue(columns, headerIndex, "level"));
            disease.diseaseGroup = ParseDiseaseGroup(GetValue(columns, headerIndex, "group"));
            disease.description = GetValue(columns, headerIndex, "description");
            disease.patientDialogue = GetValue(columns, headerIndex, "patientDialogue");

            disease.symptoms.Clear();

            AddSymptoms(disease, GetValue(columns, headerIndex, "askSymptoms"), ExaminationStep.Ask);
            AddSymptoms(disease, GetValue(columns, headerIndex, "pulseSymptoms"), ExaminationStep.PulseCheck);

            disease.correctHerbs.Clear();

            string correctHerbsText = GetValue(columns, headerIndex, "correctHerbs");
            string[] herbNames = correctHerbsText.Split('|');

            foreach (string herbName in herbNames)
            {
                string key = NormalizeKey(herbName);

                if (string.IsNullOrWhiteSpace(key))
                    continue;

                if (herbLookup.TryGetValue(key, out HerbData herb))
                {
                    disease.correctHerbs.Add(herb);
                }
                else
                {
                    Debug.LogWarning("Không tìm thấy HerbData cho thuốc: " + herbName + " | Bệnh: " + diseaseName);
                }
            }

            EditorUtility.SetDirty(disease);

            if (isNew)
                createdCount++;
            else
                updatedCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Import bệnh xong. Tạo mới: " + createdCount + " | Cập nhật: " + updatedCount);
    }

    private Dictionary<string, int> BuildHeaderIndex(string[] headers)
    {
        Dictionary<string, int> result = new Dictionary<string, int>();

        for (int i = 0; i < headers.Length; i++)
        {
            string key = headers[i].Trim();

            if (!result.ContainsKey(key))
            {
                result.Add(key, i);
            }
        }

        return result;
    }

    private string GetValue(string[] columns, Dictionary<string, int> headerIndex, string columnName)
    {
        if (!headerIndex.ContainsKey(columnName))
            return "";

        int index = headerIndex[columnName];

        if (index < 0 || index >= columns.Length)
            return "";

        return columns[index].Trim();
    }

    private void AddSymptoms(DiseaseData disease, string rawText, ExaminationStep step)
    {
        if (string.IsNullOrWhiteSpace(rawText))
            return;

        string[] symptoms = rawText.Split('|');

        foreach (string symptom in symptoms)
        {
            if (string.IsNullOrWhiteSpace(symptom))
                continue;

            SymptomData symptomData = new SymptomData
            {
                symptomText = symptom.Trim(),
                showAtStep = step
            };

            disease.symptoms.Add(symptomData);
        }
    }

    private Dictionary<string, HerbData> BuildHerbLookup()
    {
        Dictionary<string, HerbData> lookup = new Dictionary<string, HerbData>();

        string[] guids = AssetDatabase.FindAssets("t:HerbData", new[] { herbFolder });

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            HerbData herb = AssetDatabase.LoadAssetAtPath<HerbData>(path);

            if (herb == null)
                continue;

            AddHerbKey(lookup, herb.name, herb);
            AddHerbKey(lookup, herb.herbName, herb);
        }

        return lookup;
    }

    private void AddHerbKey(Dictionary<string, HerbData> lookup, string rawKey, HerbData herb)
    {
        string key = NormalizeKey(rawKey);

        if (string.IsNullOrWhiteSpace(key))
            return;

        if (!lookup.ContainsKey(key))
        {
            lookup.Add(key, herb);
        }
    }

    private DiseaseLevel ParseDiseaseLevel(string text)
    {
        text = text.Trim().ToLower();

        if (text.Contains("4"))
            return DiseaseLevel.Level4;

        if (text.Contains("3"))
            return DiseaseLevel.Level3;

        if (text.Contains("2"))
            return DiseaseLevel.Level2;

        return DiseaseLevel.Level1;
    }

    private DiseaseGroup ParseDiseaseGroup(string text)
    {
        string key = NormalizeKey(text);

        switch (key)
        {
            case "hohap":
                return DiseaseGroup.HoHap;

            case "tieuhoa":
                return DiseaseGroup.TieuHoa;

            case "thankinh":
                return DiseaseGroup.ThanKinh;

            case "timmach":
                return DiseaseGroup.TimMach;

            case "coxuongkhop":
                return DiseaseGroup.CoXuongKhop;

            case "tietnieu":
                return DiseaseGroup.TietNieu;

            case "dalieu":
                return DiseaseGroup.DaLieu;

            case "ngoaikhoa":
                return DiseaseGroup.NgoaiKhoa;

            case "docto":
                return DiseaseGroup.DocTo;

            default:
                return DiseaseGroup.Khac;
        }
    }

    private string CreateSafeAssetName(string text)
    {
        string noAccent = RemoveDiacritics(text);
        noAccent = noAccent.Replace(" ", "_");

        StringBuilder builder = new StringBuilder();

        foreach (char c in noAccent)
        {
            if (char.IsLetterOrDigit(c) || c == '_')
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }

    private string NormalizeKey(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";

        string noAccent = RemoveDiacritics(text).ToLower();

        StringBuilder builder = new StringBuilder();

        foreach (char c in noAccent)
        {
            if (char.IsLetterOrDigit(c))
            {
                builder.Append(c);
            }
        }

        return builder.ToString();
    }

    private string RemoveDiacritics(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";

        string normalized = text.Normalize(NormalizationForm.FormD);
        StringBuilder builder = new StringBuilder();

        foreach (char c in normalized)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);

            if (category != UnicodeCategory.NonSpacingMark)
            {
                if (c == 'đ')
                    builder.Append('d');
                else if (c == 'Đ')
                    builder.Append('D');
                else
                    builder.Append(c);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}