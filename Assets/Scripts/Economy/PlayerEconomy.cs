using System;
using System.IO;
using UnityEngine;

public class PlayerEconomy : MonoBehaviour
{
    public static PlayerEconomy Instance { get; private set; }

    [Header("Tiền mặc định khi chưa có save")]
    [SerializeField] private int money = 200;

    [Header("Tín nhiệm mặc định khi chưa có save")]
    [SerializeField] private int reputation = 0;

    public int Money { get { return money; } }
    public int Reputation { get { return reputation; } }

    private string SavePath
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, "player_economy_save.json");
        }
    }

    [Serializable]
    private class PlayerEconomySaveData
    {
        public int money;
        public int reputation;
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

        LoadEconomy();
    }

    public void AddMoney(int amount)
    {
        if (amount <= 0)
            return;

        money += amount;

        Debug.Log("Nhận tiền: +" + amount + ". Tiền hiện tại: " + money);

        SaveEconomy();
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

        SaveEconomy();

        return true;
    }

    public void AddReputation(int amount)
    {
        reputation += amount;

        if (reputation < 0)
            reputation = 0;

        Debug.Log("Tín nhiệm thay đổi: " + amount + ". Tín nhiệm hiện tại: " + reputation);

        SaveEconomy();
    }

    public void SetMoney(int newMoney)
    {
        money = Mathf.Max(0, newMoney);
        SaveEconomy();
    }

    public void SetReputation(int newReputation)
    {
        reputation = Mathf.Max(0, newReputation);
        SaveEconomy();
    }

    public void SaveEconomy()
    {
        PlayerEconomySaveData saveData = new PlayerEconomySaveData();
        saveData.money = money;
        saveData.reputation = reputation;

        string json = JsonUtility.ToJson(saveData, true);

        File.WriteAllText(SavePath, json);

        Debug.Log("Đã lưu tiền/tín nhiệm tại: " + SavePath);
    }

    private void LoadEconomy()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("Chưa có file save tiền/tín nhiệm. Dùng giá trị mặc định.");
            SaveEconomy();
            return;
        }

        string json = File.ReadAllText(SavePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning("File save tiền/tín nhiệm rỗng. Dùng giá trị mặc định.");
            SaveEconomy();
            return;
        }

        PlayerEconomySaveData saveData = JsonUtility.FromJson<PlayerEconomySaveData>(json);

        if (saveData == null)
        {
            Debug.LogWarning("Không đọc được file save tiền/tín nhiệm. Dùng giá trị mặc định.");
            SaveEconomy();
            return;
        }

        money = Mathf.Max(0, saveData.money);
        reputation = Mathf.Max(0, saveData.reputation);

        Debug.Log("Đã load tiền/tín nhiệm. Tiền: " + money + ", tín nhiệm: " + reputation);
    }

    public void DeleteEconomySave()
    {
        if (File.Exists(SavePath))
        {
            File.Delete(SavePath);
            Debug.Log("Đã xóa save tiền/tín nhiệm.");
        }

        money = 200;
        reputation = 0;

        SaveEconomy();
    }

    private void OnApplicationQuit()
    {
        SaveEconomy();
    }

    private void OnDestroy()
    {
        if (Instance == this)
        {
            SaveEconomy();
        }
    }
}