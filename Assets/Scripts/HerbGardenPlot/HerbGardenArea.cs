using UnityEngine;

public class HerbGardenArea : MonoBehaviour
{
    [Header("Tag người chơi")]
    [SerializeField] private string playerTag = "Player";

    [Header("Tùy chọn")]
    [SerializeField] private bool closePlantPanelWhenExit = true;
    [SerializeField] private bool clearSelectedPlantWhenExit = true;

    private void OnTriggerExit2D(Collider2D collision)
    {
        if (!collision.CompareTag(playerTag))
            return;

        if (GardenPlantSelectionUI.Instance == null)
            return;

        if (closePlantPanelWhenExit)
        {
            GardenPlantSelectionUI.Instance.Close();
        }

        if (clearSelectedPlantWhenExit)
        {
            GardenPlantSelectionUI.Instance.ClearSelectedPlant();
        }

        Debug.Log("Player rời khỏi vườn thuốc. Đã hủy cây đang chọn.");
    }
}