using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class SceneSpawnManager : MonoBehaviour
{
    [Header("Camera Bounds Parent")]
    [SerializeField] private string cameraBoundsParentName = "CameraBounds";

    private void Start()
    {
        if (!SceneTransitionData.isChangingScene)
        {
            Debug.Log("Không phải chuyển scene, không spawn lại Player");
            return;
        }

        StartCoroutine(SpawnPlayerAfterSceneLoaded());
    }

    private IEnumerator SpawnPlayerAfterSceneLoaded()
    {
        // Đợi 1 frame để scene mới load xong hết object
        yield return null;

        if (PlayerSceneKeeper.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy PlayerSceneKeeper");
            yield break;
        }

        if (string.IsNullOrEmpty(SceneTransitionData.targetSpawnPointName))
        {
            Debug.LogWarning("Chưa có tên spawn point");
            yield break;
        }

        GameObject spawnPoint = GameObject.Find(SceneTransitionData.targetSpawnPointName);

        if (spawnPoint == null)
        {
            Debug.LogWarning("Không tìm thấy spawn point: " + SceneTransitionData.targetSpawnPointName);
            yield break;
        }

        Transform player = PlayerSceneKeeper.Instance.transform;

        Vector3 oldPosition = player.position;
        Vector3 newPosition = spawnPoint.transform.position;
        Vector3 delta = newPosition - oldPosition;

        // Đưa Player tới spawn point
        player.position = newPosition;

        Rigidbody2D rb = PlayerSceneKeeper.Instance.GetComponent<Rigidbody2D>();
        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }

        Debug.Log("Đã đưa Player tới spawn point");

        // Đổi CameraBounds theo vị trí Player sau khi spawn
        SetCameraBoundsByPlayerPosition(player.position);

        // Gắn lại Cinemachine follow Player
        CinemachineCamera cineCam = FindAnyObjectByType<CinemachineCamera>();

        if (cineCam != null)
        {
            cineCam.Target.TrackingTarget = player;

            // Báo cho Cinemachine biết Player vừa bị teleport
            cineCam.OnTargetObjectWarped(player, delta);

            Debug.Log("Đã gắn CinemachineCamera Follow Player");
        }
        else
        {
            Debug.LogWarning("Không tìm thấy CinemachineCamera");
        }

        // Đợi thêm 1 frame để Cinemachine cập nhật lại
        yield return null;

        Camera mainCam = Camera.main;

        if (mainCam != null)
        {
            mainCam.transform.position = new Vector3(
                player.position.x,
                player.position.y,
                mainCam.transform.position.z
            );
        }

        // Reset dữ liệu chuyển scene
        SceneTransitionData.isChangingScene = false;
        SceneTransitionData.targetSpawnPointName = "";
    }

    private void SetCameraBoundsByPlayerPosition(Vector3 playerPosition)
    {
        GameObject boundsParent = GameObject.Find(cameraBoundsParentName);

        if (boundsParent == null)
        {
            Debug.LogWarning("Không tìm thấy object cha CameraBounds");
            return;
        }

        Collider2D[] allBounds = boundsParent.GetComponentsInChildren<Collider2D>(true);

        Collider2D correctBounds = null;

        foreach (Collider2D bounds in allBounds)
        {
            if (bounds == null) continue;

            if (bounds.OverlapPoint(playerPosition))
            {
                correctBounds = bounds;
                break;
            }
        }

        if (correctBounds == null)
        {
            Debug.LogWarning("Player không nằm trong CameraBounds nào tại vị trí: " + playerPosition);
            return;
        }

        CinemachineConfiner2D confiner = FindAnyObjectByType<CinemachineConfiner2D>();

        if (confiner == null)
        {
            Debug.LogWarning("Không tìm thấy CinemachineConfiner2D");
            return;
        }

        confiner.BoundingShape2D = correctBounds;
        confiner.InvalidateBoundingShapeCache();

        Debug.Log("Đã đổi camera bounds sang: " + correctBounds.gameObject.name);
    }
}