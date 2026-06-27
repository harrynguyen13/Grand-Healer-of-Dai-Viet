using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class PrescriptionUIController : MonoBehaviour
{
    [Header("Root UI")]
    [SerializeField] private GameObject prescriptionPanel;

    [Header("Database")]
    [SerializeField] private MedicalDatabase medicalDatabase;

    [Header("Cấp y quán")]
    [SerializeField] private int clinicLevel = 1;

    [Header("Kho thuốc bên trái")]
    [SerializeField] private Transform herbListContent;
    [SerializeField] private HerbItemUI herbItemPrefab;

    [Header("Gói thuốc bên phải")]
    [SerializeField] private Transform selectedHerbRoot;
    [SerializeField] private SelectedHerbItemUI selectedHerbItemPrefab;

    [Header("Nút kê đơn")]
    [SerializeField] private Button confirmPrescriptionButton;

    [Header("Số lượng test ban đầu")]
    [SerializeField] private int defaultHerbQuantity = 5;

    private readonly Dictionary<HerbData, int> herbQuantities = new Dictionary<HerbData, int>();
    private readonly Dictionary<HerbData, int> selectedHerbs = new Dictionary<HerbData, int>();
    private readonly Dictionary<HerbData, HerbItemUI> leftHerbItems = new Dictionary<HerbData, HerbItemUI>();

    private void Awake()
    {
        if (confirmPrescriptionButton != null)
        {
            confirmPrescriptionButton.onClick.RemoveAllListeners();
            confirmPrescriptionButton.onClick.AddListener(ConfirmPrescription);
        }

        Hide();
    }

    public void Show()
    {
        if (prescriptionPanel != null)
            prescriptionPanel.SetActive(true);

        selectedHerbs.Clear();

        BuildHerbList();
        RefreshSelectedHerbs();
    }

    public void Hide()
    {
        if (prescriptionPanel != null)
            prescriptionPanel.SetActive(false);
    }

    private void BuildHerbList()
    {
        if (medicalDatabase == null)
        {
            Debug.LogError("PrescriptionUIController chưa có MedicalDatabase.");
            return;
        }

        if (herbListContent == null)
        {
            Debug.LogError("PrescriptionUIController chưa kéo HerbListContent.");
            return;
        }

        if (herbItemPrefab == null)
        {
            Debug.LogError("PrescriptionUIController chưa kéo HerbItemUI prefab.");
            return;
        }

        ClearChildren(herbListContent);
        leftHerbItems.Clear();

        List<HerbData> unlockedHerbs = medicalDatabase.GetUnlockedHerbs(clinicLevel);

        foreach (HerbData herb in unlockedHerbs)
        {
            if (herb == null)
                continue;

            if (!herbQuantities.ContainsKey(herb))
                herbQuantities.Add(herb, defaultHerbQuantity);

            HerbItemUI item = Instantiate(herbItemPrefab, herbListContent);
            item.Setup(herb, herbQuantities[herb], OnClickHerbFromStorage);

            if (!leftHerbItems.ContainsKey(herb))
                leftHerbItems.Add(herb, item);
        }

        Debug.Log("Đã tạo danh sách dược liệu: " + unlockedHerbs.Count);
    }

    private void OnClickHerbFromStorage(HerbData herb)
    {
        if (herb == null)
            return;

        if (!herbQuantities.ContainsKey(herb))
            return;

        if (herbQuantities[herb] <= 0)
        {
            Debug.Log("Đã hết dược liệu: " + herb.herbName);
            return;
        }

        herbQuantities[herb]--;

        if (!selectedHerbs.ContainsKey(herb))
            selectedHerbs.Add(herb, 0);

        selectedHerbs[herb]++;

        UpdateLeftHerbItem(herb);
        RefreshSelectedHerbs();

        Debug.Log("Thêm vào gói thuốc: " + herb.herbName);
    }

    private void OnClickSelectedHerb(HerbData herb)
    {
        if (herb == null)
            return;

        if (!selectedHerbs.ContainsKey(herb))
            return;

        selectedHerbs[herb]--;

        if (selectedHerbs[herb] <= 0)
            selectedHerbs.Remove(herb);

        if (!herbQuantities.ContainsKey(herb))
            herbQuantities.Add(herb, 0);

        herbQuantities[herb]++;

        UpdateLeftHerbItem(herb);
        RefreshSelectedHerbs();

        Debug.Log("Bỏ khỏi gói thuốc: " + herb.herbName);
    }

    private void RefreshSelectedHerbs()
    {
        if (selectedHerbRoot == null)
        {
            Debug.LogWarning("Chưa kéo SelectedHerbRoot.");
            return;
        }

        if (selectedHerbItemPrefab == null)
        {
            Debug.LogWarning("Chưa kéo SelectedHerbItemUI prefab.");
            return;
        }

        ClearChildren(selectedHerbRoot);

        foreach (KeyValuePair<HerbData, int> pair in selectedHerbs)
        {
            HerbData herb = pair.Key;
            int quantity = pair.Value;

            if (herb == null || quantity <= 0)
                continue;

            SelectedHerbItemUI item = Instantiate(selectedHerbItemPrefab, selectedHerbRoot);
            item.Setup(herb, quantity, OnClickSelectedHerb);
        }
    }

    private void ConfirmPrescription()
    {
        if (selectedHerbs.Count == 0)
        {
            Debug.LogWarning("Chưa chọn vị thuốc nào.");
            return;
        }

        Debug.Log("===== KÊ ĐƠN =====");

        foreach (KeyValuePair<HerbData, int> pair in selectedHerbs)
        {
            HerbData herb = pair.Key;
            int quantity = pair.Value;

            if (herb == null)
                continue;

            Debug.Log("- " + herb.herbName + " x" + quantity);
        }

        Debug.Log("Đã kê đơn xong.");

        Hide();
    }

    private void UpdateLeftHerbItem(HerbData herb)
    {
        if (herb == null)
            return;

        if (!leftHerbItems.ContainsKey(herb))
            return;

        if (!herbQuantities.ContainsKey(herb))
            return;

        leftHerbItems[herb].UpdateQuantity(herbQuantities[herb]);
    }

    private void ClearChildren(Transform parent)
    {
        if (parent == null)
            return;

        for (int i = parent.childCount - 1; i >= 0; i--)
        {
            Destroy(parent.GetChild(i).gameObject);
        }
    }
}