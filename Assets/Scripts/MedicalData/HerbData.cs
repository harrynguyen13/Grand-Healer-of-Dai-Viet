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
    public int price = 5;

    [Header("Icon UI")]
    public Sprite icon;
}