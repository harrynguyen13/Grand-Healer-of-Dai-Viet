using System.Collections;
using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class LocalSaveManager : MonoBehaviour
{
    public static LocalSaveManager Instance { get; private set; }

    [Header("Player")]
    [SerializeField] private Transform player;
    [SerializeField] private GameObject playerPrefab;
    [SerializeField] private string playerTag = "Player";

    [Header("Scene không save")]
    [SerializeField] private string[] ignoredScenes =
    {
        "LoginScene",
        "IntroScene"
    };

    private const string HasLocalSaveKey = "HasLocalSave";
    private const string PlayerSceneKey = "PlayerScene";
    private const string PlayerXKey = "PlayerX";
    private const string PlayerYKey = "PlayerY";
    private const string PlayerZKey = "PlayerZ";
    private const string LoadFromSaveKey = "LoadFromSave";

    private bool hasStarted = false;
    private bool applicationQuitting = false;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        SceneManager.sceneLoaded += OnSceneLoaded;
    }

    private void Start()
    {
        hasStarted = true;

        string currentScene = SceneManager.GetActiveScene().name;

        if (!ShouldIgnoreScene(currentScene))
        {
            StartCoroutine(PreparePlayerAfterSceneLoaded());
        }
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f5Key.wasPressedThisFrame)
        {
            SaveGame();
        }
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        if (ShouldIgnoreScene(scene.name))
            return;

        StartCoroutine(PreparePlayerAfterSceneLoaded());
    }

    private IEnumerator PreparePlayerAfterSceneLoaded()
    {
        yield return null;
        yield return null;

        FindOrCreatePlayerIfMissing();

        if (PlayerPrefs.GetInt(LoadFromSaveKey, 0) == 1)
        {
            LoadPlayerPosition();

            PlayerPrefs.DeleteKey(LoadFromSaveKey);
            PlayerPrefs.Save();
        }
    }

    public void SaveGame()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        if (ShouldIgnoreScene(currentScene))
            return;

        if (HerbInventory.Instance != null)
        {
            HerbInventory.Instance.SaveInventory();
        }
        else
        {
            Debug.LogWarning("Không tìm thấy HerbInventory để lưu kho thuốc.");
        }

        if (PlayerEconomy.Instance != null)
        {
            PlayerEconomy.Instance.SaveEconomy();
        }

        FindPlayerIfMissing();

        if (player == null)
        {
            PlayerPrefs.Save();
            Debug.Log("Đã lưu dữ liệu hệ thống, nhưng không tìm thấy Player để lưu vị trí.");
            return;
        }

        PlayerPrefs.SetInt(HasLocalSaveKey, 1);
        PlayerPrefs.SetString(PlayerSceneKey, currentScene);

        PlayerPrefs.SetFloat(PlayerXKey, player.position.x);
        PlayerPrefs.SetFloat(PlayerYKey, player.position.y);
        PlayerPrefs.SetFloat(PlayerZKey, player.position.z);

        PlayerPrefs.Save();

        Debug.Log("Đã lưu game local tại scene " + currentScene + ": " + player.position);
    }

    private void LoadPlayerPosition()
    {
        if (PlayerPrefs.GetInt(HasLocalSaveKey, 0) != 1)
            return;

        FindOrCreatePlayerIfMissing();

        if (player == null)
        {
            Debug.LogWarning("Không có Player để load vị trí.");
            return;
        }

        string savedScene = PlayerPrefs.GetString(PlayerSceneKey, "");
        string currentScene = SceneManager.GetActiveScene().name;

        Debug.Log("Load vị trí save. SavedScene = " + savedScene + ", CurrentScene = " + currentScene);

        if (savedScene != currentScene)
        {
            Debug.LogWarning("Không load vị trí vì scene hiện tại không khớp scene đã save.");
            return;
        }

        Vector3 savedPosition = new Vector3(
            PlayerPrefs.GetFloat(PlayerXKey, player.position.x),
            PlayerPrefs.GetFloat(PlayerYKey, player.position.y),
            PlayerPrefs.GetFloat(PlayerZKey, player.position.z)
        );

        player.position = savedPosition;

        Rigidbody2D rb2d = player.GetComponent<Rigidbody2D>();

        if (rb2d != null)
        {
            rb2d.position = savedPosition;
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
        }

        Debug.Log("Đã load vị trí người chơi từ local save: " + savedPosition);
    }

    private void FindPlayerIfMissing()
    {
        if (player != null)
            return;

        GameObject playerObj = GameObject.FindGameObjectWithTag(playerTag);

        if (playerObj != null)
        {
            player = playerObj.transform;
        }
    }

    private void FindOrCreatePlayerIfMissing()
    {
        FindPlayerIfMissing();

        if (player != null)
            return;

        string currentScene = SceneManager.GetActiveScene().name;

        if (ShouldIgnoreScene(currentScene))
            return;

        if (playerPrefab == null)
        {
            Debug.LogWarning("Không tìm thấy Player trong scene và chưa kéo Player Prefab vào LocalSaveManager.");
            return;
        }

        GameObject newPlayer = Instantiate(playerPrefab, Vector3.zero, Quaternion.identity);
        newPlayer.name = playerPrefab.name;

        player = newPlayer.transform;

        if (newPlayer.GetComponent<PlayerSceneKeeper>() == null)
        {
            newPlayer.AddComponent<PlayerSceneKeeper>();
        }

        Debug.Log("Đã tạo Player mới từ prefab vì scene này không có Player.");
    }

    private bool ShouldIgnoreScene(string sceneName)
    {
        for (int i = 0; i < ignoredScenes.Length; i++)
        {
            if (ignoredScenes[i] == sceneName)
                return true;
        }

        return false;
    }

    [ContextMenu("Save Game Now")]
    public void SaveGameNow()
    {
        SaveGame();
    }

    [ContextMenu("Delete Local Player Save")]
    public void DeleteLocalPlayerSave()
    {
        PlayerPrefs.DeleteKey(HasLocalSaveKey);
        PlayerPrefs.DeleteKey(PlayerSceneKey);
        PlayerPrefs.DeleteKey(PlayerXKey);
        PlayerPrefs.DeleteKey(PlayerYKey);
        PlayerPrefs.DeleteKey(PlayerZKey);
        PlayerPrefs.DeleteKey(LoadFromSaveKey);
        PlayerPrefs.Save();

        Debug.Log("Đã xóa local save vị trí Player.");
    }

    private void OnApplicationQuit()
    {
        applicationQuitting = true;
        SaveGame();
    }

    private void OnDisable()
    {
        if (!hasStarted)
            return;

        if (applicationQuitting)
            return;

        SaveGame();
    }

    private void OnDestroy()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }
}