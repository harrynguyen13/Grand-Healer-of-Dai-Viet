using UnityEngine;

public class SettingsTabController : MonoBehaviour
{
    [Header("Panels")]
    [SerializeField] private GameObject settingsPanel;
    [SerializeField] private GameObject guidePanel;

    private void OnEnable()
    {
        ShowGeneralTab();
    }

    public void ShowGeneralTab()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(true);
        }

        if (guidePanel != null)
        {
            guidePanel.SetActive(false);
        }
    }

    public void ShowGuideTab()
    {
        if (settingsPanel != null)
        {
            settingsPanel.SetActive(false);
        }

        if (guidePanel != null)
        {
            guidePanel.SetActive(true);
        }
    }

    public void CloseWindow()
    {
        gameObject.SetActive(false);
    }
}