using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class MapTeleportTrigger : MonoBehaviour
{
    [Header("Điểm player sẽ được dịch chuyển tới")]
    [SerializeField] private Transform targetSpawnPoint;

    [Header("Camera Bounds của map đích")]
    [SerializeField] private Collider2D targetCameraBounds;

    [Header("Cinemachine Confiner 2D")]
    [SerializeField] private CinemachineConfiner2D cinemachineConfiner;

    [Header("Tag của Player")]
    [SerializeField] private string playerTag = "Player";

    [Header("Chống teleport liên tục")]
    [SerializeField] private float teleportCooldown = 0.3f;

    private static bool isTeleporting = false;

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (isTeleporting) return;
        if (!other.CompareTag(playerTag)) return;

        if (targetSpawnPoint == null)
        {
            Debug.LogWarning($"{name}: Chưa gán Target Spawn Point");
            return;
        }

        StartCoroutine(TeleportPlayer(other));
    }

    private IEnumerator TeleportPlayer(Collider2D playerCollider)
    {
        isTeleporting = true;

        Rigidbody2D rb = playerCollider.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
            rb.position = targetSpawnPoint.position;
        }
        else
        {
            playerCollider.transform.position = targetSpawnPoint.position;
        }

        if (cinemachineConfiner != null && targetCameraBounds != null)
        {
            cinemachineConfiner.BoundingShape2D = targetCameraBounds;
            cinemachineConfiner.InvalidateBoundingShapeCache();
        }

        yield return new WaitForSeconds(teleportCooldown);

        isTeleporting = false;
    }
}