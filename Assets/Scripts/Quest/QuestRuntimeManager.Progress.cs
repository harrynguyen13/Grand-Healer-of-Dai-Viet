using UnityEngine;

public partial class QuestRuntimeManager
{
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