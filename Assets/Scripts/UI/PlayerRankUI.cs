using TMPro;
using UnityEngine;

public class PlayerRankUI : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;

    [Header("Test cấp bậc")]
    [SerializeField] private bool useTestReputation = true;
    [SerializeField] private int testReputation = 0;

    private int lastReputation = -1;

    private void Start()
    {
        RefreshRankText();
    }

    private void Update()
    {
        int currentReputation = GetCurrentReputation();

        if (currentReputation == lastReputation)
            return;

        RefreshRankText();
    }

    private void RefreshRankText()
    {
        if (rankText == null)
            return;

        int reputation = GetCurrentReputation();

        lastReputation = reputation;
        rankText.text = GetRankName(reputation);
    }

    private int GetCurrentReputation()
    {
        if (useTestReputation)
        {
            return testReputation;
        }

        if (PlayerEconomy.Instance != null)
        {
            return PlayerEconomy.Instance.Reputation;
        }

        return testReputation;
    }

    private string GetRankName(int reputation)
    {
        if (reputation < 100) return "Y Sinh";
        if (reputation < 200) return "Lương Y";
        if (reputation < 300) return "Đại Phu";
        if (reputation < 500) return "Danh Y";
        return "Lương Y Đại Việt";
    }
}