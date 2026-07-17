using System;
using System.IO;
using UnityEngine;

public class PlayerEconomy : MonoBehaviour
{
    public static PlayerEconomy Instance { get; private set; }

    private const string SaveFileName = "player_economy_save.json";

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
            return GameSavePath.GetSavePath(SaveFileName);
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
        string savePath = SavePath;

        File.WriteAllText(savePath, json);

        Debug.Log("Đã lưu tiền/tín nhiệm tại: " + savePath);
    }

    private void LoadEconomy()
    {
        GameSavePath.MigrateLegacyRootSave(SaveFileName);

        string savePath = SavePath;

        if (!File.Exists(savePath))
        {
            Debug.Log("Chưa có file save tiền/tín nhiệm. Dùng giá trị mặc định.");
            SaveEconomy();
            return;
        }

        LoadEconomyFromPath(savePath);
    }

    private void LoadEconomyFromPath(string path)
    {
        if (!File.Exists(path))
        {
            Debug.LogWarning("Không tìm thấy file save tiền/tín nhiệm tại: " + path);
            return;
        }

        string json = File.ReadAllText(path);

        if (string.IsNullOrWhiteSpace(json))
        {
            Debug.LogWarning("File save tiền/tín nhiệm rỗng. Dùng giá trị mặc định.");

            money = 200;
            reputation = 0;

            SaveEconomy();
            return;
        }

        PlayerEconomySaveData saveData = JsonUtility.FromJson<PlayerEconomySaveData>(json);

        if (saveData == null)
        {
            Debug.LogWarning("Không đọc được file save tiền/tín nhiệm. Dùng giá trị mặc định.");

            money = 200;
            reputation = 0;

            SaveEconomy();
            return;
        }

        money = Mathf.Max(0, saveData.money);
        reputation = Mathf.Max(0, saveData.reputation);

        Debug.Log(
            "Đã load tiền/tín nhiệm từ: " + path
            + " | Tiền: " + money
            + " | Tín nhiệm: " + reputation
        );
    }

    public void DeleteEconomySave()
    {
        GameSavePath.DeleteSaveAndLegacy(SaveFileName);

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