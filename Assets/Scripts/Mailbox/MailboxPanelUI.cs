using System.Collections.Generic;
using TMPro;
using UnityEngine;

public class MailboxPanelUI : MonoBehaviour
{
    [Header("List bên trái")]
    [SerializeField] private Transform mailListContent;
    [SerializeField] private MailItemUI mailItemPrefab;

    [Header("Chi tiết bên phải")]
    [SerializeField] private TMP_Text contentDetailText;
    [SerializeField] private TMP_Text rewardDetailText;

    private void OnEnable()
    {
        if (MailboxManager.Instance != null)
        {
            MailboxManager.Instance.OnMailboxChanged += RefreshUI;
        }

        RefreshUI();
    }

    private void OnDisable()
    {
        if (MailboxManager.Instance != null)
        {
            MailboxManager.Instance.OnMailboxChanged -= RefreshUI;
        }
    }

    public void RefreshUI()
    {
        BuildMailList();
        ClearDetail();
    }

    private void BuildMailList()
    {
        if (mailListContent == null || mailItemPrefab == null)
            return;

        ClearChildren(mailListContent);

        if (MailboxManager.Instance == null)
            return;

        List<MailMessage> mails = MailboxManager.Instance.GetAllMails();

        foreach (MailMessage mail in mails)
        {
            MailItemUI item = Instantiate(mailItemPrefab, mailListContent);
            item.Setup(mail, OnMailClicked);
        }
    }

    private void OnMailClicked(MailMessage mail)
    {
        if (mail == null || MailboxManager.Instance == null)
            return;

        MailMessage openedMail = MailboxManager.Instance.OpenMailAndClaim(mail.id);

        if (openedMail == null)
            return;

        ShowMailDetail(openedMail);
        BuildMailList();
    }

    private void ShowMailDetail(MailMessage mail)
    {
        if (mail == null)
            return;

        if (contentDetailText != null)
        {
            contentDetailText.text = mail.content;
        }

        if (rewardDetailText != null)
        {
            rewardDetailText.text = BuildRewardText(mail);
        }
    }

    private string BuildRewardText(MailMessage mail)
    {
        if (mail == null)
            return "";

        List<string> lines = new List<string>();

        if (!string.IsNullOrWhiteSpace(mail.yThuUsageNote))
        {
            lines.Add(mail.yThuUsageNote);
            lines.Add("");
        }

        if (mail.moneyDelta > 0)
        {
            lines.Add("Thưởng tiền: +" + mail.moneyDelta + " xu");
        }
        else if (mail.moneyDelta < 0)
        {
            lines.Add("Phạt tiền: " + mail.moneyDelta + " xu");
        }

        if (mail.reputationDelta > 0)
        {
            lines.Add("Thưởng tín nhiệm: +" + mail.reputationDelta);
        }
        else if (mail.reputationDelta < 0)
        {
            lines.Add("Phạt tín nhiệm: " + mail.reputationDelta);
        }

        if (mail.herbRewards != null)
        {
            foreach (MailHerbReward herbReward in mail.herbRewards)
            {
                if (herbReward == null)
                    continue;

                if (string.IsNullOrWhiteSpace(herbReward.herbName))
                    continue;

                if (herbReward.amount <= 0)
                    continue;

                lines.Add("Dược liệu: +" + herbReward.amount + " " + herbReward.herbName);
            }
        }

        if (lines.Count == 0)
        {
            return "Không có thưởng/phạt.";
        }

        return string.Join("\n", lines).TrimEnd();
    }

    private void ClearDetail()
    {
        if (contentDetailText != null)
        {
            contentDetailText.text = "Chọn một thư để xem nội dung.";
        }

        if (rewardDetailText != null)
        {
            rewardDetailText.text = "";
        }
    }

    private void ClearChildren(Transform root)
    {
        if (root == null)
            return;

        for (int i = root.childCount - 1; i >= 0; i--)
        {
            Destroy(root.GetChild(i).gameObject);
        }
    }
}