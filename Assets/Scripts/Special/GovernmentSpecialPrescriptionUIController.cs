using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class GovernmentSpecialPrescriptionUIController : MonoBehaviour
{
    [Header("Root UI")]
    [SerializeField] private GameObject prescriptionPanel;

    [Header("Database")]
    [SerializeField] private MedicalDatabase medicalDatabase;

    [Header("Kho thuốc bên trái")]
    [SerializeField] private Transform herbListContent;
    [SerializeField] private HerbItemUI herbItemPrefab;

    [Header("Gói thuốc bên phải")]
    [SerializeField] private Transform selectedHerbRoot;
    [SerializeField] private SelectedHerbItemUI selectedHerbItemPrefab;

    [Header("Nút kê đơn")]
    [SerializeField] private Button confirmPrescriptionButton;

    [Header("Nút đóng")]
    [SerializeField] private Button closeButton;

    [Header("Số lượng test nếu chưa có HerbInventory")]
    [SerializeField] private int fallbackHerbQuantity = 5;

    private readonly Dictionary<HerbData, int> herbQuantities = new Dictionary<HerbData, int>();
    private readonly Dictionary<HerbData, int> selectedHerbs = new Dictionary<HerbData, int>();
    private readonly Dictionary<HerbData, HerbItemUI> leftHerbItems = new Dictionary<HerbData, HerbItemUI>();

    private SpecialDiseaseCase currentSpecialCase;
    private Action<SpecialPrescriptionEvaluationResult> onPrescriptionFinished;

    private bool isSubmitting;

    private void Awake()
    {
        if (confirmPrescriptionButton != null)
        {
            confirmPrescriptionButton.onClick.RemoveAllListeners();
            confirmPrescriptionButton.onClick.AddListener(ConfirmPrescription);
        }

        if (closeButton != null)
        {
            closeButton.onClick.RemoveAllListeners();
            closeButton.onClick.AddListener(Hide);
        }

        Hide();
    }

    public void Show(
        SpecialDiseaseCase specialCase,
        Action<SpecialPrescriptionEvaluationResult> onFinished
    )
    {
        currentSpecialCase = specialCase;
        onPrescriptionFinished = onFinished;
        isSubmitting = false;

        if (currentSpecialCase == null)
        {
            Debug.LogError("GovernmentSpecialPrescriptionUIController: Chưa có SpecialDiseaseCase.");
            return;
        }

        if (!currentSpecialCase.CanTryTreatment())
        {
            Debug.LogWarning(
                "Chưa thể bốc thuốc đặc biệt."
                + " | IsCured = " + currentSpecialCase.IsCured
                + " | IsFailed = " + currentSpecialCase.IsFailed
                + " | RemainingAttempts = " + currentSpecialCase.RemainingAttempts
            );
            return;
        }

        if (prescriptionPanel != null)
            prescriptionPanel.SetActive(true);

        selectedHerbs.Clear();

        BuildHerbList();
        RefreshSelectedHerbs();

        if (confirmPrescriptionButton != null)
            confirmPrescriptionButton.interactable = true;

        Debug.Log("Đã mở UI bốc thuốc đặc biệt.");
    }

    public void Hide()
    {
        if (prescriptionPanel != null)
            prescriptionPanel.SetActive(false);

        if (HerbRoleTooltipUI.Instance != null)
            HerbRoleTooltipUI.Instance.Hide();
    }

    private void BuildHerbList()
    {
        if (medicalDatabase == null)
        {
            Debug.LogError("GovernmentSpecialPrescriptionUIController chưa có MedicalDatabase.");
            return;
        }

        if (herbListContent == null)
        {
            Debug.LogError("GovernmentSpecialPrescriptionUIController chưa kéo HerbListContent.");
            return;
        }

        ScrollRect scrollRect = herbListContent.GetComponent<ScrollRect>();

        if (scrollRect != null && scrollRect.content != null)
        {
            herbListContent = scrollRect.content;
        }

        if (herbItemPrefab == null)
        {
            Debug.LogError("GovernmentSpecialPrescriptionUIController chưa kéo HerbItemUI prefab.");
            return;
        }

        if (HerbInventory.Instance != null)
        {
            HerbInventory.Instance.RefreshUnlockedHerbsByPlayerLevel(false);
        }

        ClearChildren(herbListContent);

        herbQuantities.Clear();
        leftHerbItems.Clear();

        List<HerbData> unlockedHerbs = medicalDatabase.GetUnlockedHerbs();

        foreach (HerbData herb in unlockedHerbs)
        {
            if (herb == null)
                continue;

            int quantity = GetRealHerbQuantity(herb);

            if (!herbQuantities.ContainsKey(herb))
                herbQuantities.Add(herb, quantity);

            HerbItemUI item = Instantiate(herbItemPrefab, herbListContent);

            item.Setup(herb, quantity, OnClickHerbFromStorage);

            SetupTooltipForItem(item.gameObject, herb);

            if (!leftHerbItems.ContainsKey(herb))
                leftHerbItems.Add(herb, item);
        }

        Debug.Log("Đã tạo danh sách dược liệu đặc biệt theo cấp hiện tại: " + PlayerLevelService.GetCurrentUnlockLevel());
        Debug.Log("Số dược liệu mở khóa: " + unlockedHerbs.Count);
    }

    private int GetRealHerbQuantity(HerbData herb)
    {
        if (herb == null)
            return 0;

        if (HerbInventory.Instance != null)
        {
            return HerbInventory.Instance.GetQuantity(herb);
        }

        Debug.LogWarning("Không có HerbInventory. Dùng số lượng test cho: " + herb.herbName);
        return fallbackHerbQuantity;
    }

    private void OnClickHerbFromStorage(HerbData herb)
    {
        if (isSubmitting)
            return;

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

        Debug.Log("Thêm vào gói thuốc đặc biệt: " + herb.herbName + " x" + selectedHerbs[herb]);
    }

    private void OnClickSelectedHerb(HerbData herb)
    {
        if (isSubmitting)
            return;

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

        Debug.Log("Bỏ khỏi gói thuốc đặc biệt: " + herb.herbName);
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

            SetupTooltipForItem(item.gameObject, herb);
        }
    }

    private void ConfirmPrescription()
    {
        if (isSubmitting)
            return;

        if (currentSpecialCase == null)
        {
            Debug.LogError("Không có ca bệnh đặc biệt hiện tại.");
            return;
        }

        if (!currentSpecialCase.CanTryTreatment())
        {
            Debug.LogWarning(
                "Không còn đủ điều kiện thử chữa bệnh đặc biệt."
                + " | IsCured = " + currentSpecialCase.IsCured
                + " | IsFailed = " + currentSpecialCase.IsFailed
                + " | RemainingAttempts = " + currentSpecialCase.RemainingAttempts
            );
            return;
        }

        if (selectedHerbs.Count == 0)
        {
            Debug.LogWarning("Chưa chọn vị thuốc nào.");
            return;
        }

        isSubmitting = true;

        if (confirmPrescriptionButton != null)
            confirmPrescriptionButton.interactable = false;

        Dictionary<HerbData, int> selectedSnapshot = new Dictionary<HerbData, int>(selectedHerbs);

        if (HerbInventory.Instance != null)
        {
            if (!HerbInventory.Instance.HasEnoughPrescription(selectedSnapshot))
            {
                Debug.LogWarning("Kho thuốc không đủ để kê đơn.");

                isSubmitting = false;

                if (confirmPrescriptionButton != null)
                    confirmPrescriptionButton.interactable = true;

                BuildHerbList();
                RefreshSelectedHerbs();

                return;
            }
        }

        Debug.Log("===== KÊ ĐƠN ĐẶC BIỆT =====");

        foreach (KeyValuePair<HerbData, int> pair in selectedSnapshot)
        {
            HerbData herb = pair.Key;
            int quantity = pair.Value;

            if (herb == null)
                continue;

            Debug.Log("- " + herb.herbName + " x" + quantity);
        }

        SpecialPrescriptionEvaluationResult result =
            SpecialPrescriptionEvaluator.Evaluate(
                currentSpecialCase.SpecialDisease,
                selectedSnapshot
            );

        Debug.Log("Kết quả đánh giá đơn đặc biệt: " + result.isCorrect);
        Debug.Log("Lý do: " + result.message);

        if (HerbInventory.Instance != null)
        {
            bool removed = HerbInventory.Instance.RemovePrescription(selectedSnapshot);

            if (!removed)
            {
                Debug.LogWarning("Không trừ được thuốc trong kho.");

                isSubmitting = false;

                if (confirmPrescriptionButton != null)
                    confirmPrescriptionButton.interactable = true;

                return;
            }
        }

        currentSpecialCase.RegisterTreatmentResult(result.isCorrect);

        if (result.isCorrect)
        {
            SpecialYThuPrescriptionRecordService.SaveCorrectPrescription(selectedSnapshot);
        }

        Hide();

        onPrescriptionFinished?.Invoke(result);
        onPrescriptionFinished = null;

        isSubmitting = false;
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

    private void SetupTooltipForItem(GameObject itemObject, HerbData herb)
    {
        if (itemObject == null)
            return;

        if (herb == null)
            return;

        HerbTooltipTrigger tooltipTrigger = itemObject.GetComponent<HerbTooltipTrigger>();

        if (tooltipTrigger == null)
        {
            tooltipTrigger = itemObject.AddComponent<HerbTooltipTrigger>();
        }

        tooltipTrigger.SetHerb(herb);
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