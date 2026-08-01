using System;
using System.Collections.Generic;
using UnityEngine;

public partial class QuestRuntimeManager
{
    private const string ActiveQuestStartValueKeyPrefix = "ActiveQuestStartValue_";

    //Dùng cho nhiệm vụ chỉ được hoàn thành 1 lần trong save.
    private const string CompletedOnceQuestKeyPrefix = "Quest_CompletedOnce_";

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
                ClearQuestStartValue(i);
                continue;
            }

            ApplySavedQuestStartValue(i, currentQuest);

            if (currentQuest.IsCompleted)
            {
                GiveQuestReward(currentQuest, stage);

                Debug.Log("Nhiệm vụ đã hoàn thành, thay nhiệm vụ mới ở slot "
                    + (i + 1)
                    + ": "
                    + currentQuest.Title);

                slotQuestIds[i] = "";
                ClearQuestStartValue(i);
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
            SaveQuestStartValue(i, newQuest);

            Debug.Log("Random nhiệm vụ mới vào slot "
                + (i + 1)
                + ": "
                + newQuest.Title
                + " | Mốc bắt đầu: "
                + newQuest.StartValue);
        }

        SaveActiveQuestSlots(slotQuestIds);

        TrySendQuanHuyenQuestMailIfActive(slotQuestIds);

        List<QuestDefinition> result = new List<QuestDefinition>();

        for (int i = 0; i < ActiveQuestCount; i++)
        {
            QuestDefinition quest = FindQuestById(questPool, slotQuestIds[i]);

            if (quest != null)
            {
                ApplySavedQuestStartValue(i, quest);

                if (!quest.IsCompleted)
                {
                    result.Add(quest);
                }
            }
        }

        return result;
    }

    private void TrySendQuanHuyenQuestMailIfActive(string[] slotQuestIds)
    {
        if (slotQuestIds == null)
            return;

        if (IsOfficialQuestCompleted())
            return;

        bool hasQuanHuyenQuest = false;

        for (int i = 0; i < slotQuestIds.Length; i++)
        {
            if (slotQuestIds[i] == "S5_Official")
            {
                hasQuanHuyenQuest = true;
                break;
            }
        }

        if (!hasQuanHuyenQuest)
            return;

        if (SpecialQuestMailBridge.Instance == null)
        {
            Debug.LogWarning("QuestRuntimeManager: Chưa có SpecialQuestMailBridge để gửi thư Quan Huyện.");
            return;
        }

        SpecialQuestMailBridge.Instance.SendQuanHuyenQuestMailOnce();
    }

    private void RewardCompletedOldStageQuests(int oldStage, int currentReputation)
    {
        List<QuestDefinition> oldQuestPool = BuildQuestPool(oldStage, currentReputation);

        for (int i = 0; i < ActiveQuestCount; i++)
        {
            string questId = PlayerPrefs.GetString(GetActiveQuestSlotKey(i), "");

            if (string.IsNullOrEmpty(questId))
                continue;
            
            if (IsRankQuestId(questId))
                continue;

            QuestDefinition oldQuest = FindQuestById(oldQuestPool, questId);

            if (oldQuest == null)
                continue;

            ApplySavedQuestStartValue(i, oldQuest);

            if (!oldQuest.IsCompleted)
                continue;

            GiveQuestReward(oldQuest, oldStage);
        }
    }

    private void GiveQuestReward(QuestDefinition quest, int stage)
    {
        if (quest == null)
            return;

        if (QuestRewardManager.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy QuestRewardManager để phát thưởng nhiệm vụ.");
            return;
        }

        string rewardText = QuestRewardManager.Instance.GiveRewardIfNeeded(quest, stage);

        if (!string.IsNullOrEmpty(rewardText))
            LastRewardMessage = rewardText;


         //Nếu là nhiệm vụ chỉ hoàn thành 1 lần,
         //đánh dấu lại để sau này không random ra nữa.
        if (IsCompletedOnceQuest(quest))
        {
            MarkCompletedOnceQuest(quest.Id);
        }
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

            if (IsQuestAlreadyInSlots(quest.Id, slotQuestIds))
                continue;

             //Chặn nhiệm vụ chỉ hoàn thành 1 lần.
            if (IsCompletedOnceQuest(quest) && HasCompletedOnceQuest(quest.Id))
                continue;

            if (IsNonRepeatableQuestAlreadyCompleted(quest))
                continue;

            candidates.Add(quest);
        }

        return candidates;
    }

    private bool IsNonRepeatableQuestAlreadyCompleted(QuestDefinition quest)
    {
        if (quest == null)
            return false;

        if (quest.Id == "S5_Official")
        {
            return IsOfficialQuestCompleted();
        }

        if (quest.Id == "S5_ThatDiet")
        {
            return quest.RawCurrent >= quest.Target;
        }

        return false;
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
                ClearQuestStartValue(i);
            }
        }

        PlayerPrefs.Save();
    }

    private void ClearActiveQuestSlots()
    {
        for (int i = 0; i < ActiveQuestCount; i++)
        {
            PlayerPrefs.DeleteKey(GetActiveQuestSlotKey(i));
            ClearQuestStartValue(i);
        }

        PlayerPrefs.Save();
    }

    private void SaveQuestStartValue(int slotIndex, QuestDefinition quest)
    {
        if (quest == null)
            return;

        int startValue = quest.RawCurrent;

        PlayerPrefs.SetInt(GetActiveQuestStartValueKey(slotIndex), startValue);
        quest.SetStartValue(startValue);

        PlayerPrefs.Save();
    }

    private void ApplySavedQuestStartValue(int slotIndex, QuestDefinition quest)
    {
        if (quest == null)
            return;

        string key = GetActiveQuestStartValueKey(slotIndex);

        if (!PlayerPrefs.HasKey(key))
        {
            SaveQuestStartValue(slotIndex, quest);
            return;
        }

        int savedStartValue = PlayerPrefs.GetInt(key, quest.RawCurrent);
        quest.SetStartValue(savedStartValue);
    }

    private void ClearQuestStartValue(int slotIndex)
    {
        PlayerPrefs.DeleteKey(GetActiveQuestStartValueKey(slotIndex));
    }

    private string GetActiveQuestSlotKey(int index)
    {
        return ActiveQuestKeyPrefix + index;
    }

    private string GetActiveQuestStartValueKey(int index)
    {
        return ActiveQuestStartValueKeyPrefix + index;
    }

    private bool IsCompletedOnceQuest(QuestDefinition quest)
    {
        if (quest == null)
            return false;

        return IsRankQuestId(quest.Id);
    }

    private bool IsRankQuestId(string questId)
    {
        if (string.IsNullOrEmpty(questId))
            return false;

        return questId.Contains("_Rank_");
    }

    private bool HasCompletedOnceQuest(string questId)
    {
        if (string.IsNullOrEmpty(questId))
            return false;

        return PlayerPrefs.GetInt(GetCompletedOnceQuestKey(questId), 0) == 1;
    }

    private void MarkCompletedOnceQuest(string questId)
    {
        if (string.IsNullOrEmpty(questId))
            return;

        PlayerPrefs.SetInt(GetCompletedOnceQuestKey(questId), 1);
        PlayerPrefs.Save();

        Debug.Log("Đã đánh dấu nhiệm vụ chỉ hoàn thành 1 lần: " + questId);
    }

    private string GetCompletedOnceQuestKey(string questId)
    {
        return CompletedOnceQuestKeyPrefix + questId;
    }
}