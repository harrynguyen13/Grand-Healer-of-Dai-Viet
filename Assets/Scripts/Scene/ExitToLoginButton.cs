using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitToLoginButton : MonoBehaviour
{
    [SerializeField] private string loginSceneName = "LoginScene";

    [Header("Root UI cần tắt trước khi về Login")]
    [SerializeField] private GameObject settingsUIRoot;

    public void GoToLoginScene()
    {
        Time.timeScale = 1f;

        if (settingsUIRoot != null)
        {
            settingsUIRoot.SetActive(false);
        }

        SceneManager.LoadScene(loginSceneName);
    }
}