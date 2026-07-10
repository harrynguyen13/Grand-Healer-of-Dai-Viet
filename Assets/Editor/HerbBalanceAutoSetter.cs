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
        GUILayout.Label("Tự động set Level / Rarity / Category / BuyPrice / SellPrice / Stock", EditorStyles.boldLabel);

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

            string key = NormalizeKey(herb.name);

            ApplyDefault(herb);
            ApplyRarityAndLevelByName(herb, key);
            ApplyCategoryByName(herb, key);

            herb.autoCalculateBalance = true;
            herb.AutoCalculateBalance();

            EditorUtility.SetDirty(herb);
            updatedCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log("Đã cập nhật balance cho " + updatedCount + " vị thuốc.");
    }

    private string NormalizeKey(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        string key = value.Trim().ToLowerInvariant();

        key = key.Replace(" ", "_");
        key = key.Replace("-", "_");

        while (key.Contains("__"))
        {
            key = key.Replace("__", "_");
        }

        return key;
    }

    private void ApplyDefault(HerbData herb)
    {
        herb.category = HerbCategory.Khac;
        herb.rarity = HerbRarity.Common;
        herb.unlockClinicLevel = 1;
        herb.autoCalculateBalance = true;
    }

    private void ApplyRarityAndLevelByName(HerbData herb, string key)
    {
        // LEVEL 4 - thuốc độc / đặc biệt nguy hiểm
        if (IsMatch(key,
            "phu_tu",
            "phu_tu_che",
            "hung_hoang",
            "chu_sa"))
        {
            SetBalance(herb, HerbRarity.Toxic, 4);
        }

        // LEVEL 4 - cực quý / bệnh khó
        else if (IsMatch(key,
            "tam_that",
            "thach_quyet_minh",
            "thuyen_thoai",
            "hai_kim_sa"))
        {
            SetBalance(herb, HerbRarity.Precious, 4);
        }

        // LEVEL 3 - thuốc quý
        else if (IsMatch(key,
            "ba_kich",
            "dam_duong_hoac",
            "thuc_dia",
            "sinh_dia",
            "thien_ma",
            "nhuc_thung_dung",
            "son_thu_du",
            "do_trong",
            "tuc_doan",
            "cau_tich",
            "xuyen_boi_mau",
            "kho_sam",
            "bach_tien_bi",
            "dai_hoang",
            "nguu_tat",
            "doc_hoat",
            "khuong_hoat",
            "dan_bi",
            "ha_thu_o",
            "hoang_lien",
            "nhuc_que"))
        {
            SetBalance(herb, HerbRarity.Rare, 3);
        }

        // LEVEL 2 - thuốc trung bình
        else if (IsMatch(key,
            "bach_thuoc",
            "ban_ha",
            "phuc_linh",
            "dan_sam",
            "dang_quy",
            "duong_quy",
            "xuyen_khung",
            "dang_sam",
            "cat_canh",
            "sa_sam",
            "mach_mon",
            "moc_huong",
            "hau_phac",
            "tang_bach_bi",
            "tho_phuc_linh",
            "thang_ma",
            "uat_kim",
            "xuong_bo",
            "chi_tu",
            "lien_tam",
            "toan_tao_nhan",
            "long_nhan",
            "dao_nhan",
            "binh_lang",
            "su_quan_tu",
            "cau_dang",
            "chi_thuc"))
        {
            SetBalance(herb, HerbRarity.Uncommon, 2);
        }

        // LEVEL 1 - thuốc phổ biến đầu game
        else if (IsMatch(key,
            "sinh_khuong",
            "can_khuong",
            "tran_bi",
            "cam_thao",
            "tia_to",
            "kinh_gioi",
            "bac_ha",
            "ngu_tinh_thao",
            "bo_cong_anh",
            "sai_dat",
            "phong_phong",
            "tang_ky_sinh",
            "son_tra",
            "mach_nha",
            "than_khuc",
            "xa_tien_tu",
            "nhan_tran",
            "kim_tien_thao",
            "hong_hoa",
            "kim_ngan_hoa",
            "lien_kieu",
            "bach_bo",
            "ban_ha_che",
            "huong_phu",
            "bach_truat",
            "hoai_son"))
        {
            SetBalance(herb, HerbRarity.Common, 1);
        }
    }

    private void SetBalance(HerbData herb, HerbRarity rarity, int level)
    {
        herb.rarity = rarity;
        herb.unlockClinicLevel = level;
    }

    private void ApplyCategoryByName(HerbData herb, string key)
    {
        // Giải biểu
        if (IsMatch(key,
            "gung",
            "sinh_khuong",
            "can_khuong",
            "tia_to",
            "kinh_gioi",
            "bac_ha",
            "phong_phong",
            "doc_hoat",
            "khuong_hoat"))
        {
            herb.category = HerbCategory.GiaiBieu;
        }

        // Thanh nhiệt
        else if (IsMatch(key,
            "kim_ngan_hoa",
            "lien_kieu",
            "bo_cong_anh",
            "ngu_tinh_thao",
            "sai_dat",
            "nhan_tran",
            "chi_tu",
            "dan_bi",
            "kho_sam",
            "bach_tien_bi",
            "dai_hoang",
            "thang_ma",
            "xuyen_boi_mau",
            "sinh_dia",
            "hoang_lien"))
        {
            herb.category = HerbCategory.ThanhNhiet;
        }

        // Hóa đờm trị ho
        else if (IsMatch(key,
            "bach_bo",
            "cat_canh",
            "ban_ha",
            "ban_ha_che",
            "xuyen_boi_mau",
            "tang_bach_bi"))
        {
            herb.category = HerbCategory.HoaDamChiHo;
        }

        // Lý khí
        else if (IsMatch(key,
            "tran_bi",
            "huong_phu",
            "moc_huong",
            "hau_phac",
            "chi_thuc",
            "xuong_bo"))
        {
            herb.category = HerbCategory.LyKhi;
        }

        // Tiêu thực
        else if (IsMatch(key,
            "son_tra",
            "than_khuc",
            "mach_nha",
            "binh_lang",
            "su_quan_tu"))
        {
            herb.category = HerbCategory.TieuThuc;
        }

        // Hoạt huyết
        else if (IsMatch(key,
            "dan_sam",
            "dang_quy",
            "duong_quy",
            "xuyen_khung",
            "tam_that",
            "hong_hoa",
            "uat_kim",
            "nguu_tat",
            "dao_nhan"))
        {
            herb.category = HerbCategory.HoatHuyet;
        }

        // Lợi thủy
        else if (IsMatch(key,
            "xa_tien_tu",
            "kim_tien_thao",
            "hai_kim_sa",
            "tho_phuc_linh",
            "phuc_linh"))
        {
            herb.category = HerbCategory.LoiThuy;
        }

        // Bổ khí huyết
        else if (IsMatch(key,
            "cam_thao",
            "bach_truat",
            "dang_sam",
            "bach_thuoc",
            "mach_mon",
            "long_nhan",
            "hoai_son",
            "sa_sam",
            "bach_bo"))
        {
            herb.category = HerbCategory.BoKhiHuyet;
        }

        // Bổ thận
        else if (IsMatch(key,
            "ba_kich",
            "dam_duong_hoac",
            "nhuc_thung_dung",
            "thuc_dia",
            "son_thu_du",
            "do_trong",
            "tuc_doan",
            "cau_tich",
            "tang_ky_sinh",
            "thien_ma",
            "ha_thu_o",
            "nhuc_que"))
        {
            herb.category = HerbCategory.BoThan;
        }

        // An thần
        else if (IsMatch(key,
            "toan_tao_nhan",
            "lien_tam",
            "long_nhan",
            "thien_ma"))
        {
            herb.category = HerbCategory.AnThan;
        }

        // Độc tính / đặc biệt
        else if (IsMatch(key,
            "phu_tu",
            "phu_tu_che",
            "hung_hoang",
            "chu_sa",
            "thach_quyet_minh",
            "thuyen_thoai"))
        {
            herb.category = HerbCategory.DocTinh;
        }

        // Còn lại giữ Khac
    }

    private bool IsMatch(string key, params string[] names)
    {
        foreach (string name in names)
        {
            if (key == name)
                return true;
        }

        return false;
    }
}