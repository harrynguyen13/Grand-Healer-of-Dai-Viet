using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class HerbGardenUnlockZone : MonoBehaviour
{
    [Header("Khu này cần đạt cấp nào để mở")]
    [SerializeField] private int requiredStage = 2;

    [Header("Các ô đất thuộc khu này")]
    [SerializeField] private List<HerbGardenPlot> gardenPlots = new List<HerbGardenPlot>();

    [Header("Overlay khóa phủ xám khu này")]
    [SerializeField] private GameObject lockedVisual;

    [Header("Text báo mở khóa")]
    [SerializeField] private TMP_Text unlockText;

    [Header("Collider chặn click nếu cần")]
    [SerializeField] private Collider2D blockCollider;

    [Header("Tự tìm HerbGardenPlot con")]
    [SerializeField] private bool autoFindPlotsInChildren = true;

    private int lastPlayerUnlockLevel = -999;

    private void Awake()
    {
        if (autoFindPlotsInChildren)
            FindPlotsInChildren();

        RefreshUnlockState();
    }

    private void Start()
    {
        RefreshUnlockState();
    }

    private void Update()
    {
        int currentUnlockLevel = PlayerLevelService.GetCurrentUnlockLevel();

        if (currentUnlockLevel == lastPlayerUnlockLevel)
            return;

        RefreshUnlockState();
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
    }

    public void RefreshUnlockState()
    {
        int currentUnlockLevel = PlayerLevelService.GetCurrentUnlockLevel();
        lastPlayerUnlockLevel = currentUnlockLevel;

        bool isUnlocked = currentUnlockLevel >= requiredStage;

        // Tất cả khu chưa mở đều phủ xám.
        bool shouldShowLockedVisual = !isUnlocked;

        // Nhưng chỉ khu kế tiếp mới hiện chữ.
        // Ví dụ cấp 1 chỉ hiện chữ ở khu cần cấp 2.
        bool shouldShowUnlockText = !isUnlocked && requiredStage == currentUnlockLevel + 1;

        ApplyPlotsUnlockState(isUnlocked);
        ApplyLockedVisual(shouldShowLockedVisual);
        ApplyUnlockText(shouldShowUnlockText);
        ApplyBlockCollider(!isUnlocked);
    }

    private void ApplyPlotsUnlockState(bool isUnlocked)
    {
        for (int i = 0; i < gardenPlots.Count; i++)
        {
            if (gardenPlots[i] == null)
                continue;

            gardenPlots[i].SetPlotUnlocked(isUnlocked);
        }
    }

    private void ApplyLockedVisual(bool shouldShow)
    {
        if (lockedVisual != null)
            lockedVisual.SetActive(shouldShow);
    }

    private void ApplyUnlockText(bool shouldShow)
    {
        if (unlockText == null)
            return;

        unlockText.gameObject.SetActive(shouldShow);

        if (!shouldShow)
            return;

        unlockText.text =
            "Đạt cấp "
            + requiredStage
            + " - "
            + PlayerLevelService.GetRankNameByStage(requiredStage)
            + " để mở khóa";
    }

    private void ApplyBlockCollider(bool shouldBlock)
    {
        if (blockCollider != null)
            blockCollider.enabled = shouldBlock;
    }
}