using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class DoorToScene : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad = "ClinicInterior";

    [Header("Player Settings")]
    [SerializeField] private string playerTag = "Player";

    private bool playerInside = false;

    private void Update()
    {
        if (!playerInside) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            SceneManager.LoadScene(sceneToLoad);
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInside = true;
            Debug.Log("Nhấn E để chuyển cảnh");
        }
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (other.CompareTag(playerTag))
        {
            playerInside = false;
        }
    }
}