using UnityEngine;
using UnityEngine.InputSystem;

public class HerbGardenPlantStation : MonoBehaviour
{
    [Header("Phím mở / đóng UI chọn cây")]
    [SerializeField] private Key interactKey = Key.Q;

    [Header("Tag người chơi")]
    [SerializeField] private string playerTag = "Player";

    [Header("Cấp tối thiểu để mở vườn")]
    [SerializeField] private int requiredUnlockLevel = 2;

    private bool isPlayerInRange;

    private void Update()
    {
        if (!isPlayerInRange)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current[interactKey].wasPressedThisFrame)
        {
            TogglePlantSelectionUI();
        }
    }

    private void TogglePlantSelectionUI()
    {
        if (GardenPlantSelectionUI.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy GardenPlantSelectionUI trong scene.");
            return;
        }

        if (GardenPlantSelectionUI.Instance.IsOpen)
        {
            GardenPlantSelectionUI.Instance.Close();
            Debug.Log("Đã đóng UI chọn cây trồng.");
            return;
        }

        int currentUnlockLevel = PlayerLevelService.GetCurrentUnlockLevel();

        if (currentUnlockLevel < requiredUnlockLevel)
        {
            Debug.Log(
                "Vườn thuốc chưa mở khóa. Cần đạt cấp "
                + requiredUnlockLevel
                + " - "
                + PlayerLevelService.GetRankNameByStage(requiredUnlockLevel)
                + " để bắt đầu trồng dược liệu."
            );

            return;
        }

        GardenPlantSelectionUI.Instance.Open();

        Debug.Log("Đã mở UI chọn cây trồng.");
    }

    private void OnTriggerEnter2D(Collider2D collision)
    {
        if (!collision.CompareTag(playerTag))
            return;

        isPlayerInRange = true;
    }

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag(playerTag))
            return;

        isPlayerInRange = false;

        if (GardenPlantSelectionUI.Instance != null && GardenPlantSelectionUI.Instance.IsOpen)
        {
            GardenPlantSelectionUI.Instance.Close();
        }
    }
}