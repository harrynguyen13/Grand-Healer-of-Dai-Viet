using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestRuntimeManager : MonoBehaviour
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

        if (!IsOfficialQuestCompleted())
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

        return "Hậu truyện";
    }

    private List<QuestDefinition> GetActiveQuests(int stage, int reputation)
    {
        LastRewardMessage = "";

        int savedStage = PlayerPrefs.GetInt(ActiveStageKey, -1);

        if (savedStage != -1 && savedStage != stage)
        {
            RewardCompletedOldStageQuests(savedStage, reputation);

            ClearActiveQuestSlots();
            PlayerPrefs.SetInt(ActiveStageKey, stage);
            PlayerPrefs.Save();
        }
        else if (savedStage != stage)
        {
            ClearActiveQuestSlots();
            PlayerPrefs.SetInt(ActiveStageKey, stage);
            PlayerPrefs.Save();
        }

        List<QuestDefinition> questPool = BuildQuestPool(stage, reputation);

        string[] slotQuestIds = new string[ActiveQuestCount];

        for (int i = 0; i < ActiveQuestCount; i++)
        {
            slotQuestIds[i] = PlayerPrefs.GetString(GetActiveQuestSlotKey(i), "");
        }

        for (int i = 0; i < ActiveQuestCount; i++)
        {
            if (string.IsNullOrEmpty(slotQuestIds[i]))
                continue;

            QuestDefinition currentQuest = FindQuestById(questPool, slotQuestIds[i]);

            if (currentQuest == null)
            {
                slotQuestIds[i] = "";
                continue;
            }

            if (currentQuest.IsCompleted)
            {
                GiveQuestReward(currentQuest, stage);

                Debug.Log("Nhiệm vụ đã hoàn thành, thay nhiệm vụ mới ở slot "
                    + (i + 1)
                    + ": "
                    + currentQuest.Title);

                slotQuestIds[i] = "";
            }
        }

        for (int i = 0; i < ActiveQuestCount; i++)
        {
            if (!string.IsNullOrEmpty(slotQuestIds[i]))
                continue;

            List<QuestDefinition> candidates = GetAvailableQuestCandidatesForSlots(
                questPool,
                slotQuestIds
            );

            if (candidates.Count == 0)
                continue;

            int randomIndex = GetRandomIndex(candidates.Count);
            QuestDefinition newQuest = candidates[randomIndex];

            slotQuestIds[i] = newQuest.Id;

            Debug.Log("Random nhiệm vụ mới vào slot "
                + (i + 1)
                + ": "
                + newQuest.Title);
        }

        SaveActiveQuestSlots(slotQuestIds);

        List<QuestDefinition> result = new List<QuestDefinition>();

        for (int i = 0; i < ActiveQuestCount; i++)
        {
            QuestDefinition quest = FindQuestById(questPool, slotQuestIds[i]);

            if (quest != null && !quest.IsCompleted)
            {
                result.Add(quest);
            }
        }

        return result;
    }

    private void RewardCompletedOldStageQuests(int oldStage, int currentReputation)
    {
        List<QuestDefinition> oldQuestPool = BuildQuestPool(oldStage, currentReputation);

        for (int i = 0; i < ActiveQuestCount; i++)
        {
            string questId = PlayerPrefs.GetString(GetActiveQuestSlotKey(i), "");

            if (string.IsNullOrEmpty(questId))
                continue;

            QuestDefinition oldQuest = FindQuestById(oldQuestPool, questId);

            if (oldQuest == null)
                continue;

            if (!oldQuest.IsCompleted)
                continue;

            GiveQuestReward(oldQuest, oldStage);
        }
    }

    private void GiveQuestReward(QuestDefinition quest, int stage)
    {
        if (QuestRewardManager.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy QuestRewardManager để phát thưởng nhiệm vụ.");
            return;
        }

        string rewardText = QuestRewardManager.Instance.GiveRewardIfNeeded(quest, stage);

        if (!string.IsNullOrEmpty(rewardText))
            LastRewardMessage = rewardText;
    }

    private int GetRandomIndex(int maxExclusive)
    {
        if (maxExclusive <= 0)
            return 0;

        System.Random random = new System.Random(Guid.NewGuid().GetHashCode());
        return random.Next(0, maxExclusive);
    }

    private List<QuestDefinition> GetAvailableQuestCandidatesForSlots(
        List<QuestDefinition> questPool,
        string[] slotQuestIds
    )
    {
        List<QuestDefinition> candidates = new List<QuestDefinition>();

        for (int i = 0; i < questPool.Count; i++)
        {
            QuestDefinition quest = questPool[i];

            if (quest == null)
                continue;

            if (quest.IsCompleted)
                continue;

            if (IsQuestAlreadyInSlots(quest.Id, slotQuestIds))
                continue;

            candidates.Add(quest);
        }

        return candidates;
    }

    private bool IsQuestAlreadyInSlots(string questId, string[] slotQuestIds)
    {
        if (string.IsNullOrEmpty(questId))
            return false;

        for (int i = 0; i < slotQuestIds.Length; i++)
        {
            if (slotQuestIds[i] == questId)
                return true;
        }

        return false;
    }

    private void SaveActiveQuestSlots(string[] slotQuestIds)
    {
        for (int i = 0; i < ActiveQuestCount; i++)
        {
            string key = GetActiveQuestSlotKey(i);

            if (i < slotQuestIds.Length && !string.IsNullOrEmpty(slotQuestIds[i]))
            {
                PlayerPrefs.SetString(key, slotQuestIds[i]);
            }
            else
            {
                PlayerPrefs.DeleteKey(key);
            }
        }

        PlayerPrefs.Save();
    }

    private void ClearActiveQuestSlots()
    {
        for (int i = 0; i < ActiveQuestCount; i++)
        {
            PlayerPrefs.DeleteKey(GetActiveQuestSlotKey(i));
        }

        PlayerPrefs.Save();
    }

    private string GetActiveQuestSlotKey(int index)
    {
        return ActiveQuestKeyPrefix + index;
    }

    private List<QuestDefinition> BuildQuestPool(int stage, int reputation)
    {
        List<QuestDefinition> quests = new List<QuestDefinition>();

        if (stage == 1)
        {
            AddQuest(quests, "S1_Cure_1", "Chữa khỏi 1 ca bệnh cho dân làng", GetCorrectTreatmentCount, 1);
            AddQuest(quests, "S1_Cure_3", "Chữa khỏi 3 ca bệnh cho dân làng", GetCorrectTreatmentCount, 3);

            AddQuest(quests, "S1_Level1_1", "Chữa khỏi 1 ca bệnh nhẹ cấp 1", () => GetLevelCuredValue(1), 1);
            AddQuest(quests, "S1_Level1_2", "Chữa khỏi 2 ca bệnh nhẹ cấp 1", () => GetLevelCuredValue(1), 2);

            AddQuest(quests, "S1_AchNghich", "Chữa khỏi bệnh Ách nghịch 1 lần", () => GetDiseaseCuredValue("AchNghichAnNac"), 1);

            AddQuest(quests, "S1_Gather_1", "Thu thập 1 dược liệu trong vườn nhà", GetGatheredTotalValue, 1);
            AddQuest(quests, "S1_Gather_3", "Thu thập 3 dược liệu phổ thông trong vườn", GetGatheredTotalValue, 3);
            AddQuest(quests, "S1_Gather_5", "Thu thập 5 dược liệu phổ thông trong vườn", GetGatheredTotalValue, 5);

            AddQuest(quests, "S1_BacHa_2", "Thu thập 2 lần vị Bạc hà trong vườn", () => GetGatheredHerbValue("Bạc hà"), 2);
            AddQuest(quests, "S1_SinhKhuong_2", "Thu thập 3 lần vị Sinh khương trong vườn", () => GetGatheredHerbValue("Sinh khương"), 2);
            AddQuest(quests, "S1_TiaTo_2", "Thu thập 1 lần vị Tía tô trong vườn", () => GetGatheredHerbValue("Tía tô"), 1);

            AddQuest(quests, "S1_Buy_1", "Mua 1 dược liệu bất kỳ từ thương nhân", GetBoughtTotalValue, 1);
            AddQuest(quests, "S1_Buy_3", "Mua 3 dược liệu bất kỳ từ thương nhân", GetBoughtTotalValue, 3);

            AddQuest(quests, "S1_Rank_100", "Đạt 100 tín nhiệm để lên cấp Lương Y", () => reputation, ySinhTarget);
        }
        else if (stage == 2)
        {
            AddQuest(quests, "S2_Cure_5", "Chữa khỏi 5 ca bệnh cho dân làng", GetCorrectTreatmentCount, 5);
            AddQuest(quests, "S2_Cure_7", "Chữa khỏi 10 ca bệnh cho dân làng", GetCorrectTreatmentCount, 10);

            AddQuest(quests, "S2_Level2_1", "Chữa khỏi 1 ca bệnh cấp 2", () => GetLevelCuredValue(2), 1);
            AddQuest(quests, "S2_Level2_2", "Chữa khỏi 2 ca bệnh cấp 2", () => GetLevelCuredValue(2), 2);

            AddQuest(quests, "S2_KhaiThau", "Chữa khỏi bệnh Khái thấu phong nhiệt 1 lần", () => GetDiseaseCuredValue("KhaiThauPhongNhiet"), 1);

            AddQuest(quests, "S2_Gather_8", "Thu thập 8 lần dược liệu trong vườn", GetGatheredTotalValue, 8);
            AddQuest(quests, "S2_Gather_12", "Thu thập 12 lần dược liệu phổ thông trong vườn", GetGatheredTotalValue, 12);

            AddQuest(quests, "S2_KinhGioi_3", "Thu thập 3 lần vị Kinh giới trong vườn", () => GetGatheredHerbValue("Kinh giới"), 3);
            AddQuest(quests, "S2_CamThao_3", "Thu thập 3 lần vị Cam thảo trong vườn", () => GetGatheredHerbValue("Cam thảo"), 3);
            AddQuest(quests, "S2_BacHa_5", "Thu thập 5 lần vị Bạc hà trong vườn", () => GetGatheredHerbValue("Bạc hà"), 5);

            AddQuest(quests, "S2_Buy_5", "Mua 5 dược liệu bất kỳ từ thương nhân", GetBoughtTotalValue, 5);
            AddQuest(quests, "S2_Buy_8", "Mua 12 dược liệu bất kỳ từ thương nhân", GetBoughtTotalValue, 12);

            AddQuest(quests, "S2_Rank_200", "Đạt 200 tín nhiệm để lên cấp Đại Phu", () => reputation, luongYTarget);
        }
        else if (stage == 3)
        {
            AddQuest(quests, "S3_Cure_10", "Chữa khỏi 10 ca bệnh cho dân làng", GetCorrectTreatmentCount, 10);
            AddQuest(quests, "S3_Cure_12", "Chữa khỏi 15 ca bệnh cho dân làng", GetCorrectTreatmentCount, 15);

            AddQuest(quests, "S3_Level3_2", "Chữa khỏi 2 ca bệnh cấp 3", () => GetLevelCuredValue(3), 2);
            AddQuest(quests, "S3_Level3_3", "Chữa khỏi 3 ca bệnh cấp 3", () => GetLevelCuredValue(3), 3);

            AddQuest(quests, "S3_TamHoa", "Chữa khỏi bệnh Tâm hỏa vượng 1 lần", () => GetDiseaseCuredValue("TamHoaVuong"), 1);

            AddQuest(quests, "S3_Gather_15", "Thu thập 15 lần dược liệu trong vườn", GetGatheredTotalValue, 15);
            AddQuest(quests, "S3_Gather_20", "Thu thập 20 lần dược liệu trong vườn", GetGatheredTotalValue, 20);

            AddQuest(quests, "S3_TranBi_4", "Thu thập 4 lần vị Trần bì trong vườn", () => GetGatheredHerbValue("Trần bì"), 4);
            AddQuest(quests, "S3_BachTruat_4", "Thu thập 4 lần vị Bạch truật trong vườn", () => GetGatheredHerbValue("Bạch truật"), 4);
            AddQuest(quests, "S3_CamThao_6", "Thu thập 6 lần vị Cam thảo trong vườn", () => GetGatheredHerbValue("Cam thảo"), 6);

            AddQuest(quests, "S3_Buy_10", "Mua 10 dược liệu bất kỳ từ thương nhân", GetBoughtTotalValue, 10);
            AddQuest(quests, "S3_Buy_12", "Mua 12 dược liệu bất kỳ từ thương nhân", GetBoughtTotalValue, 12);

            AddQuest(quests, "S3_Rank_300", "Đạt 300 tín nhiệm để lên cấp Danh Y", () => reputation, daiPhuTarget);
        }
        else if (stage == 4)
        {
            AddQuest(quests, "S4_Cure_15", "Chữa khỏi 15 ca bệnh cho dân làng", GetCorrectTreatmentCount, 15);
            AddQuest(quests, "S4_Cure_20", "Chữa khỏi 20 ca bệnh cho dân làng", GetCorrectTreatmentCount, 20);

            AddQuest(quests, "S4_Level4_2", "Chữa khỏi 2 ca bệnh nặng cấp 4", () => GetLevelCuredValue(4), 2);
            AddQuest(quests, "S4_Level4_3", "Chữa khỏi 3 ca bệnh nặng cấp 4", () => GetLevelCuredValue(4), 3);

            AddQuest(quests, "S4_ThanDuongHu", "Chữa khỏi bệnh Thận dương hư 1 lần", () => GetDiseaseCuredValue("ThanDuongHu"), 1);

            AddQuest(quests, "S4_Gather_20", "Thu thập 20 lần dược liệu  trong vườn", GetGatheredTotalValue, 20);
            AddQuest(quests, "S4_Gather_25", "Thu thập 25 lần dược liệu  trong vườn", GetGatheredTotalValue, 25);

            AddQuest(quests, "S4_Buy_12", "Mua 12 dược liệu bất kỳ từ thương nhân", GetBoughtTotalValue, 12);
            AddQuest(quests, "S4_Buy_15", "Mua 15 dược liệu bất kỳ từ thương nhân", GetBoughtTotalValue, 15);

            AddQuest(quests, "S4_TamThat_2", "Mua 2 vị Tam thất từ thương nhân", () => GetBoughtHerbValue("Tam thất"), 2);
            AddQuest(quests, "S4_NhucQue_1", "Mua 1 vị Nhục quế từ thương nhân", () => GetBoughtHerbValue("Nhục quế"), 1);

            AddQuest(quests, "S4_Rank_500", "Đạt 500 tín nhiệm để thành Lương Y Đại Việt", () => reputation, danhYTarget);
        }
        else if (stage == 5)
        {
            AddQuest(quests, "S5_Official", "Khám và chữa khỏi bệnh cho quan phủ", () => IsOfficialQuestCompleted() ? 1 : 0, 1);
            AddQuest(quests, "S5_ThatDiet", "Giúp Quan phủ khỏe lại", () => GetDiseaseCuredValue("ThatDietTrungDocDich"), 1);

            AddQuest(quests, "S5_Gather_25", "Thu thập 25 lần dược liệu để chuẩn bị ca bệnh lớn", GetGatheredTotalValue, 25);
            AddQuest(quests, "S5_Gather_30", "Thu thập 30 lần dược liệu để dự trữ cho phủ huyện", GetGatheredTotalValue, 30);

            AddQuest(quests, "S5_Buy_15", "Mua 15 dược liệu bất kỳ từ thương nhân", GetBoughtTotalValue, 15);
            AddQuest(quests, "S5_Buy_20", "Mua 20 dược liệu bất kỳ từ thương nhân", GetBoughtTotalValue, 20);

            AddQuest(quests, "S5_HungHoang_1", "Mua 1 vị Hùng hoàng từ thương nhân", () => GetBoughtHerbValue("Hùng hoàng"), 1);
            AddQuest(quests, "S5_HoangLien_1", "Mua 1 vị Hoàng liên từ thương nhân", () => GetBoughtHerbValue("Hoàng liên"), 1);
        }
        else
        {
            AddQuest(quests, "E_Cure_30", "Tiếp tục chữa khỏi 30 ca bệnh cho người dân", GetCorrectTreatmentCount, 30);
            AddQuest(quests, "E_Level4_5", "Chữa thêm 5 ca bệnh nặng cấp 4", () => GetLevelCuredValue(4), 5);
            AddQuest(quests, "E_Gather_50", "Tích lũy 50 lần dược liệu trong vườn", GetGatheredTotalValue, 50);
            AddQuest(quests, "E_Buy_30", "Mua thêm 30 dược liệu từ thương nhân", GetBoughtTotalValue, 30);
        }

        return quests;
    }

    private void AddQuest(
        List<QuestDefinition> quests,
        string id,
        string title,
        Func<int> getCurrentValue,
        int target
    )
    {
        quests.Add(new QuestDefinition(id, title, getCurrentValue, target));
    }

    private QuestDefinition FindQuestById(List<QuestDefinition> quests, string id)
    {
        if (string.IsNullOrEmpty(id))
            return null;

        for (int i = 0; i < quests.Count; i++)
        {
            if (quests[i] != null && quests[i].Id == id)
                return quests[i];
        }

        return null;
    }

    public void CompleteOfficialQuest()
    {
        if (QuestProgressManager.Instance != null)
        {
            QuestProgressManager.Instance.CompleteOfficialQuest();
        }
        else
        {
            PlayerPrefs.SetInt("OfficialQuestCompleted", 1);
            PlayerPrefs.Save();
        }
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

    private int GetCorrectTreatmentCount()
    {
        if (QuestProgressManager.Instance != null)
            return QuestProgressManager.Instance.GetCorrectTreatmentCount();

        return 0;
    }

    private int GetDiseaseCuredValue(string diseaseAssetName)
    {
        if (QuestProgressManager.Instance != null)
            return QuestProgressManager.Instance.GetDiseaseCuredCount(diseaseAssetName);

        return 0;
    }

    private int GetLevelCuredValue(int diseaseLevel)
    {
        if (QuestProgressManager.Instance != null)
            return QuestProgressManager.Instance.GetLevelCuredCount(diseaseLevel);

        return 0;
    }

    private int GetGatheredTotalValue()
    {
        if (QuestProgressManager.Instance != null)
            return QuestProgressManager.Instance.GetGatheredHerbTotal();

        return 0;
    }

    private int GetBoughtTotalValue()
    {
        if (QuestProgressManager.Instance != null)
            return QuestProgressManager.Instance.GetBoughtHerbTotal();

        return 0;
    }

    private int GetGatheredHerbValue(string herbName)
    {
        if (QuestProgressManager.Instance != null)
            return QuestProgressManager.Instance.GetGatheredHerbCount(herbName);

        return 0;
    }

    private int GetBoughtHerbValue(string herbName)
    {
        if (QuestProgressManager.Instance != null)
            return QuestProgressManager.Instance.GetBoughtHerbCount(herbName);

        return 0;
    }

    private bool IsOfficialQuestCompleted()
    {
        if (QuestProgressManager.Instance != null)
            return QuestProgressManager.Instance.IsOfficialQuestCompleted();

        return PlayerPrefs.GetInt("OfficialQuestCompleted", 0) == 1;
    }

    private int GetReputation()
    {
        if (PlayerEconomy.Instance != null)
            return PlayerEconomy.Instance.Reputation;

        return 0;
    }
}