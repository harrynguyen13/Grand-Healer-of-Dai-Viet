using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HerbRoleTooltipUI : MonoBehaviour
{
    public static HerbRoleTooltipUI Instance { get; private set; }

    [Header("Root")]
    [SerializeField] private GameObject tooltipRoot;
    [SerializeField] private RectTransform tooltipRect;
    [SerializeField] private RectTransform canvasRect;

    [Header("Text")]
    [SerializeField] private TextMeshProUGUI herbNameText;
    [SerializeField] private TextMeshProUGUI mainRoleText;
    [SerializeField] private TextMeshProUGUI subRoleText;

    [Header("Vị trí cạnh chuột")]
    [SerializeField] private Vector2 mouseOffset = new Vector2(18f, -18f);
    [SerializeField] private float screenMargin = 12f;

    private void Awake()
    {
        Instance = this;

        if (tooltipRect != null)
        {
            tooltipRect.anchorMin = new Vector2(0.5f, 0.5f);
            tooltipRect.anchorMax = new Vector2(0.5f, 0.5f);
            tooltipRect.pivot = new Vector2(0f, 1f);
        }

        Hide();
    }

    public void Show(HerbData herb, Vector2 mouseScreenPosition)
    {
        if (herb == null)
        {
            Hide();
            return;
        }

        SetText(herb);

        if (tooltipRoot != null)
            tooltipRoot.SetActive(true);

        SetPositionByMouse(mouseScreenPosition);
    }

    public void Move(Vector2 mouseScreenPosition)
    {
        if (tooltipRoot == null || !tooltipRoot.activeSelf)
            return;

        SetPositionByMouse(mouseScreenPosition);
    }

    public void Hide()
    {
        if (tooltipRoot != null)
            tooltipRoot.SetActive(false);
    }

    private void SetText(HerbData herb)
    {
        if (herbNameText != null)
            herbNameText.text = herb.herbName;

        string roleText = herb.treatmentRoleText;

        if (string.IsNullOrWhiteSpace(roleText))
            roleText = GetFallbackTreatmentRole(herb);

        string mainRole = GetMainRole(roleText);
        string subRoles = GetSubRoles(roleText);

        if (mainRoleText != null)
        {
            if (string.IsNullOrWhiteSpace(mainRole))
                mainRoleText.text = "<b>Chính:</b> chưa có dữ liệu";
            else
                mainRoleText.text = "<b>Chính:</b> " + mainRole;
        }

        if (subRoleText != null)
        {
            if (string.IsNullOrWhiteSpace(subRoles))
                subRoleText.text = "<b>Phụ:</b> không có";
            else
                subRoleText.text = "<b>Phụ:</b> " + subRoles;
        }
    }

    private void SetPositionByMouse(Vector2 mouseScreenPosition)
    {
        if (tooltipRect == null || canvasRect == null)
            return;

        tooltipRect.anchorMin = new Vector2(0.5f, 0.5f);
        tooltipRect.anchorMax = new Vector2(0.5f, 0.5f);
        tooltipRect.pivot = new Vector2(0f, 1f);

        Vector2 localPoint;

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRect,
            mouseScreenPosition,
            null,
            out localPoint
        );

        Vector2 finalPosition = localPoint + mouseOffset;

        float tooltipWidth = tooltipRect.rect.width;
        float tooltipHeight = tooltipRect.rect.height;

        float leftLimit = canvasRect.rect.xMin + screenMargin;
        float rightLimit = canvasRect.rect.xMax - tooltipWidth - screenMargin;
        float topLimit = canvasRect.rect.yMax - screenMargin;
        float bottomLimit = canvasRect.rect.yMin + tooltipHeight + screenMargin;

        finalPosition.x = Mathf.Clamp(finalPosition.x, leftLimit, rightLimit);
        finalPosition.y = Mathf.Clamp(finalPosition.y, bottomLimit, topLimit);

        tooltipRect.anchoredPosition = finalPosition;
    }

    private string GetMainRole(string roleText)
    {
        if (string.IsNullOrWhiteSpace(roleText))
            return "";

        string[] parts = roleText.Replace("/", ",").Split(',');

        if (parts.Length == 0)
            return "";

        return ToLowerFirstLetter(parts[0].Trim());
    }

    private string GetSubRoles(string roleText)
    {
        if (string.IsNullOrWhiteSpace(roleText))
            return "";

        string[] parts = roleText.Replace("/", ",").Split(',');

        if (parts.Length <= 1)
            return "";

        List<string> roles = new List<string>();

        for (int i = 1; i < parts.Length; i++)
        {
            string role = parts[i].Trim();

            if (string.IsNullOrWhiteSpace(role))
                continue;

            role = ToLowerFirstLetter(role);

            if (!roles.Contains(role))
                roles.Add(role);
        }

        if (roles.Count == 0)
            return "";

        return string.Join(", ", roles) + ".";
    }

    private string ToLowerFirstLetter(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";

        text = text.Trim();

        if (text.Length == 1)
            return text.ToLower();

        return char.ToLower(text[0]) + text.Substring(1);
    }

    private string GetFallbackTreatmentRole(HerbData herb)
    {
        if (herb == null)
            return "hỗ trợ điều trị";

        switch (herb.category)
        {
            case HerbCategory.GiaiBieu:
                return "giải biểu, khu phong";

            case HerbCategory.ThanhNhiet:
                return "thanh nhiệt, giải độc";

            case HerbCategory.HoaDamChiHo:
                return "hóa đờm, chỉ ho";

            case HerbCategory.LyKhi:
                return "lý khí, hành khí";

            case HerbCategory.TieuThuc:
                return "tiêu thực, hỗ trợ tiêu hóa";

            case HerbCategory.HoatHuyet:
                return "hoạt huyết, hóa ứ";

            case HerbCategory.LoiThuy:
                return "lợi thủy, thông tiểu";

            case HerbCategory.BoKhiHuyet:
                return "bổ khí huyết, phục hồi";

            case HerbCategory.BoThan:
                return "bổ thận, mạnh gân cốt";

            case HerbCategory.AnThan:
                return "an thần, dưỡng tâm";

            case HerbCategory.DocTinh:
                return "dược tính mạnh, dùng thận trọng";

            case HerbCategory.Khac:
                return "hỗ trợ điều trị theo phối ngũ";

            default:
                return "hỗ trợ điều trị theo phối ngũ";
        }
    }
}