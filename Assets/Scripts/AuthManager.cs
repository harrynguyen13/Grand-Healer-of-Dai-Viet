using UnityEngine;

public class AuthManager : MonoBehaviour
{
    public static AuthManager Instance { get; private set; }

    private const string SavedUsernameKey = "RegisteredUsername";
    private const string SavedPasswordKey = "RegisteredPassword";

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public bool HasRegisteredAccount()
    {
        return PlayerPrefs.HasKey(SavedUsernameKey) && PlayerPrefs.HasKey(SavedPasswordKey);
    }

    public bool Register(string username, string password, out string message)
    {
        username = username.Trim();
        password = password.Trim();

        if (string.IsNullOrEmpty(username))
        {
            message = "Vui lòng nhập tên người chơi.";
            return false;
        }

        if (string.IsNullOrEmpty(password))
        {
            message = "Vui lòng nhập mật khẩu.";
            return false;
        }

        PlayerPrefs.SetString(SavedUsernameKey, username);
        PlayerPrefs.SetString(SavedPasswordKey, password);
        PlayerPrefs.Save();

        message = "Đăng ký thành công.";
        return true;
    }

    public bool Login(string username, string password, out string message)
    {
        username = username.Trim();
        password = password.Trim();

        if (!HasRegisteredAccount())
        {
            message = "Tài khoản chưa tồn tại. Vui lòng đăng ký trước.";
            return false;
        }

        string savedUsername = PlayerPrefs.GetString(SavedUsernameKey, "");
        string savedPassword = PlayerPrefs.GetString(SavedPasswordKey, "");

        if (username != savedUsername || password != savedPassword)
        {
            message = "Tên người chơi hoặc mật khẩu không đúng.";
            return false;
        }

        PlayerPrefs.SetString("PlayerName", username);
        PlayerPrefs.Save();

        message = "Đăng nhập thành công.";
        return true;
    }

    public void ClearSavedAccount()
    {
        PlayerPrefs.DeleteKey(SavedUsernameKey);
        PlayerPrefs.DeleteKey(SavedPasswordKey);
        PlayerPrefs.Save();
    }
}