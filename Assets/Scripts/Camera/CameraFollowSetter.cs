using System.Collections;
using UnityEngine;
using Unity.Cinemachine;

public class CameraFollowSetter : MonoBehaviour
{
    [Header("Camera Bounds Parent")]
    [SerializeField] private string cameraBoundsParentName = "CameraBounds";

    private IEnumerator Start()
    {
        // Đợi scene + save position load xong
        yield return null;
        yield return null;

        SetupCameraFollowAndBounds();
    }

    public void SetupCameraFollowAndBounds()
    {
        Transform player = null;

        if (PlayerSceneKeeper.Instance != null)
        {
            player = PlayerSceneKeeper.Instance.transform;
        }
        else
        {
            GameObject playerObj = GameObject.FindGameObjectWithTag("Player");
            if (playerObj != null)
            {
                player = playerObj.transform;
            }
        }

        if (player == null)
        {
            Debug.LogWarning("CameraFollowSetter: Không tìm thấy Player.");
            return;
        }

        CinemachineCamera cineCam = FindAnyObjectByType<CinemachineCamera>();

        if (cineCam != null)
        {
            cineCam.Target.TrackingTarget = player;
            cineCam.OnTargetObjectWarped(player, Vector3.zero);

            Debug.Log("CameraFollowSetter: Đã gắn CinemachineCamera Follow Player.");
        }
        else
        {
            Debug.LogWarning("CameraFollowSetter: Không tìm thấy CinemachineCamera.");
        }

        SetCameraBoundsByPlayerPosition(player.position);

        Camera mainCam = Camera.main;

        if (mainCam != null)
        {
            mainCam.transform.position = new Vector3(
                player.position.x,
                player.position.y,
                mainCam.transform.position.z
            );
        }
    }

    private void SetCameraBoundsByPlayerPosition(Vector3 playerPosition)
    {
        GameObject boundsParent = GameObject.Find(cameraBoundsParentName);

        if (boundsParent == null)
        {
            Debug.LogWarning("CameraFollowSetter: Không tìm thấy object cha CameraBounds.");
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
            Debug.LogWarning("CameraFollowSetter: Player không nằm trong CameraBounds nào tại vị trí: " + playerPosition);
            return;
        }

        CinemachineConfiner2D confiner = FindAnyObjectByType<CinemachineConfiner2D>();

        if (confiner == null)
        {
            Debug.LogWarning("CameraFollowSetter: Không tìm thấy CinemachineConfiner2D.");
            return;
        }

        confiner.BoundingShape2D = correctBounds;
        confiner.InvalidateBoundingShapeCache();

        Debug.Log("CameraFollowSetter: Đã đổi camera bounds sang: " + correctBounds.gameObject.name);
    }
}