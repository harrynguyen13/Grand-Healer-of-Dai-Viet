using System.Collections.Generic;
using System.Globalization;
using UnityEditor;
using UnityEngine;

public class HerbVietnameseNameSetter : EditorWindow
{
    private string herbFolderPath = "Assets/Data/Medical/Herbs";

    [MenuItem("Tools/DongY/Set Vietnamese Herb Names")]
    public static void ShowWindow()
    {
        GetWindow<HerbVietnameseNameSetter>("Vietnamese Herb Names");
    }

    private void OnGUI()
    {
        GUILayout.Label("Tự động đổi Herb Name sang tiếng Việt", EditorStyles.boldLabel);

        herbFolderPath = EditorGUILayout.TextField("Herb Folder", herbFolderPath);

        if (GUILayout.Button("Set Vietnamese Names"))
        {
            SetVietnameseNames();
        }
    }

    private void SetVietnameseNames()
    {
        Dictionary<string, string> nameMap = new Dictionary<string, string>
        {
            // =========================
            // THÂN GỖ / CỦ / RỄ CỨNG
            // =========================
            { "bach_bo", "Bạch bộ" },
            { "bach_thuoc", "Bạch thược" },
            { "bach_tien_bi", "Bạch tiền bì" },
            { "bach_truat", "Bạch truật" },
            { "ban_ha", "Bán hạ" },
            { "ban_ha_che", "Bán hạ chế" },
            { "cam_thao", "Cam thảo" },
            { "can_khuong", "Can khương" },
            { "cat_canh", "Cát cánh" },
            { "dai_hoang", "Đại hoàng" },
            { "dan_bi", "Đan bì" },
            { "dan_sam", "Đan sâm" },
            { "dang_sam", "Đẳng sâm" },
            { "dang_quy", "Đương quy" },
            { "duong_quy", "Đương quy" },
            { "do_trong", "Đỗ trọng" },
            { "doc_hoat", "Độc hoạt" },
            { "hau_phac", "Hậu phác" },
            { "hoai_son", "Hoài sơn" },
            { "huong_phu", "Hương phụ" },
            { "kho_sam", "Khổ sâm" },
            { "khuong_hoat", "Khương hoạt" },
            { "mach_mon", "Mạch môn" },
            { "moc_huong", "Mộc hương" },
            { "nguu_tat", "Ngưu tất" },
            { "nhuc_que", "Nhục quế" },
            { "phu_tu", "Phụ tử" },
            { "phu_tu_che", "Phụ tử chế" },
            { "sa_sam", "Sa sâm" },
            { "sinh_dia", "Sinh địa" },
            { "sinh_khuong", "Sinh khương" },
            { "tam_that", "Tam thất" },
            { "tang_bach_bi", "Tang bạch bì" },
            { "thang_ma", "Thăng ma" },
            { "thien_ma", "Thiên ma" },
            { "tho_phuc_linh", "Thổ phục linh" },
            { "thuc_dia", "Thục địa" },
            { "tran_bi", "Trần bì" },
            { "tuc_doan", "Tục đoạn" },
            { "uat_kim", "Uất kim" },
            { "xuyen_khung", "Xuyên khung" },
            { "xuong_bo", "Xương bồ" },

            // =========================
            // HẠT / HOA / QUẢ
            // =========================
            { "binh_lang", "Binh lang" },
            { "cau_dang", "Câu đằng" },
            { "chi_thuc", "Chỉ thực" },
            { "chi_tu", "Chi tử" },
            { "dao_nhan", "Đào nhân" },
            { "hong_hoa", "Hồng hoa" },
            { "kim_ngan_hoa", "Kim ngân hoa" },
            { "lien_kieu", "Liên kiều" },
            { "lien_tam", "Liên tâm" },
            { "long_nhan", "Long nhãn" },
            { "mach_nha", "Mạch nha" },
            { "son_thu_du", "Sơn thù du" },
            { "son_tra", "Sơn tra" },
            { "su_quan_tu", "Sử quân tử" },
            { "toan_tao_nhan", "Toan táo nhân" },
            { "xa_tien_tu", "Xa tiền tử" },
            { "xuyen_boi_mau", "Xuyên bối mẫu" },

            // =========================
            // THÂN / CỎ / LÁ
            // =========================
            { "bac_ha", "Bạc hà" },
            { "bo_cong_anh", "Bồ công anh" },
            { "dam_duong_hoac", "Dâm dương hoắc" },
            { "kim_tien_thao", "Kim tiền thảo" },
            { "kinh_gioi", "Kinh giới" },
            { "ngu_tinh_thao", "Ngư tinh thảo" },
            { "nhan_tran", "Nhân trần" },
            { "phong_phong", "Phòng phong" },
            { "sai_dat", "Sài đất" },
            { "tang_ky_sinh", "Tang ký sinh" },
            { "tia_to", "Tía tô" },

            // =========================
            // ĐẶC BIỆT
            // =========================
            { "cau_tich", "Cẩu tích" },
            { "hai_kim_sa", "Hải kim sa" },
            { "hung_hoang", "Hùng hoàng" },
            { "nhuc_thung_dung", "Nhục thung dung" },
            { "phuc_linh", "Phục linh" },
            { "thach_quyet_minh", "Thạch quyết minh" },
            { "than_khuc", "Thần khúc" },
            { "thuyen_thoai", "Thuyền thoái" },

        };

        string[] guids = AssetDatabase.FindAssets("t:HerbData", new[] { herbFolderPath });

        int updatedCount = 0;
        int fallbackCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            HerbData herb = AssetDatabase.LoadAssetAtPath<HerbData>(path);

            if (herb == null)
                continue;

            string key = NormalizeKey(herb.name);

            if (nameMap.ContainsKey(key))
            {
                herb.herbName = nameMap[key];
            }
            else
            {
                herb.herbName = ConvertFileNameToDisplayName(key);
                fallbackCount++;

                Debug.LogWarning("Chưa có tên tiếng Việt trong nameMap: " + key);
            }

            EditorUtility.SetDirty(herb);
            updatedCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Đã đổi tên tiếng Việt cho " + updatedCount + " vị thuốc. Fallback: " + fallbackCount);
    }

    private string NormalizeKey(string rawName)
    {
        if (string.IsNullOrEmpty(rawName))
            return "";

        string key = rawName.Trim().ToLowerInvariant();

        key = key.Replace(" ", "_");
        key = key.Replace("-", "_");

        while (key.Contains("__"))
        {
            key = key.Replace("__", "_");
        }

        return key;
    }

    private string ConvertFileNameToDisplayName(string fileName)
    {
        string text = fileName.Replace("_", " ");

        TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(text);
    }
}