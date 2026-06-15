using UnityEngine;

public class SceneSpawnManager : MonoBehaviour
{
    private void Start()
    {
        if (!SceneTransitionData.isChangingScene)
        {
            Debug.Log("Không phải chuyển scene, không spawn lại Player");
            return;
        }

        if (PlayerSceneKeeper.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy PlayerSceneKeeper");
            return;
        }

        if (string.IsNullOrEmpty(SceneTransitionData.targetSpawnPointName))
        {
            Debug.LogWarning("Chưa có tên spawn point");
            return;
        }

        GameObject spawnPoint = GameObject.Find(SceneTransitionData.targetSpawnPointName);

        if (spawnPoint == null)
        {
            Debug.LogWarning("Không tìm thấy spawn point: " + SceneTransitionData.targetSpawnPointName);
            return;
        }

        PlayerSceneKeeper.Instance.transform.position = spawnPoint.transform.position;

        SceneTransitionData.isChangingScene = false;
        SceneTransitionData.targetSpawnPointName = "";

        Debug.Log("Đã đưa Player tới spawn point");
    }
}