using System.Collections;
using System.Collections.Generic;
using System.Text;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class HerbGardenHarvestAllController : MonoBehaviour
{
    [Header("Danh sách các ô đất trong vườn")]
    [SerializeField] private List<HerbGardenPlot> gardenPlots = new List<HerbGardenPlot>();

    [Header("Icon thu hoạch toàn bộ")]
    [SerializeField] private GameObject harvestAllIcon;

    [Header("Collider để click icon")]
    [SerializeField] private Collider2D harvestAllClickCollider;

    [Header("Text tổng bay lên")]
    [SerializeField] private TMP_Text harvestSummaryText;
    [SerializeField] private float summaryDuration = 1.5f;
    [SerializeField] private float summaryMoveUpDistance = 80f;

    [Header("Tự tìm các ô đất con")]
    [SerializeField] private bool autoFindPlotsInChildren = true;

    private Camera mainCamera;
    private Coroutine summaryCoroutine;
    private RectTransform summaryRectTransform;
    private CanvasGroup summaryCanvasGroup;
    private Vector2 summaryStartPosition;

    private void Awake()
    {
        mainCamera = Camera.main;

        if (autoFindPlotsInChildren && gardenPlots.Count == 0)
        {
            FindPlotsInChildren();
        }

        if (harvestAllIcon != null && harvestAllClickCollider == null)
        {
            harvestAllClickCollider = harvestAllIcon.GetComponent<Collider2D>();
        }

        SetupSummaryText();
        SetHarvestIcon(false);
    }

    private void Update()
    {
        UpdateHarvestIconState();
        HandleHarvestIconClick();
    }

    private void FindPlotsInChildren()
    {
        gardenPlots.Clear();

        HerbGardenPlot[] plots = GetComponentsInChildren<HerbGardenPlot>(true);

        for (int i = 0; i < plots.Length; i++)
        {
            if (plots[i] != null)
                gardenPlots.Add(plots[i]);
        }

        gardenPlots.Sort(ComparePlotName);
    }

    private int ComparePlotName(HerbGardenPlot a, HerbGardenPlot b)
    {
        if (a == null && b == null)
            return 0;

        if (a == null)
            return 1;

        if (b == null)
            return -1;

        return string.Compare(a.gameObject.name, b.gameObject.name);
    }

    private void SetupSummaryText()
    {
        if (harvestSummaryText == null)
            return;

        summaryRectTransform = harvestSummaryText.GetComponent<RectTransform>();
        summaryCanvasGroup = harvestSummaryText.GetComponent<CanvasGroup>();

        if (summaryCanvasGroup == null)
            summaryCanvasGroup = harvestSummaryText.gameObject.AddComponent<CanvasGroup>();

        summaryStartPosition = summaryRectTransform.anchoredPosition;

        summaryCanvasGroup.alpha = 0f;
        harvestSummaryText.gameObject.SetActive(false);
    }

    private void UpdateHarvestIconState()
    {
        bool shouldShow = ShouldShowHarvestAllIcon();
        SetHarvestIcon(shouldShow);
    }

    private bool ShouldShowHarvestAllIcon()
    {
        int plantedCount = 0;
        int readyCount = 0;
        int growingCount = 0;

        for (int i = 0; i < gardenPlots.Count; i++)
        {
            HerbGardenPlot plot = gardenPlots[i];

            if (plot == null)
                continue;

            if (!plot.IsUnlocked)
                continue;

            if (plot.IsGrowing)
            {
                plantedCount++;
                growingCount++;
            }
            else if (plot.IsReadyToHarvest)
            {
                plantedCount++;
                readyCount++;
            }
        }

        return plantedCount > 0 && readyCount > 0 && growingCount == 0;
    }

    private void SetHarvestIcon(bool active)
    {
        if (harvestAllIcon == null)
            return;

        if (harvestAllIcon.activeSelf != active)
            harvestAllIcon.SetActive(active);
    }

    private void HandleHarvestIconClick()
    {
        if (harvestAllIcon == null)
            return;

        if (!harvestAllIcon.activeSelf)
            return;

        if (Mouse.current == null)
            return;

        if (!Mouse.current.leftButton.wasPressedThisFrame)
            return;

        if (EventSystem.current != null && EventSystem.current.IsPointerOverGameObject())
            return;

        if (mainCamera == null)
            mainCamera = Camera.main;

        if (mainCamera == null)
            return;

        if (harvestAllClickCollider == null)
        {
            Debug.LogWarning("Chưa gán Collider2D cho icon thu hoạch toàn bộ.");
            return;
        }

        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        Vector3 mouseWorldPosition = mainCamera.ScreenToWorldPoint(mouseScreenPosition);
        Vector2 mousePoint = new Vector2(mouseWorldPosition.x, mouseWorldPosition.y);

        if (!harvestAllClickCollider.OverlapPoint(mousePoint))
            return;

        HarvestAllReadyPlots();
    }

    private void HarvestAllReadyPlots()
    {
        Dictionary<string, int> totalRewards = new Dictionary<string, int>();
        int harvestedCount = 0;

        for (int i = 0; i < gardenPlots.Count; i++)
        {
            HerbGardenPlot plot = gardenPlots[i];

            if (plot == null)
                continue;

            if (!plot.IsUnlocked)
                continue;

            if (!plot.IsReadyToHarvest)
                continue;

            bool harvested = plot.TryHarvestForSummary(out string herbName, out int amount);

            if (!harvested)
                continue;

            if (string.IsNullOrWhiteSpace(herbName))
                continue;

            if (!totalRewards.ContainsKey(herbName))
            {
                totalRewards.Add(herbName, 0);
            }

            totalRewards[herbName] += amount;
            harvestedCount++;
        }

        if (harvestedCount > 0)
        {
            ShowHarvestSummary(totalRewards);
        }

        Debug.Log("Đã thu hoạch toàn bộ vườn. Số ô đã thu: " + harvestedCount);

        UpdateHarvestIconState();
    }

    private void ShowHarvestSummary(Dictionary<string, int> totalRewards)
    {
        if (harvestSummaryText == null)
        {
            Debug.LogWarning("Chưa kéo Harvest Summary Text.");
            return;
        }

        StringBuilder builder = new StringBuilder();
        builder.AppendLine("Thu hoạch:");

        foreach (KeyValuePair<string, int> reward in totalRewards)
        {
            builder.AppendLine(reward.Key + " +" + reward.Value);
        }

        harvestSummaryText.text = builder.ToString();

        if (summaryCoroutine != null)
            StopCoroutine(summaryCoroutine);

        summaryCoroutine = StartCoroutine(PlaySummaryFloatingAnimation());
    }

    private IEnumerator PlaySummaryFloatingAnimation()
    {
        if (summaryRectTransform == null || summaryCanvasGroup == null)
            yield break;

        harvestSummaryText.gameObject.SetActive(true);

        summaryRectTransform.anchoredPosition = summaryStartPosition;
        summaryCanvasGroup.alpha = 1f;

        float timer = 0f;

        while (timer < summaryDuration)
        {
            timer += Time.unscaledDeltaTime;

            float t = timer / summaryDuration;

            Vector2 targetPosition =
                summaryStartPosition + new Vector2(0f, summaryMoveUpDistance);

            summaryRectTransform.anchoredPosition =
                Vector2.Lerp(summaryStartPosition, targetPosition, t);

            summaryCanvasGroup.alpha = Mathf.Lerp(1f, 0f, t);

            yield return null;
        }

        summaryCanvasGroup.alpha = 0f;
        summaryRectTransform.anchoredPosition = summaryStartPosition;
        harvestSummaryText.gameObject.SetActive(false);

        summaryCoroutine = null;
    }

    public void RefreshPlotsNow()
    {
        FindPlotsInChildren();
        UpdateHarvestIconState();
    }
}