using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class HerbInventory : MonoBehaviour
{
    public static HerbInventory Instance { get; private set; }

    private const string SaveFileName = "herb_inventory_save.json";

    [System.Serializable]
    public class HerbStock
    {
        public HerbData herb;
        public int quantity;
    }

    [System.Serializable]
    private class HerbInventorySaveData
    {
        public List<HerbStockSaveData> stocks = new List<HerbStockSaveData>();
    }

    [System.Serializable]
    private class HerbStockSaveData
    {
        public string herbKey;
        public int quantity;
    }

    [Header("Database")]
    [SerializeField] private MedicalDatabase medicalDatabase;

    [Header("Tự khởi tạo kho theo dược liệu đã mở khóa")]
    [SerializeField] private bool initializeOnAwake = true;

    [Header("Danh sách kho thuốc")]
    [SerializeField] private List<HerbStock> herbStocks = new List<HerbStock>();

    private readonly Dictionary<HerbData, int> stockLookup = new Dictionary<HerbData, int>();

    private string SavePath
    {
        get
        {
            return GameSavePath.GetSavePath(SaveFileName);
        }
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

        BuildLookup();

        GameSavePath.MigrateLegacyRootSave(SaveFileName);

        if (File.Exists(SavePath))
        {
            LoadInventory();
        }
        else
        {
            if (initializeOnAwake)
            {
                RefreshUnlockedHerbsByPlayerLevel(false);
                SaveInventory();
            }
        }

        Debug.Log("File save kho thuốc nằm tại: " + SavePath);
    }

    private void Start()
    {
        RefreshUnlockedHerbsByPlayerLevel(true);
    }

    private void OnApplicationQuit()
    {
        SaveInventory();
    }

    private void OnDisable()
    {
        if (Instance == this)
        {
            SaveInventory();
        }
    }

    private int GetCurrentClinicLevel()
    {
        int currentLevel = PlayerLevelService.GetCurrentUnlockLevel();

        if (currentLevel > 0)
            return currentLevel;

        return 1;
    }

    private void BuildLookup()
    {
        stockLookup.Clear();

        foreach (HerbStock stock in herbStocks)
        {
            if (stock == null || stock.herb == null)
                continue;

            if (!stockLookup.ContainsKey(stock.herb))
            {
                stockLookup.Add(stock.herb, Mathf.Max(0, stock.quantity));
            }
        }

        SyncAllListValues();
    }

    public List<HerbStock> GetAllStocks()
    {
        RefreshUnlockedHerbsByPlayerLevel(false);
        return herbStocks;
    }

    public void RefreshUnlockedHerbsByPlayerLevel()
    {
        RefreshUnlockedHerbsByPlayerLevel(true);
    }

    public void RefreshUnlockedHerbsByPlayerLevel(bool saveAfterUnlock)
    {
        int currentLevel = GetCurrentClinicLevel();
        UnlockHerbsForLevel(currentLevel, saveAfterUnlock);
    }

    private void UnlockHerbsForLevel(int level, bool saveAfterUnlock)
    {
        level = Mathf.Max(1, level);

        if (medicalDatabase == null)
        {
            Debug.LogWarning("HerbInventory chưa kéo MedicalDatabase.");
            return;
        }

        foreach (HerbData herb in medicalDatabase.herbs)
        {
            if (herb == null)
                continue;

            if (herb.unlockClinicLevel > level)
                continue;

            if (stockLookup.ContainsKey(herb))
                continue;

            int startQuantity = Mathf.Max(0, herb.startQuantity);

            stockLookup.Add(herb, startQuantity);

            HerbStock newStock = new HerbStock();
            newStock.herb = herb;
            newStock.quantity = startQuantity;
            herbStocks.Add(newStock);
        }

        SyncAllListValues();

        Debug.Log("Đã đồng bộ kho dược liệu theo cấp " + level + ". Tổng vị trong kho: " + herbStocks.Count);

        if (saveAfterUnlock)
        {
            SaveInventory();
        }
    }

    public int GetQuantity(HerbData herb)
    {
        RefreshUnlockedHerbsByPlayerLevel(false);

        if (herb == null)
            return 0;

        if (!stockLookup.ContainsKey(herb))
            return 0;

        return stockLookup[herb];
    }

    public bool HasEnough(HerbData herb, int amount)
    {
        if (herb == null)
            return false;

        if (amount <= 0)
            return true;

        return GetQuantity(herb) >= amount;
    }

    public bool HasEnoughPrescription(Dictionary<HerbData, int> prescription)
    {
        RefreshUnlockedHerbsByPlayerLevel(false);

        if (prescription == null)
            return false;

        foreach (KeyValuePair<HerbData, int> pair in prescription)
        {
            HerbData herb = pair.Key;
            int amount = pair.Value;

            if (herb == null)
                continue;

            if (!HasEnough(herb, amount))
            {
                Debug.LogWarning("Không đủ dược liệu: " + herb.herbName + ". Cần: " + amount + ", còn: " + GetQuantity(herb));
                return false;
            }
        }

        return true;
    }

    public bool RemoveHerb(HerbData herb, int amount)
    {
        RefreshUnlockedHerbsByPlayerLevel(false);

        if (herb == null || amount <= 0)
            return false;

        if (!HasEnough(herb, amount))
        {
            Debug.LogWarning("Không đủ " + herb.herbName + " để trừ.");
            return false;
        }

        stockLookup[herb] -= amount;
        SyncListValue(herb, stockLookup[herb]);

        SaveInventory();

        Debug.Log("Trừ dược liệu: " + herb.herbName + " x" + amount + ". Còn: " + stockLookup[herb]);

        return true;
    }

    public bool RemovePrescription(Dictionary<HerbData, int> prescription)
    {
        RefreshUnlockedHerbsByPlayerLevel(false);

        if (prescription == null)
            return false;

        if (!HasEnoughPrescription(prescription))
            return false;

        foreach (KeyValuePair<HerbData, int> pair in prescription)
        {
            HerbData herb = pair.Key;
            int amount = pair.Value;

            if (herb == null || amount <= 0)
                continue;

            if (!stockLookup.ContainsKey(herb))
                continue;

            stockLookup[herb] -= amount;
            SyncListValue(herb, stockLookup[herb]);

            Debug.Log("Trừ dược liệu: " + herb.herbName + " x" + amount + ". Còn: " + stockLookup[herb]);
        }

        SaveInventory();

        Debug.Log("Đã lưu kho thuốc sau khi kê đơn.");

        return true;
    }

    public void AddHerb(HerbData herb, int amount)
    {
        RefreshUnlockedHerbsByPlayerLevel(false);

        if (herb == null || amount <= 0)
            return;

        if (!stockLookup.ContainsKey(herb))
        {
            stockLookup.Add(herb, 0);

            HerbStock newStock = new HerbStock();
            newStock.herb = herb;
            newStock.quantity = 0;
            herbStocks.Add(newStock);
        }

        stockLookup[herb] += amount;
        SyncListValue(herb, stockLookup[herb]);

        SaveInventory();

        Debug.Log("Thêm dược liệu: " + herb.herbName + " x" + amount + ". Tổng: " + stockLookup[herb]);
    }

    public bool BuyHerb(HerbData herb, int amount)
    {
        RefreshUnlockedHerbsByPlayerLevel(false);

        if (herb == null || amount <= 0)
            return false;

        if (PlayerEconomy.Instance == null)
        {
            Debug.LogError("Không có PlayerEconomy.");
            return false;
        }

        int totalCost = herb.buyPrice * amount;

        if (!PlayerEconomy.Instance.SpendMoney(totalCost))
            return false;

        AddHerb(herb, amount);

        Debug.Log("Mua dược liệu: " + herb.herbName + " x" + amount + ". Giá: " + totalCost);
        return true;
    }

    private void SyncListValue(HerbData herb, int newQuantity)
    {
        foreach (HerbStock stock in herbStocks)
        {
            if (stock != null && stock.herb == herb)
            {
                stock.quantity = newQuantity;
                return;
            }
        }
    }

    private void SyncAllListValues()
    {
        foreach (HerbStock stock in herbStocks)
        {
            if (stock == null || stock.herb == null)
                continue;

            if (stockLookup.ContainsKey(stock.herb))
            {
                stock.quantity = stockLookup[stock.herb];
            }
        }
    }

    [ContextMenu("Save Herb Inventory Now")]
    public void SaveInventory()
    {
        HerbInventorySaveData saveData = new HerbInventorySaveData();

        foreach (HerbStock stock in herbStocks)
        {
            if (stock == null || stock.herb == null)
                continue;

            HerbStockSaveData saveStock = new HerbStockSaveData();
            saveStock.herbKey = NormalizeKey(stock.herb.name);
            saveStock.quantity = Mathf.Max(0, stock.quantity);

            saveData.stocks.Add(saveStock);
        }

        string json = JsonUtility.ToJson(saveData, true);

        try
        {
            File.WriteAllText(SavePath, json);
            Debug.Log("Đã lưu kho thuốc tại: " + SavePath);
        }
        catch (Exception e)
        {
            Debug.LogError("Lỗi lưu kho thuốc: " + e.Message);
        }
    }

    [ContextMenu("Load Herb Inventory Now")]
    public void LoadInventory()
    {
        if (!File.Exists(SavePath))
        {
            Debug.Log("Chưa có file save kho thuốc. Dùng số lượng ban đầu.");
            return;
        }

        if (medicalDatabase == null)
        {
            Debug.LogWarning("Không load được kho thuốc vì chưa kéo MedicalDatabase.");
            return;
        }

        try
        {
            string json = File.ReadAllText(SavePath);
            HerbInventorySaveData saveData = JsonUtility.FromJson<HerbInventorySaveData>(json);

            if (saveData == null)
            {
                Debug.LogWarning("File save kho thuốc rỗng hoặc lỗi.");
                return;
            }

            herbStocks.Clear();
            stockLookup.Clear();

            if (initializeOnAwake)
            {
                RefreshUnlockedHerbsByPlayerLevel(false);
            }

            foreach (HerbStockSaveData savedStock in saveData.stocks)
            {
                if (savedStock == null || string.IsNullOrEmpty(savedStock.herbKey))
                    continue;

                HerbData herb = FindHerbByKey(savedStock.herbKey);

                if (herb == null)
                {
                    Debug.LogWarning("Không tìm thấy HerbData khi load kho: " + savedStock.herbKey);
                    continue;
                }

                SetHerbQuantity(herb, savedStock.quantity);
            }

            RefreshUnlockedHerbsByPlayerLevel(false);
            SyncAllListValues();

            Debug.Log("Đã load kho thuốc từ file save: " + SavePath + ". Tổng vị trong kho: " + herbStocks.Count);
        }
        catch (Exception e)
        {
            Debug.LogError("Lỗi load kho thuốc: " + e.Message);
        }
    }

    [ContextMenu("Delete Herb Inventory Save")]
    public void DeleteInventorySave()
    {
        GameSavePath.DeleteSaveAndLegacy(SaveFileName);
    }

    public void ResetInventoryForNewGame()
    {
        Debug.Log("===== RESET KHO THUỐC VỀ BAN ĐẦU =====");

        herbStocks.Clear();
        stockLookup.Clear();

        DeleteInventorySave();

        if (initializeOnAwake)
        {
            UnlockHerbsForLevel(1, false);
        }

        SyncAllListValues();

        SaveInventory();

        Debug.Log("Đã reset kho thuốc về mặc định cấp 1. Tổng vị: " + herbStocks.Count);
    }

    private void SetHerbQuantity(HerbData herb, int quantity)
    {
        if (herb == null)
            return;

        quantity = Mathf.Max(0, quantity);

        if (!stockLookup.ContainsKey(herb))
        {
            stockLookup.Add(herb, quantity);

            HerbStock newStock = new HerbStock();
            newStock.herb = herb;
            newStock.quantity = quantity;
            herbStocks.Add(newStock);
        }
        else
        {
            stockLookup[herb] = quantity;
            SyncListValue(herb, quantity);
        }
    }

    private HerbData FindHerbByKey(string herbKey)
    {
        if (medicalDatabase == null)
            return null;

        foreach (HerbData herb in medicalDatabase.herbs)
        {
            if (herb == null)
                continue;

            if (NormalizeKey(herb.name) == herbKey)
                return herb;
        }

        return null;
    }

    private string NormalizeKey(string value)
    {
        if (string.IsNullOrEmpty(value))
            return "";

        string key = value.Trim().ToLowerInvariant();

        key = key.Replace(" ", "_");
        key = key.Replace("-", "_");

        while (key.Contains("__"))
        {
            key = key.Replace("__", "_");
        }

        return key;
    }
}