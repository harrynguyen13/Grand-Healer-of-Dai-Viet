using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class LocalSaveManager : MonoBehaviour
{
    [SerializeField] private Transform player;

    private const string HasLocalSaveKey = "HasLocalSave";
    private const string PlayerSceneKey = "PlayerScene";
    private const string PlayerXKey = "PlayerX";
    private const string PlayerYKey = "PlayerY";
    private const string PlayerZKey = "PlayerZ";

    private bool hasStarted = false;

    private void Start()
    {
        FindPlayerIfMissing();
        LoadPlayerPosition();
        hasStarted = true;
    }

    private void Update()
    {
        if (Keyboard.current != null && Keyboard.current.f5Key.wasPressedThisFrame)
        {
            SaveGame();
        }
    }

    private void OnApplicationQuit()
    {
        SaveGame();
    }

    private void OnDisable()
    {
        if (!hasStarted)
            return;

        SaveGame();
    }

    public void SaveGame()
    {
        string currentScene = SceneManager.GetActiveScene().name;

        // Không lưu ở menu và scene cốt truyện
        if (currentScene == "LoginScene" || currentScene == "IntroScene")
            return;

        // Lưu kho thuốc trước
        // Kể cả không tìm thấy Player thì kho thuốc vẫn được lưu
        if (HerbInventory.Instance != null)
        {
            HerbInventory.Instance.SaveInventory();
        }
        else
        {
            Debug.LogWarning("Không tìm thấy HerbInventory để lưu kho thuốc.");
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

        FindPlayerIfMissing();

        if (player == null)
            return;

        string savedScene = PlayerPrefs.GetString(PlayerSceneKey, "");
        string currentScene = SceneManager.GetActiveScene().name;

        if (savedScene != currentScene)
            return;

        float x = PlayerPrefs.GetFloat(PlayerXKey, player.position.x);
        float y = PlayerPrefs.GetFloat(PlayerYKey, player.position.y);
        float z = PlayerPrefs.GetFloat(PlayerZKey, player.position.z);

        player.position = new Vector3(x, y, z);

        Rigidbody2D rb2d = player.GetComponent<Rigidbody2D>();

        if (rb2d != null)
        {
            rb2d.position = player.position;
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
        }

        Debug.Log("Đã load vị trí người chơi từ local save: " + player.position);
    }

    private void FindPlayerIfMissing()
    {
        if (player != null)
            return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
            player = playerObj.transform;
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
        PlayerPrefs.Save();

        Debug.Log("Đã xóa local save vị trí Player.");
    }
}