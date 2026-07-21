using System;
using UnityEngine;

public partial class HerbGardenPlot
{
    private void SaveGardenState()
    {
        PlayerPrefs.SetInt(StateKey, (int)currentState);

        if (currentPlant != null)
            PlayerPrefs.SetString(PlantKey, currentPlant.name);
        else
            PlayerPrefs.DeleteKey(PlantKey);

        if (!string.IsNullOrEmpty(currentPlantBatchId))
            PlayerPrefs.SetString(PlantBatchKey, currentPlantBatchId);
        else
            PlayerPrefs.DeleteKey(PlantBatchKey);

        PlayerPrefs.SetString(NextReadyUtcTicksKey, nextReadyUtcTime.Ticks.ToString());
        PlayerPrefs.Save();
    }

    private void LoadGardenState()
    {
        int savedState = PlayerPrefs.GetInt(StateKey, (int)GardenState.Empty);
        string savedPlantName = PlayerPrefs.GetString(PlantKey, "");
        string savedTicksText = PlayerPrefs.GetString(NextReadyUtcTicksKey, "");
        currentPlantBatchId = PlayerPrefs.GetString(PlantBatchKey, "");

        currentState = (GardenState)savedState;
        currentPlant = FindPlantByAssetName(savedPlantName);

        if (currentState == GardenState.Empty || currentPlant == null)
        {
            SetEmpty();
            Debug.Log("Load ô đất " + gardenId + ": đang trống.");
            return;
        }

        if (!string.IsNullOrEmpty(savedTicksText) &&
            long.TryParse(savedTicksText, out long savedTicks))
        {
            nextReadyUtcTime = new DateTime(savedTicks, DateTimeKind.Utc);
        }
        else
        {
            nextReadyUtcTime = DateTime.UtcNow.AddSeconds(
                Mathf.Max(1f, currentPlant.growDurationSeconds)
            );
        }

        if (string.IsNullOrEmpty(currentPlantBatchId))
        {
            currentPlantBatchId = GetOrCreateOpenBatchId(currentPlant.rewardHerb);
            SaveGardenState();
        }

        if (currentState == GardenState.ReadyToHarvest)
        {
            SetReadyToHarvest();

            Debug.Log(
                "Load ô đất "
                + gardenId
                + ": cây đã sẵn sàng thu hoạch. Batch: "
                + currentPlantBatchId
            );

            return;
        }

        if (currentState == GardenState.Growing)
        {
            if (DateTime.UtcNow >= nextReadyUtcTime)
            {
                SetReadyToHarvest();
                SaveGardenState();

                Debug.Log("Load ô đất " + gardenId + ": thời gian đã hết khi tắt game.");
                return;
            }

            ApplyPlantVisual();

            if (harvestReadyIcon != null)
                harvestReadyIcon.SetActive(false);

            double remainingSeconds = (nextReadyUtcTime - DateTime.UtcNow).TotalSeconds;

            Debug.Log(
                "Load ô đất "
                + gardenId
                + ": "
                + currentPlant.plantName
                + " còn "
                + Mathf.CeilToInt((float)remainingSeconds)
                + " giây. Batch: "
                + currentPlantBatchId
            );
        }
    }

    private GardenPlantData FindPlantByAssetName(string assetName)
    {
        if (string.IsNullOrWhiteSpace(assetName))
            return null;

        if (gardenPlantDatabase == null || gardenPlantDatabase.plants == null)
            return null;

        for (int i = 0; i < gardenPlantDatabase.plants.Count; i++)
        {
            GardenPlantData plant = gardenPlantDatabase.plants[i];

            if (plant == null)
                continue;

            if (plant.name == assetName)
                return plant;
        }

        return null;
    }

    public void ResetGardenForNewGame()
    {
        PlayerPrefs.DeleteKey(StateKey);
        PlayerPrefs.DeleteKey(PlantKey);
        PlayerPrefs.DeleteKey(PlantBatchKey);
        PlayerPrefs.DeleteKey(NextReadyUtcTicksKey);

        DeleteOpenBatchKeysForKnownGardenPlants();

        PlayerPrefs.Save();

        SetEmpty();

        Debug.Log("Đã reset ô đất " + gardenId + " cho game mới.");
    }
}