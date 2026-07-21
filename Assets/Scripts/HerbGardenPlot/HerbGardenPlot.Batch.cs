using System;
using UnityEngine;

public partial class HerbGardenPlot
{
    private string GetOrCreateOpenBatchId(HerbData herb)
    {
        if (herb == null)
            return Guid.NewGuid().ToString("N");

        string key = GetOpenBatchKey(herb);
        string savedBatchId = PlayerPrefs.GetString(key, "");

        if (!string.IsNullOrEmpty(savedBatchId))
            return savedBatchId;

        string existingBatchId = FindExistingActiveBatchIdForHerb(herb);

        if (!string.IsNullOrEmpty(existingBatchId))
        {
            PlayerPrefs.SetString(key, existingBatchId);
            PlayerPrefs.Save();

            return existingBatchId;
        }

        string newBatchId = Guid.NewGuid().ToString("N");

        PlayerPrefs.SetString(key, newBatchId);
        PlayerPrefs.Save();

        Debug.Log("Tạo lượt trồng mới cho " + herb.herbName + ". Batch: " + newBatchId);

        return newBatchId;
    }

    private string FindExistingActiveBatchIdForHerb(HerbData herb)
    {
        if (herb == null)
            return "";

        HerbGardenPlot[] plots = FindObjectsByType<HerbGardenPlot>(
            FindObjectsInactive.Include
        );

        for (int i = 0; i < plots.Length; i++)
        {
            HerbGardenPlot plot = plots[i];

            if (plot == null)
                continue;

            if (plot.currentState == GardenState.Empty)
                continue;

            if (plot.currentPlant == null)
                continue;

            if (plot.currentPlant.rewardHerb != herb)
                continue;

            if (string.IsNullOrEmpty(plot.currentPlantBatchId))
                continue;

            return plot.currentPlantBatchId;
        }

        return "";
    }

    private void ClearOpenBatchIfNoRemainingPlots(HerbData herb, string batchId)
    {
        if (herb == null || string.IsNullOrEmpty(batchId))
            return;

        HerbGardenPlot[] plots = FindObjectsByType<HerbGardenPlot>(
            FindObjectsInactive.Include
        );

        for (int i = 0; i < plots.Length; i++)
        {
            HerbGardenPlot plot = plots[i];

            if (plot == null)
                continue;

            if (plot.HasSameActiveBatch(herb, batchId))
                return;
        }

        PlayerPrefs.DeleteKey(GetOpenBatchKey(herb));
        PlayerPrefs.Save();

        Debug.Log("Đã kết thúc lượt trồng " + herb.herbName + ". Batch: " + batchId);
    }

    private bool HasSameActiveBatch(HerbData herb, string batchId)
    {
        if (currentState == GardenState.Empty)
            return false;

        if (currentPlant == null)
            return false;

        if (currentPlant.rewardHerb != herb)
            return false;

        return currentPlantBatchId == batchId;
    }

    private string GetOpenBatchKey(HerbData herb)
    {
        if (herb == null)
            return HerbOpenBatchKeyPrefix + "Unknown";

        string herbKey = herb.name;

        if (string.IsNullOrWhiteSpace(herbKey))
            herbKey = herb.herbName;

        return HerbOpenBatchKeyPrefix + herbKey;
    }

    private void DeleteOpenBatchKeysForKnownGardenPlants()
    {
        if (gardenPlantDatabase == null || gardenPlantDatabase.plants == null)
            return;

        for (int i = 0; i < gardenPlantDatabase.plants.Count; i++)
        {
            GardenPlantData plant = gardenPlantDatabase.plants[i];

            if (plant == null || plant.rewardHerb == null)
                continue;

            PlayerPrefs.DeleteKey(GetOpenBatchKey(plant.rewardHerb));
        }
    }
}