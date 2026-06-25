using System.Collections.Generic;
using UnityEditor;
using UnityEngine;

public class HerbBalanceAutoSetter : EditorWindow
{
    private string herbFolderPath = "Assets/Data/Medical/Herbs";

    [MenuItem("Tools/DongY/Auto Set Herb Balance")]
    public static void ShowWindow()
    {
        GetWindow<HerbBalanceAutoSetter>("Herb Balance Setter");
    }

    private void OnGUI()
    {
        GUILayout.Label("Tự động set Level / Rarity / Price cho HerbData", EditorStyles.boldLabel);

        herbFolderPath = EditorGUILayout.TextField("Herb Folder", herbFolderPath);

        if (GUILayout.Button("Auto Set Herb Balance"))
        {
            AutoSetBalance();
        }
    }

    private void AutoSetBalance()
    {
        string[] guids = AssetDatabase.FindAssets("t:HerbData", new[] { herbFolderPath });

        int updatedCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            HerbData herb = AssetDatabase.LoadAssetAtPath<HerbData>(path);

            if (herb == null)
                continue;

            string key = herb.name.ToLower();

            ApplyDefault(herb);
            ApplyByName(herb, key);

            EditorUtility.SetDirty(herb);
            updatedCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Đã cập nhật balance cho " + updatedCount + " vị thuốc.");
    }

    private void ApplyDefault(HerbData herb)
    {
        herb.category = HerbCategory.Khac;
        herb.rarity = HerbRarity.Common;
        herb.unlockClinicLevel = 1;
        herb.price = 5;
    }

    private void ApplyByName(HerbData herb, string key)
    {
        // LEVEL 1 - thuốc phổ biến đầu game
        if (IsMatch(key,
            "gung", "tia_to", "cam_thao", "tran_bi", "bac_ha", "kinh_gioi",
            "mach_nha", "son_tra", "than_khuc", "xa_tien_tu", "nhan_tran",
            "kim_tien_thao"))
        {
            herb.rarity = HerbRarity.Common;
            herb.unlockClinicLevel = 1;
            herb.price = 5;
        }

        // LEVEL 2 - thuốc trung bình
        if (IsMatch(key,
            "bach_truat", "dang_quy", "dan_sam", "dang_sam", "bach_thuoc",
            "ban_ha", "phuc_linh", "hoang_ky", "xuyen_khung", "ngu_gia_bi"))
        {
            herb.rarity = HerbRarity.Uncommon;
            herb.unlockClinicLevel = 2;
            herb.price = 30;
        }

        // LEVEL 3 - thuốc quý / bệnh khó
        if (IsMatch(key,
            "thuc_dia", "thien_ma", "nhuc_thung_dung", "ba_kich", "dam_duong_hoac",
            "xuyen_bei_mau", "son_thu_du", "ha_thu_o", "cao_ban_long"))
        {
            herb.rarity = HerbRarity.Rare;
            herb.unlockClinicLevel = 3;
            herb.price = 90;
        }

        // LEVEL 4 - thuốc độc / cực quý / đặc biệt
        if (IsMatch(key,
            "phu_tu", "hung_hoang", "tam_that", "xa_huong", "chu_sa",
            "thach_quyet_minh", "thuyen_thoai", "dan_bi"))
        {
            herb.rarity = HerbRarity.Precious;
            herb.unlockClinicLevel = 4;
            herb.price = 180;
        }

        // Thuốc có độc
        if (IsMatch(key, "phu_tu", "hung_hoang", "chu_sa"))
        {
            herb.rarity = HerbRarity.Toxic;
            herb.unlockClinicLevel = 4;
            herb.price = 220;
        }

        // Gán category theo nhóm công dụng
        if (IsMatch(key, "gung", "tia_to", "bac_ha", "kinh_gioi"))
        {
            herb.category = HerbCategory.GiaiBieu;
        }
        else if (IsMatch(key, "kim_ngan_hoa", "lien_kieu", "nhan_tran", "dan_bi"))
        {
            herb.category = HerbCategory.ThanhNhiet;
        }
        else if (IsMatch(key, "tran_bi", "ban_ha"))
        {
            herb.category = HerbCategory.LyKhi;
        }
        else if (IsMatch(key, "mach_nha", "son_tra", "than_khuc"))
        {
            herb.category = HerbCategory.TieuThuc;
        }
        else if (IsMatch(key, "dan_sam", "xuyen_khung", "tam_that"))
        {
            herb.category = HerbCategory.HoatHuyet;
        }
        else if (IsMatch(key, "xa_tien_tu", "kim_tien_thao"))
        {
            herb.category = HerbCategory.LoiThuy;
        }
        else if (IsMatch(key, "dang_sam", "dang_quy", "bach_truat", "hoang_ky"))
        {
            herb.category = HerbCategory.BoKhiHuyet;
        }
        else if (IsMatch(key, "ba_kich", "dam_duong_hoac", "nhuc_thung_dung", "thuc_dia", "son_thu_du"))
        {
            herb.category = HerbCategory.BoThan;
        }
        else if (IsMatch(key, "phu_tu", "hung_hoang", "chu_sa"))
        {
            herb.category = HerbCategory.DocTinh;
        }
    }

    private bool IsMatch(string key, params string[] names)
    {
        foreach (string name in names)
        {
            if (key.Contains(name))
                return true;
        }

        return false;
    }
}