using UnityEngine;

public class SceneSpawnManager : MonoBehaviour
{
    [SerializeField] private Transform defaultSpawnPoint;

    private void Start()
    {
        if (PlayerSceneKeeper.Instance == null) return;

        if (defaultSpawnPoint != null)
        {
            PlayerSceneKeeper.Instance.transform.position = defaultSpawnPoint.position;
        }
    }
}