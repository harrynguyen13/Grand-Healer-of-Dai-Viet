using TMPro;
using UnityEngine;

public class PlayerCurrencyUI : MonoBehaviour
{
    [Header("Text hiển thị")]
    [SerializeField] private TMP_Text moneyText;
    [SerializeField] private TMP_Text reputationText;

    [Header("Test nếu muốn test UI")]
    [SerializeField] private bool useTestValue = false;
    [SerializeField] private int testMoney = 200;
    [SerializeField] private int testReputation = 0;

    private int lastMoney = -1;
    private int lastReputation = -1;

    private void Start()
    {
        RefreshUI();
    }

    private void Update()
    {
        int currentMoney = GetMoney();
        int currentReputation = GetReputation();

        if (currentMoney == lastMoney && currentReputation == lastReputation)
            return;

        RefreshUI();
    }

    private void RefreshUI()
    {
        int money = GetMoney();
        int reputation = GetReputation();

        lastMoney = money;
        lastReputation = reputation;

        if (moneyText != null)
            moneyText.text = money.ToString();

        if (reputationText != null)
            reputationText.text = GetReputationProgressText(reputation);
    }

    private int GetMoney()
    {
        if (useTestValue)
            return testMoney;

        if (PlayerEconomy.Instance != null)
            return PlayerEconomy.Instance.Money;

        return testMoney;
    }

    private int GetReputation()
    {
        if (useTestValue)
            return testReputation;

        if (PlayerEconomy.Instance != null)
            return PlayerEconomy.Instance.Reputation;

        return testReputation;
    }

    private string GetReputationProgressText(int reputation)
    {
        int nextTarget =
            PlayerLevelService.GetNextTargetReputation(reputation);

        if (reputation >= PlayerLevelService.DanhYTarget)
            return reputation + " / MAX";

        return reputation + " / " + nextTarget;
    }
}