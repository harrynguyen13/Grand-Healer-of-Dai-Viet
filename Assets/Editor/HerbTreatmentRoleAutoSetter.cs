using System.Collections.Generic;
using System.Globalization;
using System.Text;
using UnityEditor;
using UnityEngine;

public class HerbTreatmentRoleAutoSetter : EditorWindow
{
    private string herbFolderPath = "Assets/Data/Medical/Herbs";
    private bool overwriteExisting = false;

    private Dictionary<string, string> roleByHerbName = new Dictionary<string, string>();

    [MenuItem("Tools/DongY/Auto Set Herb Treatment Roles")]
    public static void ShowWindow()
    {
        GetWindow<HerbTreatmentRoleAutoSetter>("Herb Role Setter");
    }

    private void OnEnable()
    {
        BuildRoleDictionary();
    }

    private void OnGUI()
    {
        GUILayout.Label("Tự động điền vai trò điều trị dược liệu", EditorStyles.boldLabel);

        herbFolderPath = EditorGUILayout.TextField("Herb Folder", herbFolderPath);
        overwriteExisting = EditorGUILayout.Toggle("Ghi đè dữ liệu cũ", overwriteExisting);

        GUILayout.Space(10);

        if (GUILayout.Button("Auto Set Treatment Roles"))
        {
            AutoSetTreatmentRoles();
        }
    }

    private void AutoSetTreatmentRoles()
    {
        string[] guids = AssetDatabase.FindAssets("t:HerbData", new[] { herbFolderPath });

        int changedCount = 0;
        int skippedCount = 0;
        int fallbackCount = 0;

        foreach (string guid in guids)
        {
            string path = AssetDatabase.GUIDToAssetPath(guid);
            HerbData herb = AssetDatabase.LoadAssetAtPath<HerbData>(path);

            if (herb == null)
                continue;

            if (!overwriteExisting && !string.IsNullOrWhiteSpace(herb.treatmentRoleText))
            {
                skippedCount++;
                continue;
            }

            string key = NormalizeName(herb.herbName);

            string roleText;

            if (!roleByHerbName.TryGetValue(key, out roleText))
            {
                roleText = GetFallbackRoleByCategory(herb.category);
                fallbackCount++;
            }

            herb.treatmentRoleText = roleText;

            EditorUtility.SetDirty(herb);
            changedCount++;
        }

        AssetDatabase.SaveAssets();
        AssetDatabase.Refresh();

        Debug.Log(
            "Auto Set Herb Treatment Roles xong. Changed: "
            + changedCount
            + " | Skipped: "
            + skippedCount
            + " | Fallback: "
            + fallbackCount
        );
    }

    private void BuildRoleDictionary()
    {
        roleByHerbName.Clear();

        Add("Sinh khương", "Ôn trung , tán hàn , hỗ trợ tiêu hóa");
        Add("Tía tô", "Giải biểu , tán phong hàn , hành khí");
        Add("Cam thảo", "Điều hòa , giải độc nhẹ , làm dịu họng");
        Add("Bạc hà", "Sơ phong , thanh nhiệt , thông mũi");
        Add("Kinh giới", "Giải biểu , khu phong , giảm ngứa");
        Add("Phòng phong", "Khu phong , tán hàn , giảm đau");
        Add("Sơn tra", "Tiêu thực , hóa tích , hỗ trợ tiêu hóa");
        Add("Thần khúc", "Tiêu thực , hòa vị , giảm đầy bụng");
        Add("Mạch nha", "Tiêu thực , kiện vị , giảm chán ăn");

        Add("Kim ngân hoa", "Thanh nhiệt , giải độc , tiêu viêm");
        Add("Liên kiều", "Thanh nhiệt , giải độc , tán kết");
        Add("Hoàng liên", "Thanh nhiệt , giải độc , tả hỏa");
        Add("Chi tử", "Thanh nhiệt , tả hỏa , lợi thấp");
        Add("Liên tâm", "Thanh tâm hỏa , an thần , trừ phiền");
        Add("Khổ sâm", "Thanh nhiệt , táo thấp , chỉ ngứa");
        Add("Sài đất", "Thanh nhiệt , giải độc , tiêu viêm");
        Add("Bồ công anh", "Thanh nhiệt , giải độc , tiêu ung");
        Add("Ngư tinh thảo", "Thanh phế , giải độc , tiêu mủ");
        Add("Nhân trần", "Thanh thấp nhiệt , lợi mật , trừ hoàng");
        Add("Sinh địa", "Thanh nhiệt , lương huyết , dưỡng âm");
        Add("Hạ khô thảo", "Thanh can, tán kết, sáng mắt");

        Add("Bách bộ", "Nhuận phế , chỉ ho");
        Add("Tang bạch bì", "Thanh phế , chỉ ho , bình suyễn");
        Add("Cát cánh", "Tuyên phế , hóa đờm , lợi họng");
        Add("Mạch môn", "Dưỡng âm , nhuận phế , sinh tân");
        Add("Sa sâm", "Dưỡng âm , thanh phế , sinh tân");
        Add("Bán hạ chế", "Hóa đờm , giáng nghịch , chỉ nôn");
        Add("Xuyên bối mẫu", "Thanh nhiệt hóa đờm , nhuận phế , tán kết");

        Add("Trần bì", "Lý khí , kiện tỳ , hóa đờm");
        Add("Hương phụ", "Sơ can , lý khí , giảm đau");
        Add("Chỉ thực", "Phá khí , tiêu tích , trừ đầy");
        Add("Hậu phác", "Hành khí , táo thấp , tiêu đầy");
        Add("Mộc hương", "Hành khí , chỉ thống , điều trung");
        Add("Uất kim", "Hành khí , hoạt huyết , giải uất");

        Add("Bạch truật", "Kiện tỳ , táo thấp , ích khí");
        Add("Hoài sơn", "Kiện tỳ , bổ phế thận , bồi bổ");
        Add("Đảng sâm", "Bổ khí , kiện tỳ , ích phế");
        Add("Long nhãn", "Bổ tâm tỳ , dưỡng huyết , an thần");
        Add("Phục linh", "Kiện tỳ , lợi thủy , an thần");

        Add("Đương quy", "Bổ huyết , hoạt huyết , giảm đau");
        Add("Bạch thược", "Dưỡng huyết , điều can , giảm đau");
        Add("Thục địa", "Bổ huyết , dưỡng âm , bổ thận");
        Add("Tam thất", "Cầm máu , hoạt huyết , phục hồi");
        Add("Đan sâm", "Hoạt huyết , hóa ứ , thông mạch");
        Add("Hồng hoa", "Hoạt huyết , thông kinh , tán ứ");
        Add("Đào nhân", "Hoạt huyết , phá ứ , nhuận tràng");
        Add("Xuyên khung", "Hoạt huyết , hành khí , giảm đau");

        Add("Toan táo nhân", "An thần , dưỡng tâm , liễm hãn");
        Add("Xương bồ", "Khai khiếu , hóa đờm , tỉnh thần");
        Add("Thiên ma", "Bình can , tức phong , giảm chóng mặt");
        Add("Câu đằng", "Bình can , tức phong , thanh nhiệt");
        Add("Thạch quyết minh", "Bình can , tiềm dương , sáng mắt");

        Add("Đỗ trọng", "Bổ can thận , mạnh gân cốt");
        Add("Tục đoạn", "Bổ can thận , nối gân xương");
        Add("Sơn thù du", "Bổ can thận , cố tinh , liễm hãn");
        Add("Nhục quế", "Ôn dương , tán hàn , thông mạch");
        Add("Phụ tử chế", "Hồi dương , ôn thận , tán hàn");
        Add("Ba kích", "Bổ thận dương , mạnh gân cốt");
        Add("Ngưu tất", "Hoạt huyết , bổ can thận , dẫn huyết xuống");
        Add("Can khương", "Ôn trung , tán hàn , ấm tỳ vị");

        Add("Độc hoạt", "Khu phong thấp , giảm đau");
        Add("Khương hoạt", "Tán hàn , khu phong thấp , giảm đau");
        Add("Tang ký sinh", "Bổ can thận , trừ phong thấp , mạnh gân cốt");

        Add("Xa tiền tử", "Lợi thủy , thông lâm , thanh thấp nhiệt");
        Add("Hải kim sa", "Thông lâm , lợi niệu , bài sỏi");
        Add("Kim tiền thảo", "Lợi thấp , thông lâm , bài thạch");

        Add("Thổ phục linh", "Giải độc , trừ thấp , lợi khớp");
        Add("Bạch tiễn bì", "Thanh nhiệt , táo thấp , chỉ ngứa");
        Add("Thuyền thoái", "Sơ phong , thấu chẩn , chỉ ngứa");

        Add("Đại hoàng", "Tả hạ , thanh nhiệt , phá tích");
        Add("Binh lang", "Hành khí , sát trùng , tiêu tích");
        Add("Sử quân tử", "Sát trùng , tiêu tích , trị giun");
        Add("Hùng hoàng", "Giải độc , sát trùng , trị độc");
    }

    private void Add(string herbName, string roleText)
    {
        string key = NormalizeName(herbName);

        if (string.IsNullOrWhiteSpace(key))
            return;

        roleByHerbName[key] = roleText;
    }

    private string GetFallbackRoleByCategory(HerbCategory category)
    {
        switch (category)
        {
            case HerbCategory.GiaiBieu:
                return "Giải biểu / khu phong";

            case HerbCategory.ThanhNhiet:
                return "Thanh nhiệt / giải độc";

            case HerbCategory.HoaDamChiHo:
                return "Hóa đờm / chỉ ho";

            case HerbCategory.LyKhi:
                return "Lý khí / hành khí";

            case HerbCategory.TieuThuc:
                return "Tiêu thực / hỗ trợ tiêu hóa";

            case HerbCategory.HoatHuyet:
                return "Hoạt huyết / hóa ứ";

            case HerbCategory.LoiThuy:
                return "Lợi thủy / thông tiểu";

            case HerbCategory.BoKhiHuyet:
                return "Bổ khí huyết / phục hồi";

            case HerbCategory.BoThan:
                return "Bổ thận / mạnh gân cốt";

            case HerbCategory.AnThan:
                return "An thần / dưỡng tâm";

            case HerbCategory.DocTinh:
                return "Dược tính mạnh / dùng thận trọng";

            case HerbCategory.Khac:
                return "Hỗ trợ điều trị theo phối ngũ";

            default:
                return "Hỗ trợ điều trị theo phối ngũ";
        }
    }

    private string NormalizeName(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";

        string normalized = text.Trim().ToLowerInvariant().Normalize(NormalizationForm.FormD);

        StringBuilder builder = new StringBuilder();

        foreach (char c in normalized)
        {
            UnicodeCategory category = CharUnicodeInfo.GetUnicodeCategory(c);

            if (category != UnicodeCategory.NonSpacingMark)
            {
                builder.Append(c);
            }
        }

        return builder.ToString().Normalize(NormalizationForm.FormC);
    }
}