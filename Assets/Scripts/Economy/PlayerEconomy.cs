using UnityEngine;

public class PlayerEconomy : MonoBehaviour
{
    public static PlayerEconomy Instance { get; private set; }

    [Header("Tiền")]
    [SerializeField] private int money = 200;

    [Header("Tín nhiệm")]
    [SerializeField] private int reputation = 0;

    public int Money
    {
        get { return money; }
    }

    public int Reputation
    {
        get { return reputation; }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0)
            return;

        money += amount;

        Debug.Log("Nhận tiền: +" + amount + ". Tiền hiện tại: " + money);
    }

    public bool SpendMoney(int amount)
    {
        if (amount <= 0)
            return true;

        if (money < amount)
        {
            Debug.LogWarning("Không đủ tiền. Cần: " + amount + ", hiện có: " + money);
            return false;
        }

        money -= amount;

        Debug.Log("Trừ tiền: -" + amount + ". Tiền hiện tại: " + money);
        return true;
    }

    public void AddReputation(int amount)
    {
        reputation += amount;

        if (reputation < 0)
            reputation = 0;

        Debug.Log("Tín nhiệm thay đổi: " + amount + ". Tín nhiệm hiện tại: " + reputation);
    }

    public void SetMoney(int newMoney)
    {
        money = Mathf.Max(0, newMoney);
    }

    public void SetReputation(int newReputation)
    {
        reputation = Mathf.Max(50, newReputation);
    }
}