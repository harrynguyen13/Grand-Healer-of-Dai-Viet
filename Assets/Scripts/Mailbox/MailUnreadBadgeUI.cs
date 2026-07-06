using TMPro;
using UnityEngine;

public class MailUnreadBadgeUI : MonoBehaviour
{
    [Header("Badge")]
    [SerializeField] private GameObject badgeRoot;
    [SerializeField] private TMP_Text countText;

    private void OnEnable()
    {
        if (MailboxManager.Instance != null)
        {
            MailboxManager.Instance.OnMailboxChanged += Refresh;
        }

        Refresh();
    }

    private void Start()
    {
        Refresh();
    }

    private void OnDisable()
    {
        if (MailboxManager.Instance != null)
        {
            MailboxManager.Instance.OnMailboxChanged -= Refresh;
        }
    }

    public void Refresh()
    {
        int unreadCount = 0;

        if (MailboxManager.Instance != null)
        {
            unreadCount = MailboxManager.Instance.GetUnreadCount();
        }

        if (badgeRoot != null)
        {
            badgeRoot.SetActive(unreadCount > 0);
        }

        if (countText != null)
        {
            if (unreadCount > 99)
            {
                countText.text = "99+";
            }
            else
            {
                countText.text = unreadCount.ToString();
            }
        }
    }
}