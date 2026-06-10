using UnityEngine;

public class PlayerSceneKeeper : MonoBehaviour
{
    public static PlayerSceneKeeper Instance;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }
}