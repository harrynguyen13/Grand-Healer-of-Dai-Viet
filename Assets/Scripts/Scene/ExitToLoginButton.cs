using UnityEngine;
using UnityEngine.SceneManagement;

public class ExitToLoginButton : MonoBehaviour
{
    [Header("Scene")]
    [SerializeField] private string loginSceneName = "LoginScene";

    [Header("Root UI cần tắt trước khi về Login")]
    [SerializeField] private GameObject settingsUiRoot;

    private bool isExiting = false;

    private void OnEnable()
    {
        isExiting = false;
    }

    public void GoToLoginScene()
    {
        if (isExiting)
            return;

        isExiting = true;

        if (LocalSaveManager.Instance != null)
        {
            LocalSaveManager.Instance.SaveGame();
            Debug.Log("Đã lưu game trước khi quay về LoginScene.");
        }
        else
        {
            Debug.LogWarning("Không tìm thấy LocalSaveManager để lưu game trước khi về LoginScene.");
        }

        if (settingsUiRoot == null)
        {
            SettingsTabController settingsTabController = GetComponentInParent<SettingsTabController>(true);

            if (settingsTabController != null)
            {
                settingsUiRoot = settingsTabController.gameObject;
            }
        }

        if (settingsUiRoot != null)
        {
            settingsUiRoot.SetActive(false);
        }

        SceneManager.LoadScene(loginSceneName);
    }
}