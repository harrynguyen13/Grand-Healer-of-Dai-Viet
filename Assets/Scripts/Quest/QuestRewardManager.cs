using System;
using System.Collections.Generic;
using UnityEngine;

public class QuestRewardManager : MonoBehaviour
{
    public static QuestRewardManager Instance { get; private set; }

    [Header("Database dược liệu")]
    [SerializeField] private MedicalDatabase medicalDatabase;

    [Header("Giữ object này qua scene khác")]
    [SerializeField] private bool dontDestroyOnLoad = true;

    private const string QuestRewardRunIdKey = "QuestPanel_RewardRunId";
    private const string QuestRewardClaimedKeyPrefix = "QuestPanel_RewardClaimed_";

    private enum RewardType
    {
        Money,
        Herb,
        Reputation
    }

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

    public string GiveRewardIfNeeded(QuestDefinition quest, int stage)
    {
        LastRewardMessage = "";

        if (quest == null)
            return "";

        string claimedKey = GetQuestRewardClaimedKey(quest.Id);

        if (PlayerPrefs.GetInt(claimedKey, 0) == 1)
            return "";

        if (MailboxManager.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy MailboxManager để gửi thưởng nhiệm vụ.");
            return "";
        }

        List<RewardType> rewardPair = GetRandomRewardPair();
        List<string> rewardTexts = new List<string>();

        int moneyReward = 0;
        int reputationReward = 0;
        List<MailHerbReward> herbRewards = new List<MailHerbReward>();

        int successCount = 0;

        for (int i = 0; i < rewardPair.Count; i++)
        {
            if (TryPrepareReward(
                rewardPair[i],
                stage,
                ref moneyReward,
                ref reputationReward,
                herbRewards,
                out string rewardText
            ))
            {
                rewardTexts.Add(rewardText);
                successCount++;
            }
        }

        if (successCount < 2)
        {
            List<RewardType> fallbackRewards = GetFallbackRewards(rewardPair);

            for (int i = 0; i < fallbackRewards.Count; i++)
            {
                if (successCount >= 2)
                    break;

                if (TryPrepareReward(
                    fallbackRewards[i],
                    stage,
                    ref moneyReward,
                    ref reputationReward,
                    herbRewards,
                    out string rewardText
                ))
                {
                    rewardTexts.Add(rewardText);
                    successCount++;
                }
            }
        }

        if (successCount <= 0)
        {
            Debug.LogWarning("Không tạo được phần thưởng nhiệm vụ: " + quest.Title);
            return "";
        }

        LastRewardMessage = string.Join(", ", rewardTexts);

        MailboxManager.Instance.AddQuestRewardMail(
            quest.Title,
            moneyReward,
            reputationReward,
            herbRewards
        );

        PlayerPrefs.SetInt(claimedKey, 1);
        PlayerPrefs.Save();

        Debug.Log("Hoàn thành nhiệm vụ: " + quest.Title);
        Debug.Log("Phần thưởng đã gửi vào hòm thư: " + LastRewardMessage);

        return LastRewardMessage;
    }

    private bool TryPrepareReward(
        RewardType rewardType,
        int stage,
        ref int moneyReward,
        ref int reputationReward,
        List<MailHerbReward> herbRewards,
        out string rewardText
    )
    {
        rewardText = "";

        if (rewardType == RewardType.Money)
        {
            int money = GetRewardMoney(stage);
            moneyReward += money;

            rewardText = "+" + money + " tiền";
            return true;
        }

        if (rewardType == RewardType.Herb)
        {
            HerbData herb = GetRandomRewardHerb(stage);

            if (herb == null)
                return false;

            int amount = GetRewardHerbAmount(stage);

            MailHerbReward herbReward = new MailHerbReward();
            herbReward.herbName = herb.herbName;
            herbReward.amount = amount;

            herbRewards.Add(herbReward);

            rewardText = "+" + amount + " " + herb.herbName;
            return true;
        }

        if (rewardType == RewardType.Reputation)
        {
            int reputation = GetRewardReputation(stage);
            reputationReward += reputation;

            rewardText = "+" + reputation + " tín nhiệm";
            return true;
        }

        return false;
    }

    private List<RewardType> GetRandomRewardPair()
    {
        int randomPair = UnityEngine.Random.Range(0, 3);

        if (randomPair == 0)
        {
            return new List<RewardType>
            {
                RewardType.Money,
                RewardType.Herb
            };
        }

        if (randomPair == 1)
        {
            return new List<RewardType>
            {
                RewardType.Money,
                RewardType.Reputation
            };
        }

        return new List<RewardType>
        {
            RewardType.Herb,
            RewardType.Reputation
        };
    }

    private List<RewardType> GetFallbackRewards(List<RewardType> alreadyTried)
    {
        List<RewardType> fallbackRewards = new List<RewardType>
        {
            RewardType.Money,
            RewardType.Herb,
            RewardType.Reputation
        };

        for (int i = fallbackRewards.Count - 1; i >= 0; i--)
        {
            if (alreadyTried.Contains(fallbackRewards[i]))
                fallbackRewards.RemoveAt(i);
        }

        return fallbackRewards;
    }

    private HerbData GetRandomRewardHerb(int stage)
    {
        if (medicalDatabase == null)
        {
            Debug.LogWarning("QuestRewardManager chưa kéo MedicalDatabase.");
            return null;
        }

        int unlockLevel = Mathf.Clamp(stage, 1, 5);
        List<HerbData> unlockedHerbs = medicalDatabase.GetUnlockedHerbs();

        if (unlockedHerbs == null || unlockedHerbs.Count == 0)
            return null;

        List<HerbData> validHerbs = new List<HerbData>();

        for (int i = 0; i < unlockedHerbs.Count; i++)
        {
            if (unlockedHerbs[i] != null)
                validHerbs.Add(unlockedHerbs[i]);
        }

        if (validHerbs.Count == 0)
            return null;

        int randomIndex = UnityEngine.Random.Range(0, validHerbs.Count);
        return validHerbs[randomIndex];
    }

    private int GetRewardMoney(int stage)
    {
        if (stage <= 1)
            return UnityEngine.Random.Range(25, 41);

        if (stage == 2)
            return UnityEngine.Random.Range(50, 81);

        if (stage == 3)
            return UnityEngine.Random.Range(90, 141);

        if (stage == 4)
            return UnityEngine.Random.Range(150, 231);

        if (stage == 5)
            return UnityEngine.Random.Range(250, 401);

        return UnityEngine.Random.Range(180, 301);
    }

    private int GetRewardReputation(int stage)
    {
        if (stage <= 1)
            return UnityEngine.Random.Range(5, 9);

        if (stage == 2)
            return UnityEngine.Random.Range(8, 13);

        if (stage == 3)
            return UnityEngine.Random.Range(12, 19);

        if (stage == 4)
            return UnityEngine.Random.Range(18, 27);

        if (stage == 5)
            return UnityEngine.Random.Range(30, 46);

        return UnityEngine.Random.Range(15, 26);
    }

    private int GetRewardHerbAmount(int stage)
    {
        if (stage <= 1)
            return 1;

        if (stage == 2)
            return UnityEngine.Random.Range(1, 3);

        if (stage == 3)
            return 2;

        if (stage == 4)
            return UnityEngine.Random.Range(2, 4);

        if (stage == 5)
            return UnityEngine.Random.Range(3, 5);

        return 2;
    }

    private string GetQuestRewardClaimedKey(string questId)
    {
        return QuestRewardClaimedKeyPrefix + GetRewardRunId() + "_" + questId;
    }

    private string GetRewardRunId()
    {
        string runId = PlayerPrefs.GetString(QuestRewardRunIdKey, "");

        if (string.IsNullOrEmpty(runId))
        {
            runId = Guid.NewGuid().ToString();
            PlayerPrefs.SetString(QuestRewardRunIdKey, runId);
            PlayerPrefs.Save();
        }

        return runId;
    }

    public void ResetRewardForNewGame()
    {
        PlayerPrefs.DeleteKey(QuestRewardRunIdKey);
        PlayerPrefs.Save();

        LastRewardMessage = "";

        Debug.Log("Đã reset mã phần thưởng nhiệm vụ cho game mới.");
    }
}