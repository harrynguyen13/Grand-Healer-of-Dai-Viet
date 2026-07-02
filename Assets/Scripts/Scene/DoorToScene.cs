using UnityEngine;
using UnityEngine.SceneManagement;
using UnityEngine.InputSystem;

public class DoorToScene : MonoBehaviour
{
    [Header("Scene Settings")]
    [SerializeField] private string sceneToLoad = "ClinicInterior";

    [Header("Spawn Settings")]
    [SerializeField] private string targetSpawnPointName = "Spawn_From_Outside";

    [Header("Player Settings")]
    [SerializeField] private string playerTag = "Player";

    private bool playerInside = false;
    private bool isLoading = false;

    private void Update()
    {
        if (!playerInside) return;
        if (isLoading) return;

        if (Keyboard.current != null && Keyboard.current.eKey.wasPressedThisFrame)
        {
            isLoading = true;

            SceneTransitionData.isChangingScene = true;
            SceneTransitionData.targetSpawnPointName = targetSpawnPointName;

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