using TMPro;
using UnityEngine;

public class PlayerRankUI : MonoBehaviour
{
    [SerializeField] private TMP_Text rankText;

    [Header("Test cấp bậc")]
    [SerializeField] private bool useTestReputation = false;
    [SerializeField] private int testReputation = 0;

    private int lastStage = -1;

    private void Start()
    {
        RefreshRankText();
    }

    private void Update()
    {
        int currentStage = GetCurrentStage();

        if (currentStage == lastStage)
            return;

        RefreshRankText();
    }

    private void RefreshRankText()
    {
        if (rankText == null)
            return;

        int stage = GetCurrentStage();

        lastStage = stage;
        rankText.text = PlayerLevelService.GetRankNameByStage(stage);
    }

    private int GetCurrentStage()
    {
        if (useTestReputation)
        {
            return PlayerLevelService.GetStageByReputation(testReputation);
        }

        return PlayerLevelService.GetCurrentStage();
    }
}