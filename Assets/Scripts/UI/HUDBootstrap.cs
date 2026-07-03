using UnityEngine;

public class HUDBootstrap : MonoBehaviour
{
    [Header("Prefab HUD persistent")]
    [SerializeField] private GameObject hudPrefab;

    private const string HudObjectName = "HUD_Canvas_Persistent";

    private void Awake()
    {
        EnsureHUDExists();
    }

    private void EnsureHUDExists()
    {
        GameObject existingHUD = GameObject.Find(HudObjectName);

        if (existingHUD != null)
        {
            return;
        }

        if (hudPrefab == null)
        {
            Debug.LogWarning("HUDBootstrap chưa được gán HUD Prefab.");
            return;
        }

        GameObject createdHUD = Instantiate(hudPrefab);
        createdHUD.name = HudObjectName;

        DontDestroyOnLoad(createdHUD);

        Debug.Log("Đã tự tạo HUD_Canvas_Persistent vì scene hiện tại chưa có HUD.");
    }
}