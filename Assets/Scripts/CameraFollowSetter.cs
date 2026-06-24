using UnityEngine;
using Unity.Cinemachine;
using System.Collections;

public class CameraFollowSetter : MonoBehaviour
{
    private IEnumerator Start()
    {
        yield return null;
        yield return null;

        CinemachineCamera virtualCam = Object.FindAnyObjectByType<CinemachineCamera>();

        if (virtualCam == null)
        {
            Debug.LogWarning("Không tìm thấy CinemachineCamera");
            yield break;
        }

        if (PlayerSceneKeeper.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy PlayerSceneKeeper");
            yield break;
        }

        virtualCam.Follow = PlayerSceneKeeper.Instance.transform;

        // Reset trạng thái Cinemachine để nó không kéo từ vị trí cũ
        virtualCam.PreviousStateIsValid = false;

        Debug.Log("Đã gán CinemachineCamera Follow Player");
    }
}