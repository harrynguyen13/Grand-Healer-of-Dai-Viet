using UnityEngine;

public class PlayerControlLockByPanel : MonoBehaviour
{
    [Header("Khóa Player khi panel này bật")]
    [SerializeField] private string lockReason = "UI Panel";

    private void OnEnable()
    {
        PlayerControlLock.Lock(lockReason);
    }

    private void OnDisable()
    {
        PlayerControlLock.Unlock(lockReason);
    }

    private void OnDestroy()
    {
        PlayerControlLock.Unlock(lockReason);
    }
}