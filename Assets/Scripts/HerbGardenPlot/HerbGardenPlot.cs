using System;
using UnityEngine;

public partial class HerbGardenPlot : MonoBehaviour
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

    private const string HerbOpenBatchKeyPrefix = "HerbGarden_OpenBatch_";

    private GardenState currentState = GardenState.Empty;
    private GardenPlantData currentPlant;
    private DateTime nextReadyUtcTime;
    private string currentPlantBatchId = "";

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

    private string PlantBatchKey
    {
        get { return "HerbGarden_" + gardenId + "_PlantBatch"; }
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
            plantRenderer = visualTransform.GetComponent<SpriteRenderer>();
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

        string batchId = GetOrCreateOpenBatchId(plantData.rewardHerb);

        currentPlant = plantData;
        currentState = GardenState.Growing;
        currentPlantBatchId = batchId;

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
            + ". Batch: "
            + currentPlantBatchId
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

    private void SetEmpty()
    {
        currentState = GardenState.Empty;
        currentPlant = null;
        currentPlantBatchId = "";
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
}