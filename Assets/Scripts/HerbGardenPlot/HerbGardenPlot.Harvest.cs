using System.Collections;
using UnityEngine;

public partial class HerbGardenPlot
{
    private void HarvestCurrentPlant()
    {
        if (!isUnlocked)
        {
            Debug.Log("Ô đất " + gardenId + " chưa được mở khóa.");
            return;
        }

        if (currentPlant == null)
        {
            Debug.LogWarning("Ô đất đang ready nhưng không có dữ liệu cây.");
            SetEmpty();
            SaveGardenState();
            return;
        }

        if (currentPlant.rewardHerb == null)
        {
            Debug.LogWarning("Cây " + currentPlant.plantName + " chưa kéo Reward Herb.");
            return;
        }

        if (HerbInventory.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy HerbInventory.Instance để cộng dược liệu.");
            return;
        }

        if (string.IsNullOrEmpty(currentPlantBatchId))
            currentPlantBatchId = GetOrCreateOpenBatchId(currentPlant.rewardHerb);

        int amount = Mathf.Max(1, currentPlant.harvestAmount);

        HerbData harvestedHerb = currentPlant.rewardHerb;
        string harvestedBatchId = currentPlantBatchId;
        string harvestedPlantName = currentPlant.plantName;
        string harvestedHerbName = currentPlant.rewardHerb.herbName;

        HerbInventory.Instance.AddHerb(harvestedHerb, amount);

        if (QuestProgressManager.Instance != null)
        {
            QuestProgressManager.Instance.RecordHerbGathered(harvestedHerb, amount);

            QuestProgressManager.Instance.RecordGardenHarvestSessionForHerbBatch(
                harvestedHerb,
                harvestedBatchId
            );
        }

        StartCoroutine(ShowFloatingText(harvestedHerbName, amount));

        Debug.Log(
            "Thu hoạch "
            + harvestedPlantName
            + " -> nhận "
            + harvestedHerbName
            + " +"
            + amount
            + ". Batch: "
            + harvestedBatchId
        );

        SetEmpty();
        SaveGardenState();

        ClearOpenBatchIfNoRemainingPlots(harvestedHerb, harvestedBatchId);
    }

    public bool TryHarvestForSummary(out string herbName, out int amount)
    {
        herbName = "";
        amount = 0;

        if (!isUnlocked)
            return false;

        if (currentState != GardenState.ReadyToHarvest)
            return false;

        if (currentPlant == null)
        {
            Debug.LogWarning("Ô đất đang ready nhưng không có dữ liệu cây.");
            SetEmpty();
            SaveGardenState();
            return false;
        }

        if (currentPlant.rewardHerb == null)
        {
            Debug.LogWarning("Cây " + currentPlant.plantName + " chưa kéo Reward Herb.");
            return false;
        }

        if (HerbInventory.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy HerbInventory.Instance để cộng dược liệu.");
            return false;
        }

        if (string.IsNullOrEmpty(currentPlantBatchId))
            currentPlantBatchId = GetOrCreateOpenBatchId(currentPlant.rewardHerb);

        amount = Mathf.Max(1, currentPlant.harvestAmount);
        herbName = currentPlant.rewardHerb.herbName;

        HerbData harvestedHerb = currentPlant.rewardHerb;
        string harvestedBatchId = currentPlantBatchId;
        string harvestedPlantName = currentPlant.plantName;

        HerbInventory.Instance.AddHerb(harvestedHerb, amount);

        if (QuestProgressManager.Instance != null)
        {
            QuestProgressManager.Instance.RecordHerbGathered(harvestedHerb, amount);

            QuestProgressManager.Instance.RecordGardenHarvestSessionForHerbBatch(
                harvestedHerb,
                harvestedBatchId
            );
        }

        Debug.Log(
            "Thu hoạch "
            + harvestedPlantName
            + " -> nhận "
            + herbName
            + " +"
            + amount
            + ". Batch: "
            + harvestedBatchId
        );

        SetEmpty();
        SaveGardenState();

        ClearOpenBatchIfNoRemainingPlots(harvestedHerb, harvestedBatchId);

        return true;
    }

    private IEnumerator ShowFloatingText(string herbName, int amount)
    {
        if (floatingTextPrefab == null || floatingTextSpawnPoint == null)
            yield break;

        FloatingHarvestText textInstance = Instantiate(
            floatingTextPrefab,
            floatingTextSpawnPoint.position,
            Quaternion.identity
        );

        textInstance.Setup(herbName + " +" + amount);

        yield return null;
    }
}