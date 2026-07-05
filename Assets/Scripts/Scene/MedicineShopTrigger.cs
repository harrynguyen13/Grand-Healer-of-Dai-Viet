using UnityEngine;
using UnityEngine.InputSystem;

public class MedicineShopTrigger : MonoBehaviour
{
    [Header("Shop UI")]
    [SerializeField] private GameObject medicineShopPanel;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    private bool playerInRange = false;

    private void Start()
    {
        if (medicineShopPanel != null)
        {
            medicineShopPanel.SetActive(false);
        }
    }

    private void Update()
    {
        if (!playerInRange)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current.eKey.wasPressedThisFrame)
        {
            OpenShop();
        }
    }

    private void OpenShop()
    {
        if (medicineShopPanel == null)
        {
            Debug.LogWarning("Chưa gán MedicineShopPanel.");
            return;
        }

        medicineShopPanel.SetActive(true);

        Debug.Log("Đã mở UI mua thuốc.");
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = true;
            Debug.Log("Đứng gần quầy thuốc. Nhấn E để mua thuốc.");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInRange = false;
        }
    }
}