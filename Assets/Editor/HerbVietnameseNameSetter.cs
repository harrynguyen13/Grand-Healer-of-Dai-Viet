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
            { "gung", "Gừng" },
            { "tia_to", "Tía tô" },
            { "cam_thao", "Cam thảo" },
            { "tran_bi", "Trần bì" },
            { "bach_truat", "Bạch truật" },
            { "kim_ngan_hoa", "Kim ngân hoa" },
            { "lien_kieu", "Liên kiều" },
            { "bac_ha", "Bạc hà" },
            { "kinh_gioi", "Kinh giới" },
            { "mach_nha", "Mạch nha" },
            { "son_tra", "Sơn tra" },
            { "than_khuc", "Thần khúc" },
            { "xa_tien_tu", "Xa tiền tử" },
            { "kim_tien_thao", "Kim tiền thảo" },
            { "nhan_tran", "Nhân trần" },

            { "ba_kich", "Ba kích" },
            { "dam_duong_hoac", "Dâm dương hoắc" },
            { "nhuc_thung_dung", "Nhục thung dung" },
            { "son_thu_du", "Sơn thù du" },
            { "thuc_dia", "Thục địa" },
            { "ha_thu_o", "Hà thủ ô" },
            { "thien_ma", "Thiên ma" },

            { "dang_quy", "Đương quy" },
            { "dang_sam", "Đẳng sâm" },
            { "dan_sam", "Đan sâm" },
            { "hoang_ky", "Hoàng kỳ" },
            { "xuyen_khung", "Xuyên khung" },
            { "bach_thuoc", "Bạch thược" },
            { "phuc_linh", "Phục linh" },
            { "ban_ha", "Bán hạ" },

            { "phu_tu", "Phụ tử" },
            { "chu_sa", "Chu sa" },
            { "tam_that", "Tam thất" },
            { "thach_quyet_minh", "Thạch quyết minh" },
            { "thuyen_thoai", "Thuyền thoái" },
            { "dan_bi", "Đan bì" },

            { "binh_lang", "Binh lang" },
            { "hai_kim_sa", "Hải kim sa" },
            { "bo_cong_anh_kho", "Bồ công anh khô" },
            { "dai_hoang", "Đại hoàng" },
            { "can_khuong", "Can khương" },
            { "cat_canh", "Cát cánh" },
            { "cau_dang", "Câu đằng" },
            { "cau_tich", "Cẩu tích" },
            { "chi_tu", "Chi tử" },
            { "chi_thuc", "Chỉ thực" },
            { "do_trong", "Đỗ trọng" },
            { "duong_quy", "Đương quy" },
            { "hoai_son", "Hoài sơn" },
            { "hong_hoa", "Hồng hoa" },
            { "hung_hoang", "Hùng hoàng" }
        };

        string[] guids = AssetDatabase.FindAssets("t:HerbData", new[] { herbFolderPath });

        int updatedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            HerbData herb = AssetDatabase.LoadAssetAtPath<HerbData>(path);

            if (herb == null)
                continue;

            string key = herb.name.ToLower();

            if (nameMap.ContainsKey(key))
            {
                herb.herbName = nameMap[key];
            }
            else
            {
                herb.herbName = ConvertFileNameToDisplayName(key);
            }

            EditorUtility.SetDirty(herb);
            updatedCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Đã đổi tên tiếng Việt cho " + updatedCount + " vị thuốc.");
    }

    private string ConvertFileNameToDisplayName(string fileName)
    {
        string text = fileName.Replace("_", " ");
        TextInfo textInfo = CultureInfo.CurrentCulture.TextInfo;
        return textInfo.ToTitleCase(text);
    }
}