using System;
using System.Collections.Generic;

public enum MailType
{
    PatientSuccess,
    PatientFail,
    QuestReward,
    SystemReward
}

[Serializable]
public class MailHerbReward
{
    public string herbName;
    public int amount;
}

[Serializable]
public class MailMessage
{
    public string id;
    public MailType mailType;

    public string senderName;
    public string content;

    public long createdTicks;

    // Thời điểm người chơi đọc thư lần đầu.
    // 0 nghĩa là chưa từng đọc.
    public long readTicks;

    public int moneyDelta;
    public int reputationDelta;

    public List<MailHerbReward> herbRewards = new List<MailHerbReward>();

    public bool isRead;
    public bool isClaimed;
}

[Serializable]
public class MailSaveData
{
    public List<MailMessage> mails = new List<MailMessage>();
}