using UnityEngine;

public static class SceneTransitionData
{
    public static bool isChangingScene = false;
    public static string targetSpawnPointName = "";

    [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
    private static void ResetOnPlay()
    {
        isChangingScene = false;
        targetSpawnPointName = "";
    }
}