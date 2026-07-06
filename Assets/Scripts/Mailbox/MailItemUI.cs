using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class MailItemUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text senderText;
    [SerializeField] private TMP_Text timeText;
    [SerializeField] private Image unreadDot;
    [SerializeField] private Button button;

    private MailMessage currentMail;
    private Action<MailMessage> onClicked;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.onClick.AddListener(HandleClicked);
        }
    }

    public void Setup(MailMessage mail, Action<MailMessage> clickCallback)
    {
        currentMail = mail;
        onClicked = clickCallback;

        if (senderText != null)
        {
            senderText.text = mail.senderName;
        }

        if (timeText != null)
        {
            timeText.text = FormatTime(mail.createdTicks);
        }

        if (unreadDot != null)
        {
            unreadDot.gameObject.SetActive(!mail.isRead);
        }
    }

    private void HandleClicked()
    {
        if (currentMail == null)
            return;

        onClicked?.Invoke(currentMail);
    }

    private string FormatTime(long ticks)
    {
        DateTime time = new DateTime(ticks);
        return time.ToString("dd/MM");
    }
}