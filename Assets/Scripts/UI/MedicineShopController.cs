using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class MedicineShopController : MonoBehaviour
{
    [Header("Database")]
    [SerializeField] private MedicalDatabase medicalDatabase;

    [Header("UI Prefabs")]
    [SerializeField] private ShopHerbItemUI shopHerbItemPrefab;
    [SerializeField] private ShopSelectedHerbItemUI selectedHerbItemPrefab;

    [Header("UI Roots")]
    [SerializeField] private Transform shopContent;
    [SerializeField] private Transform selectedContent;

    [Header("Buttons")]
    [SerializeField] private Button buyButton;

    [Header("Panel")]
    [SerializeField] private GameObject shopPanel;

    private readonly Dictionary<HerbData, int> selectedHerbs = new Dictionary<HerbData, int>();

    private void Awake()
    {
        if (buyButton != null)
        {
            buyButton.onClick.AddListener(BuySelectedHerbs);
        }
    }

    private void OnEnable()
    {
        RefreshShopList();
        RefreshSelectedList();
    }

    public void OpenShop()
    {
        gameObject.SetActive(true);
        RefreshShopList();
        RefreshSelectedList();
    }

    public void CloseShop()
    {
        if (shopPanel != null)
        {
            shopPanel.SetActive(false);
        }
        else
        {
            gameObject.SetActive(false);
        }
    }

    private void RefreshShopList()
    {
        ClearChildren(shopContent);

        if (medicalDatabase == null)
        {
            Debug.LogWarning("MedicineShopController chưa gán MedicalDatabase.");
            return;
        }

        if (shopHerbItemPrefab == null)
        {
            Debug.LogWarning("MedicineShopController chưa gán ShopHerbItem prefab.");
            return;
        }

        int currentClinicLevel = 1;

        if (HerbInventory.Instance != null)
        {
            currentClinicLevel = HerbInventory.Instance.ClinicLevel;
        }

        List<HerbData> unlockedHerbs = medicalDatabase.GetUnlockedHerbs(currentClinicLevel);

        foreach (HerbData herb in unlockedHerbs)
        {
            if (herb == null)
            {
                continue;
            }

            ShopHerbItemUI item = Instantiate(shopHerbItemPrefab, shopContent);
            item.Setup(herb, AddHerbToSelected);
        }
    }

    private void AddHerbToSelected(HerbData herb)
    {
        if (herb == null)
        {
            return;
        }

        if (!selectedHerbs.ContainsKey(herb))
        {
            selectedHerbs.Add(herb, 0);
        }

        selectedHerbs[herb]++;

        RefreshSelectedList();
    }

    private void RemoveOneSelectedHerb(HerbData herb)
    {
        if (herb == null)
        {
            return;
        }

        if (!selectedHerbs.ContainsKey(herb))
        {
            return;
        }

        selectedHerbs[herb]--;

        if (selectedHerbs[herb] <= 0)
        {
            selectedHerbs.Remove(herb);
        }

        RefreshSelectedList();
    }

    private void RefreshSelectedList()
    {
        ClearChildren(selectedContent);

        if (selectedHerbItemPrefab == null)
        {
            return;
        }

        foreach (KeyValuePair<HerbData, int> pair in selectedHerbs)
        {
            HerbData herb = pair.Key;
            int quantity = pair.Value;

            if (herb == null || quantity <= 0)
            {
                continue;
            }

            ShopSelectedHerbItemUI item = Instantiate(selectedHerbItemPrefab, selectedContent);
            item.Setup(herb, quantity, RemoveOneSelectedHerb);
        }
    }

    private void BuySelectedHerbs()
    {
        if (selectedHerbs.Count == 0)
        {
            Debug.Log("Chưa chọn dược liệu để mua.");
            return;
        }

        if (PlayerEconomy.Instance == null)
        {
            Debug.LogError("Không tìm thấy PlayerEconomy.");
            return;
        }

        if (HerbInventory.Instance == null)
        {
            Debug.LogError("Không tìm thấy HerbInventory.");
            return;
        }

        int totalCost = CalculateTotalCost();

        if (PlayerEconomy.Instance.Money < totalCost)
        {
            Debug.LogWarning("Không đủ tiền mua thuốc. Cần: " + totalCost + ", hiện có: " + PlayerEconomy.Instance.Money);
            return;
        }

        bool paid = PlayerEconomy.Instance.SpendMoney(totalCost);

        if (!paid)
        {
            return;
        }

        foreach (KeyValuePair<HerbData, int> pair in selectedHerbs)
        {
            HerbData herb = pair.Key;
            int quantity = pair.Value;

            if (herb == null || quantity <= 0)
            {
                continue;
            }

            HerbInventory.Instance.AddHerb(herb, quantity);

            NotifyQuestHerbBought(herb, quantity);
        }

        Debug.Log("Đã mua thuốc. Tổng tiền: " + totalCost);

        selectedHerbs.Clear();
        RefreshSelectedList();
    }

    private void NotifyQuestHerbBought(HerbData herb, int quantity)
    {
        if (herb == null || quantity <= 0)
            return;

        if (QuestProgressManager.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy QuestProgressManager để ghi nhiệm vụ mua dược liệu.");
            return;
        }

        int lineCost = herb.buyPrice * quantity;

        QuestProgressManager.Instance.RecordHerbBought(
            herb,
            quantity,
            lineCost
        );

        Debug.Log("Đã ghi nhiệm vụ mua dược liệu: " + herb.herbName + " x" + quantity);
    }

    private int CalculateTotalCost()
    {
        int total = 0;

        foreach (KeyValuePair<HerbData, int> pair in selectedHerbs)
        {
            HerbData herb = pair.Key;
            int quantity = pair.Value;

            if (herb == null || quantity <= 0)
            {
                continue;
            }

            total += herb.buyPrice * quantity;
        }

        return total;
    }

    private void ClearChildren(Transform root)
    {
        if (root == null)
        {
            return;
        }

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Destroy(root.GetChild(i).gameObject);
        }
    }
}