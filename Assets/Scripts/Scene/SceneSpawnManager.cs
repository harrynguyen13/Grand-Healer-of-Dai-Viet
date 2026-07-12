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

        // Đưa Player tới spawn point
        player.position = spawnPoint.transform.position;

        // Dừng toàn bộ lực / vận tốc cũ của Player
        StopPlayerImmediately(player);

        Physics2D.SyncTransforms();

        Debug.Log("Đã đưa Player tới spawn point: " + SceneTransitionData.targetSpawnPointName);

        // Đổi CameraBounds theo vị trí Player sau khi spawn
        SetCameraBoundsByPlayerPosition(player.position);

        // Gắn lại Cinemachine và ép camera đứng ngay tại Player
        SnapCameraToPlayer(player);

        // Chờ cuối frame rồi ép lại lần nữa để tránh Cinemachine update state cũ
        yield return new WaitForEndOfFrame();

        SnapCameraToPlayer(player);

        // Reset dữ liệu chuyển scene
        SceneTransitionData.isChangingScene = false;
        SceneTransitionData.targetSpawnPointName = "";
    }

    private void StopPlayerImmediately(Transform player)
    {
        if (player == null)
            return;

        BaseMove baseMove = player.GetComponent<BaseMove>();

        if (baseMove != null)
        {
            baseMove.StopImmediately();
            return;
        }

        Rigidbody2D rb = player.GetComponent<Rigidbody2D>();

        if (rb != null)
        {
            rb.linearVelocity = Vector2.zero;
            rb.angularVelocity = 0f;
        }
    }

    private void SnapCameraToPlayer(Transform player)
    {
        if (player == null)
            return;

        CinemachineCamera cineCam = FindAnyObjectByType<CinemachineCamera>();

        if (cineCam != null)
        {
            cineCam.Target.TrackingTarget = player;

            Vector3 cinePosition = cineCam.transform.position;

            cineCam.transform.position = new Vector3(
                player.position.x,
                player.position.y,
                cinePosition.z
            );

            // Dòng này rất quan trọng: bắt Cinemachine bỏ state cũ
            cineCam.PreviousStateIsValid = false;

            // Không dùng delta từ scene cũ nữa
            cineCam.OnTargetObjectWarped(player, Vector3.zero);

            Debug.Log("Đã gắn CinemachineCamera Follow Player và snap camera.");
        }
        else
        {
            Debug.LogWarning("Không tìm thấy CinemachineCamera");
        }

        Camera mainCam = Camera.main;

        if (mainCam != null)
        {
            Vector3 mainPosition = mainCam.transform.position;

            mainCam.transform.position = new Vector3(
                player.position.x,
                player.position.y,
                mainPosition.z
            );
        }

        Physics2D.SyncTransforms();
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
            if (bounds == null)
                continue;

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