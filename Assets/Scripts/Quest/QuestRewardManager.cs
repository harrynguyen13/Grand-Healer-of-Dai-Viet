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

        List<RewardType> rewardPair = GetRandomRewardPair();
        List<string> rewardTexts = new List<string>();
        int successCount = 0;

        for (int i = 0; i < rewardPair.Count; i++)
        {
            if (TryGiveReward(rewardPair[i], stage, out string rewardText))
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

                if (TryGiveReward(fallbackRewards[i], stage, out string rewardText))
                {
                    rewardTexts.Add(rewardText);
                    successCount++;
                }
            }
        }

        if (successCount <= 0)
        {
            Debug.LogWarning("Không phát được phần thưởng nhiệm vụ: " + quest.Title);
            return "";
        }

        PlayerPrefs.SetInt(claimedKey, 1);
        PlayerPrefs.Save();

        LastRewardMessage = string.Join(", ", rewardTexts);

        Debug.Log("Hoàn thành nhiệm vụ: " + quest.Title);
        Debug.Log("Phần thưởng nhận được: " + LastRewardMessage);

        return LastRewardMessage;
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

    private bool TryGiveReward(RewardType rewardType, int stage, out string rewardText)
    {
        rewardText = "";

        if (rewardType == RewardType.Money)
        {
            int money = GetRewardMoney(stage);

            if (GiveMoneyReward(money))
            {
                rewardText = "+" + money + " tiền";
                return true;
            }

            return false;
        }

        if (rewardType == RewardType.Herb)
        {
            HerbData herb = GetRandomRewardHerb(stage);

            if (herb == null)
                return false;

            int amount = GetRewardHerbAmount(stage);

            if (GiveHerbReward(herb, amount))
            {
                rewardText = "+" + amount + " " + herb.herbName;
                return true;
            }

            return false;
        }

        if (rewardType == RewardType.Reputation)
        {
            int reputation = GetRewardReputation(stage);

            if (GiveReputationReward(reputation))
            {
                rewardText = "+" + reputation + " tín nhiệm";
                return true;
            }

            return false;
        }

        return false;
    }

    private bool GiveMoneyReward(int amount)
    {
        if (PlayerEconomy.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy PlayerEconomy để cộng tiền thưởng nhiệm vụ.");
            return false;
        }

        PlayerEconomy.Instance.AddMoney(amount);
        return true;
    }

    private bool GiveReputationReward(int amount)
    {
        if (PlayerEconomy.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy PlayerEconomy để cộng tín nhiệm thưởng nhiệm vụ.");
            return false;
        }

        PlayerEconomy.Instance.AddReputation(amount);
        return true;
    }

    private bool GiveHerbReward(HerbData herb, int amount)
    {
        if (HerbInventory.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy HerbInventory để cộng dược liệu thưởng nhiệm vụ.");
            return false;
        }

        if (herb == null || amount <= 0)
            return false;

        HerbInventory.Instance.AddHerb(herb, amount);
        return true;
    }

    private HerbData GetRandomRewardHerb(int stage)
    {
        if (medicalDatabase == null)
        {
            Debug.LogWarning("QuestRewardManager chưa kéo MedicalDatabase.");
            return null;
        }

        int unlockLevel = Mathf.Clamp(stage, 1, 5);
        List<HerbData> unlockedHerbs = medicalDatabase.GetUnlockedHerbs(unlockLevel);

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