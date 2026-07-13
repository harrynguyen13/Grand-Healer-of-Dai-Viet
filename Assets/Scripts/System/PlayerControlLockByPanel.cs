using UnityEngine;

public class PlayerControlLockByPanel : MonoBehaviour
{
    [Header("Khóa Player khi panel này hiện")]
    [SerializeField] private string lockReason = "UI Panel";

    [Header("Kiểm tra CanvasGroup")]
    [SerializeField] private bool unlockWhenCanvasGroupHidden = true;

    private bool hasLocked;

    private void OnEnable()
    {
        RefreshLockState();
    }

    private void LateUpdate()
    {
        RefreshLockState();
    }

    private void OnDisable()
    {
        UnlockIfNeeded();
    }

    private void OnDestroy()
    {
        UnlockIfNeeded();
    }

    public void RefreshLockState()
    {
        if (ShouldLockPlayer())
        {
            LockIfNeeded();
        }
        else
        {
            UnlockIfNeeded();
        }
    }

    public void ForceUnlock()
    {
        UnlockIfNeeded();
    }

    public void ForceLock()
    {
        LockIfNeeded();
    }

    private bool ShouldLockPlayer()
    {
        if (!isActiveAndEnabled)
            return false;

        if (!gameObject.activeInHierarchy)
            return false;

        if (!unlockWhenCanvasGroupHidden)
            return true;

        return IsVisibleByCanvasGroup();
    }

    private bool IsVisibleByCanvasGroup()
    {
        CanvasGroup[] canvasGroups = GetComponentsInParent<CanvasGroup>(true);

        if (canvasGroups == null || canvasGroups.Length == 0)
            return true;

        for (int i = 0; i < canvasGroups.Length; i++)
        {
            if (canvasGroups[i] == null)
                continue;

            if (canvasGroups[i].alpha <= 0.01f)
                return false;
        }

        return true;
    }

    private void LockIfNeeded()
    {
        if (hasLocked)
            return;

        hasLocked = true;
        PlayerControlLock.Lock(lockReason);
    }

    private void UnlockIfNeeded()
    {
        if (!hasLocked)
            return;

        hasLocked = false;
        PlayerControlLock.Unlock(lockReason);
    }
}