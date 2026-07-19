using UnityEngine;
using UnityEngine.InputSystem;

public class HerbGardenPlantStation : MonoBehaviour
{
    [Header("Phím mở UI chọn cây")]
    [SerializeField] private Key interactKey = Key.E;

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
            OpenPlantSelectionUI();
        }
    }

    private void OpenPlantSelectionUI()
    {
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

        if (GardenPlantSelectionUI.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy GardenPlantSelectionUI trong scene.");
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
    }
}