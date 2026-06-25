using UnityEngine;
using UnityEngine.SceneManagement;

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
        if (Input.GetKeyDown(KeyCode.F5))
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
        if (!hasStarted) return;

        SaveGame();
    }

    public void SaveGame()
    {
        // Không lưu ở menu và scene cốt truyện
        string currentScene = SceneManager.GetActiveScene().name;

        if (currentScene == "LoginScene" || currentScene == "IntroScene")
            return;

        FindPlayerIfMissing();

        // Không có Player thì bỏ qua, không báo lỗi nữa
        if (player == null)
            return;

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

        Debug.Log("Đã load vị trí người chơi từ local save: " + player.position);
    }

    private void FindPlayerIfMissing()
    {
        if (player != null) return;

        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj != null)
            player = playerObj.transform;
    }
}