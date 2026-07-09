using System.Collections.Generic;
using UnityEngine;

public static class PlayerControlLock
{
    private static readonly HashSet<string> lockReasons = new HashSet<string>();

    public static bool IsLocked
    {
        get
        {
            return lockReasons.Count > 0;
        }
    }

    public static void Lock(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            reason = "Unknown";

        lockReasons.Add(reason);

        ForceStopPlayer();

        Debug.Log("Đã khóa điều khiển Player: " + reason);
    }

    public static void Unlock(string reason)
    {
        if (string.IsNullOrWhiteSpace(reason))
            reason = "Unknown";

        if (lockReasons.Contains(reason))
        {
            lockReasons.Remove(reason);
        }

        ForceStopPlayer();

        Debug.Log("Đã mở khóa điều khiển Player: " + reason);
    }

    public static void UnlockAll()
    {
        lockReasons.Clear();

        ForceStopPlayer();

        Debug.Log("Đã mở toàn bộ khóa điều khiển Player.");
    }

    private static void ForceStopPlayer()
    {
        GameObject playerObj = GameObject.FindGameObjectWithTag("Player");

        if (playerObj == null)
            return;

        Rigidbody2D rb2d = playerObj.GetComponent<Rigidbody2D>();

        if (rb2d != null)
        {
            rb2d.linearVelocity = Vector2.zero;
            rb2d.angularVelocity = 0f;
        }

        playerObj.SendMessage(
            "ForceStopMovement",
            SendMessageOptions.DontRequireReceiver
        );
    }
}