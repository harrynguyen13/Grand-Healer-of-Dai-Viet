using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

public class HerbGardenPlot : MonoBehaviour
{
    [Header("ID riêng của vườn")]
    [SerializeField] private string gardenId = "HomeGarden_01";

    [Header("Icon khi có thể thu hoạch")]
    [SerializeField] private GameObject harvestReadyIcon;

    [Header("Điểm hiện text bay lên")]
    [SerializeField] private Transform floatingTextSpawnPoint;

    [Header("Prefab text bay lên")]
    [SerializeField] private FloatingHarvestText floatingTextPrefab;

    [Header("Danh sách thuốc có thể thu trong vườn nhà")]
    [SerializeField] private List<HerbData> gardenHerbs = new List<HerbData>();

    [Header("Số loại thuốc nhận mỗi lần thu hoạch")]
    [SerializeField] private int herbsPerHarvest = 3;

    [Header("Thời gian hồi theo cấp người chơi")]
    [SerializeField] private float level1GrowTime = 50f;
    [SerializeField] private float level2GrowTime = 80f;
    [SerializeField] private float level3GrowTime = 100f;
    [SerializeField] private float level4GrowTime = 120f;
    [SerializeField] private float level5GrowTime = 140f;

    [Header("Số lượng thuốc nhận theo cấp người chơi")]
    [SerializeField] private Vector2Int level1AmountRange = new Vector2Int(2, 5);
    [SerializeField] private Vector2Int level2AmountRange = new Vector2Int(4, 8);
    [SerializeField] private Vector2Int level3AmountRange = new Vector2Int(6, 10);
    [SerializeField] private Vector2Int level4AmountRange = new Vector2Int(8, 15);
    [SerializeField] private Vector2Int level5AmountRange = new Vector2Int(10, 20);

    private bool isReadyToHarvest;
    private DateTime nextReadyUtcTime;

    private string ReadyKey
    {
        get { return "HerbGarden_" + gardenId + "_Ready"; }
    }

    private string NextReadyUtcTicksKey
    {
        get { return "HerbGarden_" + gardenId + "_NextReadyUtcTicks"; }
    }

    private void Start()
    {
        if (string.IsNullOrWhiteSpace(gardenId))
            gardenId = gameObject.name;

        LoadGardenState();
    }

    private void Update()
    {
        if (isReadyToHarvest)
            return;

        if (DateTime.UtcNow >= nextReadyUtcTime)
        {
            SetHarvestReady(true);
            SaveGardenState();

            Debug.Log("Vườn thuốc đã hồi xong, có thể thu hoạch.");
        }
    }

    public void TryHarvest()
    {
        if (!isReadyToHarvest)
        {
            Debug.Log("Vườn thuốc chưa sẵn sàng để thu hoạch.");
            return;
        }

        if (gardenHerbs == null || gardenHerbs.Count == 0)
        {
            Debug.LogWarning("Vườn chưa có danh sách HerbData.");
            return;
        }

        if (HerbInventory.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy HerbInventory.Instance để cộng thuốc.");
            return;
        }

        List<HerbData> pickedHerbs = GetRandomUniqueHerbs();

        if (pickedHerbs.Count == 0)
        {
            Debug.LogWarning("Không random được thuốc vì danh sách gardenHerbs rỗng hoặc toàn null.");
            return;
        }

        List<HarvestResult> harvestResults = new List<HarvestResult>();

        for (int i = 0; i < pickedHerbs.Count; i++)
        {
            HerbData herb = pickedHerbs[i];

            if (herb == null)
                continue;

            int amount = GetRandomAmountByPlayerLevel();

            HerbInventory.Instance.AddHerb(herb, amount);

            if (QuestProgressManager.Instance != null)
            {
                QuestProgressManager.Instance.RecordHerbGathered(herb, amount);
            }

            harvestResults.Add(new HarvestResult(herb.herbName, amount));

            Debug.Log("Thu hoạch vườn thuốc: " + herb.herbName + " +" + amount);
        }

        StartCoroutine(ShowFloatingTexts(harvestResults));

        SetHarvestReady(false);
        StartNewGrowCycle();
        SaveGardenState();
    }

    private void LoadGardenState()
    {
        int savedReady = PlayerPrefs.GetInt(ReadyKey, 0);
        string savedTicksText = PlayerPrefs.GetString(NextReadyUtcTicksKey, "");

        if (savedReady == 1)
        {
            SetHarvestReady(true);
            Debug.Log("Load vườn thuốc: đang sẵn sàng thu hoạch.");
            return;
        }

        if (!string.IsNullOrEmpty(savedTicksText) &&
            long.TryParse(savedTicksText, out long savedTicks))
        {
            nextReadyUtcTime = new DateTime(savedTicks, DateTimeKind.Utc);

            if (DateTime.UtcNow >= nextReadyUtcTime)
            {
                SetHarvestReady(true);
                SaveGardenState();

                Debug.Log("Load vườn thuốc: thời gian đã hết trong lúc tắt game.");
                return;
            }

            SetHarvestReady(false);

            double remainingSeconds = (nextReadyUtcTime - DateTime.UtcNow).TotalSeconds;
            Debug.Log("Load vườn thuốc: còn " + Mathf.CeilToInt((float)remainingSeconds) + " giây để thu hoạch.");
            return;
        }

        SetHarvestReady(false);
        StartNewGrowCycle();
        SaveGardenState();

        Debug.Log("Load vườn thuốc: chưa có save, bắt đầu vòng hồi mới.");
    }

    private void StartNewGrowCycle()
    {
        float growTime = GetGrowTimeByPlayerLevel();
        nextReadyUtcTime = DateTime.UtcNow.AddSeconds(growTime);

        Debug.Log("Vườn thuốc bắt đầu hồi. Sẵn sàng sau " + growTime + " giây.");
    }

    private void SaveGardenState()
    {
        PlayerPrefs.SetInt(ReadyKey, isReadyToHarvest ? 1 : 0);
        PlayerPrefs.SetString(NextReadyUtcTicksKey, nextReadyUtcTime.Ticks.ToString());
        PlayerPrefs.Save();
    }

    private void SetHarvestReady(bool ready)
    {
        isReadyToHarvest = ready;

        if (harvestReadyIcon != null)
            harvestReadyIcon.SetActive(ready);
    }

    private List<HerbData> GetRandomUniqueHerbs()
    {
        List<HerbData> validHerbs = gardenHerbs
            .Where(herb => herb != null)
            .OrderBy(herb => UnityEngine.Random.value)
            .ToList();

        int count = Mathf.Min(herbsPerHarvest, validHerbs.Count);

        return validHerbs.Take(count).ToList();
    }

    private IEnumerator ShowFloatingTexts(List<HarvestResult> results)
    {
        if (floatingTextPrefab == null || floatingTextSpawnPoint == null)
            yield break;

        for (int i = 0; i < results.Count; i++)
        {
            HarvestResult result = results[i];

            Vector3 spawnPosition =
                floatingTextSpawnPoint.position +
                new Vector3(0f, i * 0.3f, 0f);

            FloatingHarvestText textInstance = Instantiate(
                floatingTextPrefab,
                spawnPosition,
                Quaternion.identity
            );

            textInstance.Setup(result.herbName + " +" + result.amount);

            yield return new WaitForSeconds(0.12f);
        }
    }

    private float GetGrowTimeByPlayerLevel()
    {
        int level = GetPlayerLevel();

        if (level == 1)
            return level1GrowTime;

        if (level == 2)
            return level2GrowTime;

        if (level == 3)
            return level3GrowTime;

        if (level == 4)
            return level4GrowTime;

        return level5GrowTime;
    }

    private int GetRandomAmountByPlayerLevel()
    {
        Vector2Int range = GetAmountRangeByPlayerLevel();

        int min = Mathf.Min(range.x, range.y);
        int max = Mathf.Max(range.x, range.y);

        return UnityEngine.Random.Range(min, max + 1);
    }

    private Vector2Int GetAmountRangeByPlayerLevel()
    {
        int level = GetPlayerLevel();

        if (level == 1)
            return level1AmountRange;

        if (level == 2)
            return level2AmountRange;

        if (level == 3)
            return level3AmountRange;

        if (level == 4)
            return level4AmountRange;

        return level5AmountRange;
    }

    private int GetPlayerLevel()
    {
        if (PlayerEconomy.Instance == null)
            return 1;

        int reputation = PlayerEconomy.Instance.Reputation;

        if (reputation < 100)
            return 1;

        if (reputation < 200)
            return 2;

        if (reputation < 300)
            return 3;

        if (reputation < 500)
            return 4;

        return 5;
    }

    public void ResetGardenForNewGame()
    {
        PlayerPrefs.DeleteKey(ReadyKey);
        PlayerPrefs.DeleteKey(NextReadyUtcTicksKey);
        PlayerPrefs.Save();

        SetHarvestReady(false);
        StartNewGrowCycle();
        SaveGardenState();

        Debug.Log("Đã reset vườn thuốc cho game mới.");
    }

    private struct HarvestResult
    {
        public string herbName;
        public int amount;

        public HarvestResult(string herbName, int amount)
        {
            this.herbName = herbName;
            this.amount = amount;
        }
    }
}