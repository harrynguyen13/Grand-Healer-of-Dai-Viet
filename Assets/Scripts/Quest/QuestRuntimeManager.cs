using System.Collections.Generic;
using UnityEngine;

public partial class QuestRuntimeManager : MonoBehaviour
{
    public static QuestRuntimeManager Instance { get; private set; }

    [Header("Giữ object này qua scene khác")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    private const int ActiveQuestCount = 2;

    private const string ActiveStageKey = "QuestPanel_ActiveStage";
    private const string ActiveQuestKeyPrefix = "QuestPanel_ActiveQuest_";

    public string LastRewardMessage { get; private set; }

    private bool isRefreshingQuestState;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(this);
            return;
        }

        Instance = this;

        if (dontDestroyOnLoad)
            DontDestroyOnLoad(gameObject);
    }

    public string GetQuestPanelText()
    {
        int reputation = GetReputation();
        int stage = GetCurrentStage(reputation);
        List<QuestDefinition> activeQuests = GetActiveQuests(stage, reputation);

        string result =
            "<b>" + GetChapterTitle(stage) + "</b>\n\n" +
            "<b>Nhiệm vụ hiện tại</b>\n";

        if (activeQuests.Count == 0)
        {
            result += "Đã hoàn thành toàn bộ nhiệm vụ cấp này.";
            return result;
        }

        for (int i = 0; i < activeQuests.Count; i++)
        {
            QuestDefinition quest = activeQuests[i];

            result +=
                (i + 1) + ". " +
                quest.Title +
                " (" +
                quest.GetProgressText() +
                ")";

            if (i < activeQuests.Count - 1)
                result += "\n\n";
        }

        return result;
    }

    public void RefreshQuestStateNow()
    {
        if (isRefreshingQuestState)
            return;

        isRefreshingQuestState = true;

        try
        {
            int reputation = GetReputation();
            int stage = GetCurrentStage(reputation);

            GetActiveQuests(stage, reputation);
        }
        finally
        {
            isRefreshingQuestState = false;
        }
    }

    public int GetCurrentStage()
    {
        return PlayerLevelService.GetCurrentStage();
    }

    public int GetCurrentStage(int reputation)
    {
        return PlayerLevelService.GetStageByReputation(reputation);
    }

    public string GetChapterTitle(int stage)
    {
        return PlayerLevelService.GetChapterTitle(stage);
    }

    public void ResetQuestRuntimeForNewGame()
    {
        PlayerPrefs.DeleteKey(ActiveStageKey);
        ClearActiveQuestSlots();

        if (QuestRewardManager.Instance != null)
            QuestRewardManager.Instance.ResetRewardForNewGame();

        LastRewardMessage = "";

        PlayerPrefs.Save();

        Debug.Log("Đã reset nhiệm vụ đang nhận.");
    }
}