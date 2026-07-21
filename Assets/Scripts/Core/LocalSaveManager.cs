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

    [Header("Scene Menu")]
    [SerializeField] private string loginSceneName = "LoginScene";

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
            string savedScene = PlayerPrefs.GetString(PlayerSceneKey, "");
            string currentScene = SceneManager.GetActiveScene().name;

            if (savedScene != currentScene)
            {
                Debug.LogWarning("Không load vị trí vì scene hiện tại không khớp scene đã save. SavedScene = "
                    + savedScene
                    + ", CurrentScene = "
                    + currentScene);

                PlayerPrefs.DeleteKey(LoadFromSaveKey);
                PlayerPrefs.Save();

                yield break;
            }

            Vector3 savedPosition = GetSavedPlayerPosition();

        yield return StartCoroutine(ApplyPlayerPositionRepeated(savedPosition, 10));

        // Chờ toàn bộ NPC trong scene khởi tạo xong
        yield return null;
        yield return null;

        // Load vị trí NPC
        if (NPCSaveManager.Instance != null)
        {
            NPCSaveManager.Instance.LoadNPCs();
        }
        else
        {
            Debug.LogWarning("Không tìm thấy NPCSaveManager.");
        }

        PlayerPrefs.DeleteKey(LoadFromSaveKey);
        PlayerPrefs.Save();

        Debug.Log("Đã load vị trí người chơi từ local save: " + savedPosition);
        }
    }

    public bool HasLocalSave()
    {
        return PlayerPrefs.GetInt(HasLocalSaveKey, 0) == 1;
    }

    public string GetSavedSceneName()
    {
        return PlayerPrefs.GetString(PlayerSceneKey, "");
    }

    public void ContinueGameFromLocalSave()
    {
        if (!HasLocalSave())
        {
            Debug.LogWarning("Không có local save để tiếp tục.");
            return;
        }

        string savedScene = GetSavedSceneName();

        if (string.IsNullOrWhiteSpace(savedScene))
        {
            Debug.LogWarning("Có save nhưng không có tên scene đã lưu.");
            return;
        }

        PlayerPrefs.SetInt(LoadFromSaveKey, 1);
        PlayerPrefs.Save();

        Debug.Log("Tiếp tục game từ local save. Scene: " + savedScene);

        SceneManager.LoadScene(savedScene);
    }

    public void SaveAndReturnToLogin()
    {
        SaveGame();

        Debug.Log("Đã lưu game trước khi quay về LoginScene.");

        SceneManager.LoadScene(loginSceneName);
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

        // Lưu vị trí toàn bộ NPC
        if (NPCSaveManager.Instance != null)
        {
            NPCSaveManager.Instance.SaveNPCs();
        }
        else
        {
            Debug.LogWarning("Không tìm thấy NPCSaveManager.");
        }

        Debug.Log("Đã lưu game local tại scene " + currentScene + ": " + player.position);
    }

    private Vector3 GetSavedPlayerPosition()
    {
        FindPlayerIfMissing();

        Vector3 fallbackPosition = player != null ? player.position : Vector3.zero;

        return new Vector3(
            PlayerPrefs.GetFloat(PlayerXKey, fallbackPosition.x),
            PlayerPrefs.GetFloat(PlayerYKey, fallbackPosition.y),
            PlayerPrefs.GetFloat(PlayerZKey, fallbackPosition.z)
        );
    }

    private IEnumerator ApplyPlayerPositionRepeated(Vector3 targetPosition, int frameCount)
    {
        for (int i = 0; i < frameCount; i++)
        {
            FindOrCreatePlayerIfMissing();

            if (player != null)
            {
                ApplyPlayerPosition(targetPosition);
                RebindMinimapToPlayer();
            }

            yield return null;
        }

        if (player != null)
        {
            ApplyPlayerPosition(targetPosition);
            RebindMinimapToPlayer();
        }
    }

    private void ApplyPlayerPosition(Vector3 targetPosition)
    {
        if (player == null)
            return;

        player.position = targetPosition;

        Rigidbody2D rb2d = player.GetComponent<Rigidbody2D>();

        if (rb2d != null)
        {
            rb2d.position = targetPosition;
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
        }
    }

    private void RebindMinimapToPlayer()
    {
        if (player == null)
            return;

        GameObject minimapCameraObj = GameObject.Find("MinimapCamera");

        if (minimapCameraObj != null)
        {
            minimapCameraObj.SendMessage(
                "SetTarget",
                player,
                SendMessageOptions.DontRequireReceiver
            );

            Vector3 minimapPosition = minimapCameraObj.transform.position;
            minimapCameraObj.transform.position = new Vector3(
                player.position.x,
                player.position.y,
                minimapPosition.z
            );
        }

        Camera mainCamera = Camera.main;

        if (mainCamera != null)
        {
            mainCamera.SendMessage(
                "SetTarget",
                player,
                SendMessageOptions.DontRequireReceiver
            );
        }
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

    [ContextMenu("Continue From Local Save")]
    public void ContinueFromLocalSaveContext()
    {
        ContinueGameFromLocalSave();
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

    public void ResetForNewGame()
{
    Debug.Log("===== LocalSaveManager reset cho game mới =====");

    PlayerPrefs.DeleteKey(HasLocalSaveKey);
    PlayerPrefs.DeleteKey(PlayerSceneKey);
    PlayerPrefs.DeleteKey(PlayerXKey);
    PlayerPrefs.DeleteKey(PlayerYKey);
    PlayerPrefs.DeleteKey(PlayerZKey);

    PlayerPrefs.SetInt(LoadFromSaveKey, 0);
    PlayerPrefs.Save();

    SceneTransitionData.isChangingScene = false;
    SceneTransitionData.targetSpawnPointName = "";

    DestroyCurrentPlayerInMemory();

    player = null;

    Debug.Log("Đã xóa local save vị trí và hủy Player cũ trong RAM.");
    }

    private void DestroyCurrentPlayerInMemory()
    {
        if (player != null)
        {
            Destroy(player.gameObject);
            player = null;
        }

        GameObject[] playerObjects = GameObject.FindGameObjectsWithTag(playerTag);

        for (int i = 0; i < playerObjects.Length; i++)
        {
            if (playerObjects[i] == null)
                continue;

            Destroy(playerObjects[i]);
        }

        if (PlayerSceneKeeper.Instance != null)
        {
            Destroy(PlayerSceneKeeper.Instance.gameObject);
        }
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