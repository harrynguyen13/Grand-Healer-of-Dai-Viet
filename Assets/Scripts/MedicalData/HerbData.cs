using UnityEngine;

public enum HerbRarity
{
    [InspectorName("Phổ biến")]
    Common,

    [InspectorName("Khá hiếm")]
    Uncommon,

    [InspectorName("Quý")]
    Rare,

    [InspectorName("Có độc")]
    Toxic,

    [InspectorName("Cực quý")]
    Precious
}

public enum HerbCategory
{
    [InspectorName("Giải biểu")]
    GiaiBieu,

    [InspectorName("Thanh nhiệt")]
    ThanhNhiet,

    [InspectorName("Hóa đờm chỉ ho")]
    HoaDamChiHo,

    [InspectorName("Lý khí")]
    LyKhi,

    [InspectorName("Tiêu thực")]
    TieuThuc,

    [InspectorName("Hoạt huyết")]
    HoatHuyet,

    [InspectorName("Lợi thủy")]
    LoiThuy,

    [InspectorName("Bổ khí huyết")]
    BoKhiHuyet,

    [InspectorName("Bổ thận")]
    BoThan,

    [InspectorName("An thần")]
    AnThan,

    [InspectorName("Độc tính / đặc biệt")]
    DocTinh,

    [InspectorName("Khác")]
    Khac
}

[CreateAssetMenu(fileName = "NewHerb", menuName = "Đông Y/Dữ liệu dược liệu")]
public class HerbData : ScriptableObject
{
    [Header("Thông tin dược liệu")]
    public string herbName;

    [TextArea(2, 5)]
    public string description;

    [Header("Y thư")]
    [Tooltip("Vai trò điều trị ngắn gọn của vị thuốc. Ví dụ: Thanh nhiệt / giải độc.")]
    [TextArea(1, 2)]
    public string treatmentRoleText;

    public HerbCategory category;
    public HerbRarity rarity;

    [Header("Gameplay")]
    public int unlockClinicLevel = 1;

    [Tooltip("Giữ lại để code cũ không lỗi. Giá này tự bằng sellPrice.")]
    public int price = 3;

    [Header("Giá mua / giá kê thuốc")]
    [Tooltip("Giá người chơi mua dược liệu từ thương nhân.")]
    public int buyPrice = 2;

    [Tooltip("Giá tính cho NPC bệnh nhân khi vị thuốc này nằm trong đơn.")]
    public int sellPrice = 3;

    [Header("Kho thuốc")]
    [Tooltip("Số lượng ban đầu trong kho khi khởi tạo game.")]
    public int startQuantity = 50;

    [Header("Tự cân bằng")]
    [Tooltip("Bật để tự tính giá và số lượng theo độ hiếm, nhóm thuốc, cấp mở khóa.")]
    public bool autoCalculateBalance = true;

    [Header("Icon UI")]
    public Sprite icon;

#if UNITY_EDITOR
    private void OnValidate()
    {
        if (autoCalculateBalance)
        {
            AutoCalculateBalance();
        }
    }
#endif

    public void AutoCalculateBalance()
    {
        if (unlockClinicLevel < 1)
            unlockClinicLevel = 1;

        int baseBuyPrice = GetBaseBuyPriceByRarity();
        int categoryBonus = GetCategoryBonus();
        int levelBonus = GetLevelBonus();

        buyPrice = baseBuyPrice + categoryBonus + levelBonus;

        if (buyPrice < 1)
            buyPrice = 1;

        int profit = GetProfitByRarity();

        sellPrice = buyPrice + profit;

        if (sellPrice < buyPrice + 1)
            sellPrice = buyPrice + 1;

        startQuantity = GetStartQuantityByRarity();

        // Giữ lại cho code cũ nếu còn gọi price
        price = sellPrice;
    }

    private int GetBaseBuyPriceByRarity()
    {
        switch (rarity)
        {
            case HerbRarity.Common:
                return 2;

            case HerbRarity.Uncommon:
                return 4;

            case HerbRarity.Rare:
                return 7;

            case HerbRarity.Toxic:
                return 8;

            case HerbRarity.Precious:
                return 12;

            default:
                return 2;
        }
    }

    private int GetCategoryBonus()
    {
        switch (category)
        {
            case HerbCategory.GiaiBieu:
                return 0;

            case HerbCategory.ThanhNhiet:
                return 0;

            case HerbCategory.HoaDamChiHo:
                return 1;

            case HerbCategory.LyKhi:
                return 1;

            case HerbCategory.TieuThuc:
                return 1;

            case HerbCategory.HoatHuyet:
                return 2;

            case HerbCategory.LoiThuy:
                return 1;

            case HerbCategory.BoKhiHuyet:
                return 2;

            case HerbCategory.BoThan:
                return 3;

            case HerbCategory.AnThan:
                return 2;

            case HerbCategory.DocTinh:
                return 3;

            case HerbCategory.Khac:
                return 0;

            default:
                return 0;
        }
    }

    private int GetLevelBonus()
    {
        if (unlockClinicLevel <= 1)
            return 0;

        if (unlockClinicLevel == 2)
            return 1;

        if (unlockClinicLevel == 3)
            return 2;

        if (unlockClinicLevel == 4)
            return 3;

        return 4;
    }

    private int GetProfitByRarity()
    {
        switch (rarity)
        {
            case HerbRarity.Common:
                return 1;

            case HerbRarity.Uncommon:
                return 2;

            case HerbRarity.Rare:
                return 3;

            case HerbRarity.Toxic:
                return 4;

            case HerbRarity.Precious:
                return 5;

            default:
                return 1;
        }
    }

    private int GetStartQuantityByRarity()
    {
        switch (rarity)
        {
            case HerbRarity.Common:
                return 50;

            case HerbRarity.Uncommon:
                return 40;

            case HerbRarity.Rare:
                return 30;

            case HerbRarity.Toxic:
                return 15;

            case HerbRarity.Precious:
                return 5;

            default:
                return 50;
        }
    }
}