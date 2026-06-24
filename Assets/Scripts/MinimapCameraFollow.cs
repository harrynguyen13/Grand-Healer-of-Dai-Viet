using UnityEngine;
using UnityEngine.SceneManagement;

public class MinimapCameraFollow : MonoBehaviour
{
    [SerializeField] private Transform player;
    [SerializeField] private string playerTag = "Player";
    [SerializeField] private float cameraZ = -10f;

    private void OnEnable()
    {
        SceneManager.sceneLoaded += OnSceneLoaded;
        FindPlayer();
    }

    private void OnDisable()
    {
        SceneManager.sceneLoaded -= OnSceneLoaded;
    }

    private void OnSceneLoaded(Scene scene, LoadSceneMode mode)
    {
        FindPlayer();
    }

    private void LateUpdate()
    {
        if (player == null)
        {
            FindPlayer();
        }

        if (player == null) return;

        transform.position = new Vector3(
            player.position.x,
            player.position.y,
            cameraZ
        );
    }

    private void FindPlayer()
    {
        GameObject foundPlayer = GameObject.FindGameObjectWithTag(playerTag);

        if (foundPlayer != null)
        {
            player = foundPlayer.transform;
        }
    }
}