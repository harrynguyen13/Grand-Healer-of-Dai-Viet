using System.Collections.Generic;
using UnityEngine;

public class GardenPlantSelectionUI : MonoBehaviour
{
    public static GardenPlantSelectionUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject panelRoot;
    [SerializeField] private Transform contentRoot;
    [SerializeField] private GardenPlantItemUI itemPrefab;

    [Header("Database cây trồng")]
    [SerializeField] private GardenPlantDatabase gardenPlantDatabase;

    public GardenPlantData SelectedPlant { get; private set; }

    public bool HasSelectedPlant
    {
        get { return SelectedPlant != null; }
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
        SelectedPlant = plant;

        Debug.Log("Đã chọn cây để trồng: " + plant.plantName);

        Close();
    }
}