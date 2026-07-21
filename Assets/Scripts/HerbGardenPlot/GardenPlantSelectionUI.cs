using System.Collections.Generic;
using UnityEngine;

public class GardenPlantSelectionUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private GardenPlantItemUI itemPrefab;

    [Header("Database cây trồng")]
    [SerializeField] private GardenPlantDatabase gardenPlantDatabase;

    public static GardenPlantSelectionUI Instance { get; private set; }

    public bool IsOpen
    {
        get
        {
            return panelRoot != null && panelRoot.activeSelf;
        }
    }

    public GardenPlantData SelectedPlant { get; private set; }

    public bool HasSelectedPlant
    {
        get { return SelectedPlant != null; }
    }

    private string currentPlantingBatchId = "";

    public string CurrentPlantingBatchId
    {
        get
        {
            if (string.IsNullOrEmpty(currentPlantingBatchId))
                currentPlantingBatchId = System.Guid.NewGuid().ToString("N");

            return currentPlantingBatchId;
        }
    }

    private readonly List<GameObject> spawnedItems = new List<GameObject>();

    private void Awake()
    {
        Instance = this;

        if (panelRoot == null)
            panelRoot = gameObject;
    }

    private void OnEnable()
    {
        BuildList();
    }

    public void Open()
    {
        if (panelRoot != null)
            panelRoot.SetActive(true);

        BuildList();
    }

    public void Close()
    {
        if (panelRoot != null)
            panelRoot.SetActive(false);
    }

    public void ClearSelectedPlant()
    {
        SelectedPlant = null;
        currentPlantingBatchId = "";

        Debug.Log("Đã hủy chọn cây trồng.");
    }

    private void BuildList()
    {
        ClearItems();

        if (gardenPlantDatabase == null)
        {
            Debug.LogWarning("GardenPlantSelectionUI chưa kéo GardenPlantDatabase.");
            return;
        }

        if (contentRoot == null)
        {
            Debug.LogWarning("GardenPlantSelectionUI chưa kéo Content Root.");
            return;
        }

        if (itemPrefab == null)
        {
            Debug.LogWarning("GardenPlantSelectionUI chưa kéo Item Prefab.");
            return;
        }

        if (gardenPlantDatabase.plants == null || gardenPlantDatabase.plants.Count == 0)
        {
            Debug.LogWarning("GardenPlantDatabase chưa có cây nào trong list Plants.");
            return;
        }

        for (int i = 0; i < gardenPlantDatabase.plants.Count; i++)
        {
            GardenPlantData plant = gardenPlantDatabase.plants[i];

            if (plant == null)
                continue;

            GardenPlantItemUI item = Instantiate(itemPrefab, contentRoot);
            item.Setup(plant, OnPlantClicked);

            spawnedItems.Add(item.gameObject);
        }

        Debug.Log("Đã tạo " + spawnedItems.Count + " item cây trồng trong UI.");
    }

    private void ClearItems()
    {
        for (int i = 0; i < spawnedItems.Count; i++)
        {
            if (spawnedItems[i] != null)
                Destroy(spawnedItems[i]);
        }

        spawnedItems.Clear();
    }

    private void OnPlantClicked(GardenPlantData plant)
    {
        if (plant == null)
            return;

        SelectedPlant = plant;

        // Mỗi lần chọn cây trong UI = 1 lượt trồng mới.
        // Trồng nhiều ô sau lần chọn này thì các ô đó cùng batch.
        currentPlantingBatchId = System.Guid.NewGuid().ToString("N");

        Debug.Log(
            "Đã chọn cây để trồng: "
            + plant.plantName
            + " | Batch: "
            + currentPlantingBatchId
        );

        Close();
    }
}