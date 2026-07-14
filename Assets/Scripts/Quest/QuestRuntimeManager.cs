using System.Collections.Generic;
using UnityEngine;

public partial class QuestRuntimeManager : MonoBehaviour
{
    public static QuestRuntimeManager Instance { get; private set; }

    [Header("Mốc tín nhiệm theo cấp bậc")]
    [SerializeField] private int ySinhTarget = 100;
    [SerializeField] private int luongYTarget = 200;
    [SerializeField] private int daiPhuTarget = 300;
    [SerializeField] private int danhYTarget = 500;

    [Header("Giữ object này qua scene khác")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    private const int ActiveQuestCount = 2;

    private const string ActiveStageKey = "QuestPanel_ActiveStage";
    private const string ActiveQuestKeyPrefix = "QuestPanel_ActiveQuest_";

    public string LastRewardMessage { get; private set; }

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

    public int GetCurrentStage()
    {
        return GetCurrentStage(GetReputation());
    }

    public int GetCurrentStage(int reputation)
    {
        if (reputation < ySinhTarget)
            return 1;

        if (reputation < luongYTarget)
            return 2;

        if (reputation < daiPhuTarget)
            return 3;

        if (reputation < danhYTarget)
            return 4;

        if (!PlayerLevelService.CanBecomeYDaoSuccessor())
            return 5;

        return 6;
    }

    public string GetChapterTitle(int stage)
    {
        if (stage == 1)
            return "Chương 1 - Y Sinh";

        if (stage == 2)
            return "Chương 2 - Lương Y";

        if (stage == 3)
            return "Chương 3 - Đại Phu";

        if (stage == 4)
            return "Chương 4 - Danh Y";

        if (stage == 5)
            return "Chương 5 - Phủ Huyện";

        return "Hậu truyện - Truyền Nhân Y Đạo";
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