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

    public HerbCategory category;
    public HerbRarity rarity;

    [Header("Gameplay")]
    public int unlockClinicLevel = 1;

    [Tooltip("Giữ lại để code cũ không lỗi. Giá này tự bằng sellPrice.")]
    public int price = 5;

    [Header("Giá mua / giá kê thuốc")]
    [Tooltip("Giá người chơi mua dược liệu từ thương nhân.")]
    public int buyPrice = 10;

    [Tooltip("Giá tính cho NPC bệnh nhân khi vị thuốc này nằm trong đơn.")]
    public int sellPrice = 15;

    [Header("Kho thuốc")]
    [Tooltip("Số lượng ban đầu trong kho khi khởi tạo game.")]
    public int startQuantity = 5;

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

        int basePrice = GetBasePriceByRarity();
        int categoryBonus = GetCategoryBonus();
        int levelBonus = (unlockClinicLevel - 1) * 5;

        buyPrice = basePrice + categoryBonus + levelBonus;

        if (buyPrice < 1)
            buyPrice = 1;

        float sellMultiplier = GetSellMultiplierByRarity();

        sellPrice = Mathf.RoundToInt(buyPrice * sellMultiplier);

        if (sellPrice <= buyPrice)
            sellPrice = buyPrice + 1;

        startQuantity = GetStartQuantityByRarity();

        price = sellPrice;
    }

    private int GetBasePriceByRarity()
    {
        switch (rarity)
        {
            case HerbRarity.Common:
                return 8;

            case HerbRarity.Uncommon:
                return 15;

            case HerbRarity.Rare:
                return 28;

            case HerbRarity.Toxic:
                return 35;

            case HerbRarity.Precious:
                return 50;

            default:
                return 8;
        }
    }

    private int GetCategoryBonus()
    {
        switch (category)
        {
            case HerbCategory.GiaiBieu:
                return 0;

            case HerbCategory.ThanhNhiet:
                return 2;

            case HerbCategory.HoaDamChiHo:
                return 3;

            case HerbCategory.LyKhi:
                return 4;

            case HerbCategory.TieuThuc:
                return 4;

            case HerbCategory.HoatHuyet:
                return 7;

            case HerbCategory.LoiThuy:
                return 5;

            case HerbCategory.BoKhiHuyet:
                return 9;

            case HerbCategory.BoThan:
                return 12;

            case HerbCategory.AnThan:
                return 9;

            case HerbCategory.DocTinh:
                return 14;

            case HerbCategory.Khac:
                return 0;

            default:
                return 0;
        }
    }

    private float GetSellMultiplierByRarity()
    {
        switch (rarity)
        {
            case HerbRarity.Common:
                return 1.10f;

            case HerbRarity.Uncommon:
                return 1.12f;

            case HerbRarity.Rare:
                return 1.15f;

            case HerbRarity.Toxic:
                return 1.18f;

            case HerbRarity.Precious:
                return 1.22f;

            default:
                return 1.10f;
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