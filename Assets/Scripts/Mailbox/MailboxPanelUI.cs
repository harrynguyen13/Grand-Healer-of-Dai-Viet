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
            MailboxManager.Instance.OnMailboxChanged += RefreshUI;

        RefreshUI();
    }

    private void OnDisable()
    {
        if (MailboxManager.Instance != null)
            MailboxManager.Instance.OnMailboxChanged -= RefreshUI;
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
            contentDetailText.text = mail.content;

        if (rewardDetailText != null)
            rewardDetailText.text = BuildRewardText(mail);
    }

    private string BuildRewardText(MailMessage mail)
    {
        if (mail == null)
            return "";

        List<string> sections = new List<string>();

        if (!string.IsNullOrWhiteSpace(mail.yThuUsageNote))
            sections.Add(FormatSystemNote(mail.yThuUsageNote));

        List<string> resultLines = new List<string>();

        if (mail.moneyDelta > 0)
            resultLines.Add("- Thưởng tiền: +" + mail.moneyDelta + " xu");
        else if (mail.moneyDelta < 0)
            resultLines.Add("- Trừ tiền: " + mail.moneyDelta + " xu");

        if (mail.reputationDelta > 0)
            resultLines.Add("- Cộng tín nhiệm: +" + mail.reputationDelta);
        else if (mail.reputationDelta < 0)
            resultLines.Add("- Trừ tín nhiệm: " + mail.reputationDelta);

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

                resultLines.Add(
                    "- Dược liệu: +" + herbReward.amount + " " + herbReward.herbName
                );
            }
        }

        if (resultLines.Count > 0)
            sections.Add("<b>Kết Quả</b>\n" + string.Join("\n", resultLines));

        if (sections.Count == 0)
            return "Không có thưởng/phạt.";

        return string.Join("\n\n", sections);
    }

    private string FormatSystemNote(string rawNote)
    {
        string note = rawNote.Trim();

        note = note
            .Replace("\n<b>Nhắc Nhở Từ Hệ Thống</b>", "")
            .Replace("<b>Tên bệnh đúng:</b>", "- <b>Tên bệnh đúng:</b>")
            .Replace("<b>Tên bệnh đã chọn:</b>", "- <b>Tên bệnh đã chọn:</b>")
            .Replace("<b>Dược liệu cần:</b>", "- <b>Dược liệu cần:</b>")
            .Replace("<b>Thiếu:</b>", "- <b>Thiếu:</b>")
            .Replace("<b>Thừa:</b>", "- <b>Thừa:</b>")
            .Replace(
                "(Chú ý:",
                "\n\n<b>Ghi Chú Y Thư</b>\n-"
            )
            .Trim();

        if (note.EndsWith(")"))
            note = note.Substring(0, note.Length - 1);

        return "<b>Nhắc Nhở Từ Hệ Thống</b>\n" + note;
    }

    private void ClearDetail()
    {
        if (contentDetailText != null)
            contentDetailText.text = "Chọn một thư để xem nội dung.";

        if (rewardDetailText != null)
            rewardDetailText.text = "";
    }

    private void ClearChildren(Transform root)
    {
        if (root == null)
            return;

        for (int i = root.childCount - 1; i >= 0; i--)
            Destroy(root.GetChild(i).gameObject);
    }
}