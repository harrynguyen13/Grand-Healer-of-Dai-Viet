using System;
using System.Collections;
using UnityEngine;

public class HerbGardenPlot : MonoBehaviour
{
    private enum GardenState
    {
        Empty,
        Growing,
        ReadyToHarvest
    }

    [Header("ID riêng của ô đất")]
    [SerializeField] private string gardenId = "GardenPlot_01";

    [Header("Khóa / mở ô đất")]
    [SerializeField] private bool isUnlocked = true;
    [SerializeField] private GameObject lockedVisual;

    [Header("Database cây trồng")]
    [SerializeField] private GardenPlantDatabase gardenPlantDatabase;

    [Header("Sprite cây trên ô đất")]
    [SerializeField] private SpriteRenderer plantRenderer;

    [Header("Icon khi có thể thu hoạch")]
    [SerializeField] private GameObject harvestReadyIcon;

    [Header("Điểm hiện text bay lên")]
    [SerializeField] private Transform floatingTextSpawnPoint;

    [Header("Prefab text bay lên")]
    [SerializeField] private FloatingHarvestText floatingTextPrefab;

    private GardenState currentState = GardenState.Empty;
    private GardenPlantData currentPlant;
    private DateTime nextReadyUtcTime;

    public bool IsUnlocked
    {
        get { return isUnlocked; }
    }

    public bool IsEmpty
    {
        get { return currentState == GardenState.Empty; }
    }

    public bool IsGrowing
    {
        get { return currentState == GardenState.Growing; }
    }

    public bool IsReadyToHarvest
    {
        get { return currentState == GardenState.ReadyToHarvest; }
    }

    private string StateKey
    {
        get { return "HerbGarden_" + gardenId + "_State"; }
    }

    private string PlantKey
    {
        get { return "HerbGarden_" + gardenId + "_Plant"; }
    }

    private string NextReadyUtcTicksKey
    {
        get { return "HerbGarden_" + gardenId + "_NextReadyUtcTicks"; }
    }

    private void Awake()
    {
        if (string.IsNullOrWhiteSpace(gardenId))
            gardenId = gameObject.name;

        AutoFindPlantRenderer();
    }

    private void Start()
    {
        LoadGardenState();
        ApplyLockedStateVisual();
    }

    private void Update()
    {
        if (!isUnlocked)
            return;

        if (currentState != GardenState.Growing)
            return;

        if (DateTime.UtcNow >= nextReadyUtcTime)
        {
            SetReadyToHarvest();
            SaveGardenState();

            Debug.Log("Ô đất " + gardenId + " đã có thể thu hoạch.");
        }
    }

    private void AutoFindPlantRenderer()
    {
        if (plantRenderer != null)
            return;

        Transform visualTransform = transform.Find("PlantVisual");

        if (visualTransform != null)
        {
            plantRenderer = visualTransform.GetComponent<SpriteRenderer>();
        }
    }

    public void SetPlotUnlocked(bool unlocked)
    {
        isUnlocked = unlocked;
        ApplyLockedStateVisual();
    }

    private void ApplyLockedStateVisual()
    {
        if (lockedVisual != null)
            lockedVisual.SetActive(!isUnlocked);

        if (!isUnlocked)
        {
            if (plantRenderer != null)
                plantRenderer.enabled = false;

            if (harvestReadyIcon != null)
                harvestReadyIcon.SetActive(false);

            return;
        }

        ApplyPlantVisual();

        if (harvestReadyIcon != null)
            harvestReadyIcon.SetActive(currentState == GardenState.ReadyToHarvest);
    }

    public void TryHarvest()
    {
        Interact();
    }

    public void Interact()
    {
        if (!isUnlocked)
        {
            Debug.Log("Ô đất " + gardenId + " chưa được mở khóa.");
            return;
        }

        if (currentState == GardenState.Empty)
        {
            TryPlantSelectedPlant();
            return;
        }

        if (currentState == GardenState.Growing)
        {
            Debug.Log("Cây đang phát triển, chưa thể thu hoạch.");
            return;
        }

        if (currentState == GardenState.ReadyToHarvest)
        {
            HarvestCurrentPlant();
        }
    }

    private void TryPlantSelectedPlant()
    {
        if (GardenPlantSelectionUI.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy GardenPlantSelectionUI.");
            return;
        }

        if (!GardenPlantSelectionUI.Instance.HasSelectedPlant)
        {
            Debug.Log("Chưa chọn cây trồng. Hãy mở UI và chọn cây trước.");
            return;
        }

        Plant(GardenPlantSelectionUI.Instance.SelectedPlant);
    }

    private void Plant(GardenPlantData plantData)
    {
        if (!isUnlocked)
        {
            Debug.Log("Ô đất " + gardenId + " chưa được mở khóa.");
            return;
        }

        if (plantData == null)
        {
            Debug.LogWarning("GardenPlantData bị null, không thể trồng.");
            return;
        }

        currentPlant = plantData;
        currentState = GardenState.Growing;

        float baseGrowDuration = Mathf.Max(1f, currentPlant.growDurationSeconds);
        float growDuration = GetGrowDurationByPlayerLevel(baseGrowDuration);

        nextReadyUtcTime = DateTime.UtcNow.AddSeconds(growDuration);

        ApplyPlantVisual();

        if (harvestReadyIcon != null)
            harvestReadyIcon.SetActive(false);

        SaveGardenState();

        Debug.Log(
            "Đã trồng "
            + currentPlant.plantName
            + " tại ô "
            + gardenId
            + ". Cấp vườn: "
            + PlayerLevelService.GetCurrentUnlockLevel()
            + ". Sẵn sàng sau "
            + growDuration
            + " giây."
        );
    }


    private float GetGrowDurationByPlayerLevel(float baseGrowDuration)
    {
        int unlockLevel = PlayerLevelService.GetCurrentUnlockLevel();

        if (unlockLevel <= 2)
            return baseGrowDuration;

        if (unlockLevel == 3)
            return baseGrowDuration * 1.5f;

        if (unlockLevel == 4)
            return baseGrowDuration * 2f;

        return baseGrowDuration * 2.5f;
    }

    private void SetReadyToHarvest()
    {
        currentState = GardenState.ReadyToHarvest;

        ApplyPlantVisual();

        if (harvestReadyIcon != null)
            harvestReadyIcon.SetActive(isUnlocked);
    }

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

        int amount = Mathf.Max(1, currentPlant.harvestAmount);

        HerbInventory.Instance.AddHerb(currentPlant.rewardHerb, amount);

        if (QuestProgressManager.Instance != null)
        {
            QuestProgressManager.Instance.RecordHerbGathered(currentPlant.rewardHerb, amount);
        }

        StartCoroutine(ShowFloatingText(currentPlant.rewardHerb.herbName, amount));

        Debug.Log(
            "Thu hoạch "
            + currentPlant.plantName
            + " -> nhận "
            + currentPlant.rewardHerb.herbName
            + " +"
            + amount
        );

        SetEmpty();
        SaveGardenState();
    }

    private void SetEmpty()
    {
        currentState = GardenState.Empty;
        currentPlant = null;
        nextReadyUtcTime = DateTime.MinValue;

        if (plantRenderer != null)
        {
            plantRenderer.sprite = null;
            plantRenderer.enabled = false;
        }

        if (harvestReadyIcon != null)
            harvestReadyIcon.SetActive(false);

        ApplyLockedStateVisual();
    }

    private void ApplyPlantVisual()
    {
        if (!isUnlocked)
        {
            if (plantRenderer != null)
                plantRenderer.enabled = false;

            return;
        }

        if (plantRenderer == null)
        {
            Debug.LogWarning("Ô đất " + gardenId + " chưa kéo Plant Renderer.");
            return;
        }

        if (currentPlant == null)
        {
            plantRenderer.sprite = null;
            plantRenderer.enabled = false;
            return;
        }

        if (currentState == GardenState.Growing)
        {
            plantRenderer.sprite = currentPlant.seedlingSprite;
            plantRenderer.enabled = currentPlant.seedlingSprite != null;
            return;
        }

        if (currentState == GardenState.ReadyToHarvest)
        {
            plantRenderer.sprite = currentPlant.matureSprite;
            plantRenderer.enabled = currentPlant.matureSprite != null;
        }
    }

    private void SaveGardenState()
    {
        PlayerPrefs.SetInt(StateKey, (int)currentState);

        if (currentPlant != null)
            PlayerPrefs.SetString(PlantKey, currentPlant.name);
        else
            PlayerPrefs.DeleteKey(PlantKey);

        PlayerPrefs.SetString(NextReadyUtcTicksKey, nextReadyUtcTime.Ticks.ToString());
        PlayerPrefs.Save();
    }

    private void LoadGardenState()
    {
        int savedState = PlayerPrefs.GetInt(StateKey, (int)GardenState.Empty);
        string savedPlantName = PlayerPrefs.GetString(PlantKey, "");
        string savedTicksText = PlayerPrefs.GetString(NextReadyUtcTicksKey, "");

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

        if (currentState == GardenState.ReadyToHarvest)
        {
            SetReadyToHarvest();
            Debug.Log("Load ô đất " + gardenId + ": cây đã sẵn sàng thu hoạch.");
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
                + " giây."
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

        amount = Mathf.Max(1, currentPlant.harvestAmount);
        herbName = currentPlant.rewardHerb.herbName;

        HerbInventory.Instance.AddHerb(currentPlant.rewardHerb, amount);

        if (QuestProgressManager.Instance != null)
        {
            QuestProgressManager.Instance.RecordHerbGathered(currentPlant.rewardHerb, amount);
        }

        Debug.Log(
            "Thu hoạch "
            + currentPlant.plantName
            + " -> nhận "
            + herbName
            + " +"
            + amount
        );

        SetEmpty();
        SaveGardenState();

        return true;
    }

    public void ResetGardenForNewGame()
    {
        PlayerPrefs.DeleteKey(StateKey);
        PlayerPrefs.DeleteKey(PlantKey);
        PlayerPrefs.DeleteKey(NextReadyUtcTicksKey);
        PlayerPrefs.Save();

        SetEmpty();

        Debug.Log("Đã reset ô đất " + gardenId + " cho game mới.");
    }
}