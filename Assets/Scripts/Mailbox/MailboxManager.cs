using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

public class MailboxManager : MonoBehaviour
{
    public static MailboxManager Instance { get; private set; }

    public event Action OnMailboxChanged;

    [Header("Database dược liệu")]
    [SerializeField] private MedicalDatabase medicalDatabase;

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    [SerializeField] private List<MailMessage> mails = new List<MailMessage>();

    private string SavePath
    {
        get
        {
            return Path.Combine(Application.persistentDataPath, "mailbox_save.json");
        }
    }

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);

        LoadMailbox();
        CleanupExpiredReadMails(false);
    }

    public List<MailMessage> GetAllMails()
    {
        CleanupExpiredReadMails(true);

        List<MailMessage> result = new List<MailMessage>(mails);
        result.Sort((a, b) => b.createdTicks.CompareTo(a.createdTicks));

        return result;
    }

    public int GetUnreadCount()
    {
        CleanupExpiredReadMails(true);

        int count = 0;

        foreach (MailMessage mail in mails)
        {
            if (mail != null && !mail.isRead)
            {
                count++;
            }
        }

        return count;
    }

    public void AddPatientTreatmentMail(
        string patientName,
        string diseaseName,
        bool diagnosisCorrect,
        bool prescriptionCorrect,
        int moneyDelta,
        int reputationDelta
    )
    {
        MailType mailType;
        string content;

        if (diagnosisCorrect && prescriptionCorrect)
        {
            mailType = MailType.PatientSuccess;
            content =
                "Thưa lương y,\n\n" +
                "Từ hôm được thầy xem bệnh và bốc thuốc, người tôi đã nhẹ nhõm hơn nhiều. Những cơn khó chịu trước đó cũng dần lui đi.\n\n" +
                "Nhà tôi không có gì quý, chỉ xin gửi chút lễ mọn gọi là tỏ lòng biết ơn. Mong thầy nhận cho.";
        }
        else if (diagnosisCorrect && !prescriptionCorrect)
        {
            mailType = MailType.PatientFail;
            content =
                "Thưa thầy,\n\n" +
                "Bệnh mà thầy nói xem ra không sai, nhưng thang thuốc vừa rồi uống vào vẫn chưa thấy chuyển biến rõ. Người tôi vẫn còn mệt, bệnh chưa lui hẳn.\n\n" +
                "Tôi vẫn kính trọng tay nghề của thầy, chỉ mong lần sau thầy cân nhắc đơn thuốc kỹ hơn.";
        }
        else if (!diagnosisCorrect && prescriptionCorrect)
        {
            mailType = MailType.PatientFail;
            content =
                "Thưa thầy,\n\n" +
                "Uống thuốc xong tôi có thấy dễ chịu hơn đôi chút, nhưng nghĩ lại thì bệnh tình của tôi hình như không đúng như lời thầy chẩn đoán.\n\n" +
                "Chuyện này khiến tôi hơi bất an. Mong thầy sau này xem xét cẩn trọng hơn, kẻo người bệnh trong làng mất lòng tin.";
        }
        else
        {
            mailType = MailType.PatientFail;
            content =
                "Thưa thầy,\n\n" +
                "Tôi đã dùng thuốc theo đơn, nhưng bệnh không những chẳng đỡ mà trong người còn thêm phần khó chịu. Lần chữa này thật khiến tôi thất vọng.\n\n" +
                "Tôi e rằng chuyện này truyền ra ngoài sẽ làm ảnh hưởng đến tiếng tăm của y quán.";
        }

        AddMail(
            mailType,
            patientName,
            content,
            moneyDelta,
            reputationDelta
        );
    }

    public void AddQuestRewardMail(
        string questName,
        int moneyDelta,
        int reputationDelta
    )
    {
        AddQuestRewardMail(
            questName,
            moneyDelta,
            reputationDelta,
            null
        );
    }

    public void AddQuestRewardMail(
        string questName,
        int moneyDelta,
        int reputationDelta,
        List<MailHerbReward> herbRewards
    )
    {
        string content =
            "Y quán ghi nhận công việc đã hoàn thành.\n\n" +
            "Nhiệm vụ \"" + questName + "\" đã được xử lý ổn thỏa.";

        AddMail(
            MailType.QuestReward,
            "Hệ thống nhiệm vụ",
            content,
            moneyDelta,
            reputationDelta,
            herbRewards
        );
    }

    public void AddMail(
        MailType mailType,
        string senderName,
        string content,
        int moneyDelta,
        int reputationDelta,
        List<MailHerbReward> herbRewards = null
    )
    {
        MailMessage mail = new MailMessage();

        mail.id = Guid.NewGuid().ToString();
        mail.mailType = mailType;
        mail.senderName = senderName;
        mail.content = content;
        mail.createdTicks = DateTime.Now.Ticks;
        mail.readTicks = 0;
        mail.moneyDelta = moneyDelta;
        mail.reputationDelta = reputationDelta;
        mail.herbRewards = herbRewards != null ? herbRewards : new List<MailHerbReward>();
        mail.isRead = false;
        mail.isClaimed = false;

        mails.Add(mail);

        SaveMailbox();

        if (logDebug)
        {
            Debug.Log("Đã tạo thư mới từ: " + senderName);
        }

        OnMailboxChanged?.Invoke();
    }

    public MailMessage OpenMailAndClaim(string mailId)
    {
        MailMessage mail = FindMail(mailId);

        if (mail == null)
        {
            Debug.LogWarning("Không tìm thấy thư: " + mailId);
            return null;
        }

        if (!mail.isRead)
        {
            mail.isRead = true;
            mail.readTicks = DateTime.Now.Ticks;
        }
        else if (mail.readTicks <= 0)
        {
            mail.readTicks = DateTime.Now.Ticks;
        }

        if (!mail.isClaimed)
        {
            ApplyMailRewardOrPenalty(mail);
            mail.isClaimed = true;
        }

        CleanupExpiredReadMails(false);

        SaveMailbox();
        OnMailboxChanged?.Invoke();

        return mail;
    }

    private void ApplyMailRewardOrPenalty(MailMessage mail)
    {
        if (mail == null)
            return;

        ApplyMoneyAndReputation(mail);
        ApplyHerbRewards(mail);

        Debug.Log(
            "Đã nhận thư. Tiền: "
            + mail.moneyDelta
            + " | Tín nhiệm: "
            + mail.reputationDelta
        );
    }

    private void ApplyMoneyAndReputation(MailMessage mail)
    {
        if (mail.moneyDelta == 0 && mail.reputationDelta == 0)
            return;

        if (PlayerEconomy.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy PlayerEconomy để nhận thưởng/phạt từ thư.");
            return;
        }

        if (mail.moneyDelta > 0)
        {
            PlayerEconomy.Instance.AddMoney(mail.moneyDelta);
        }
        else if (mail.moneyDelta < 0)
        {
            PlayerEconomy.Instance.SpendMoney(Mathf.Abs(mail.moneyDelta));
        }

        if (mail.reputationDelta != 0)
        {
            PlayerEconomy.Instance.AddReputation(mail.reputationDelta);
        }
    }

    private void ApplyHerbRewards(MailMessage mail)
    {
        if (mail.herbRewards == null || mail.herbRewards.Count == 0)
            return;

        if (HerbInventory.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy HerbInventory để nhận dược liệu từ thư.");
            return;
        }

        foreach (MailHerbReward herbReward in mail.herbRewards)
        {
            if (herbReward == null)
                continue;

            if (string.IsNullOrWhiteSpace(herbReward.herbName))
                continue;

            if (herbReward.amount <= 0)
                continue;

            HerbData herb = FindHerbByName(herbReward.herbName);

            if (herb == null)
            {
                Debug.LogWarning("Không tìm thấy dược liệu trong mail: " + herbReward.herbName);
                continue;
            }

            HerbInventory.Instance.AddHerb(herb, herbReward.amount);

            Debug.Log(
                "Đã nhận dược liệu từ thư: "
                + herb.herbName
                + " x"
                + herbReward.amount
            );
        }
    }

    private HerbData FindHerbByName(string herbName)
    {
        if (medicalDatabase == null)
        {
            Debug.LogWarning("MailboxManager chưa kéo MedicalDatabase.");
            return null;
        }

        List<HerbData> herbs = medicalDatabase.GetUnlockedHerbs(5);

        if (herbs == null || herbs.Count == 0)
            return null;

        string targetName = NormalizeName(herbName);

        foreach (HerbData herb in herbs)
        {
            if (herb == null)
                continue;

            if (NormalizeName(herb.herbName) == targetName)
            {
                return herb;
            }
        }

        return null;
    }

    private string NormalizeName(string text)
    {
        if (string.IsNullOrWhiteSpace(text))
            return "";

        return text.Trim().ToLower();
    }

    private MailMessage FindMail(string mailId)
    {
        foreach (MailMessage mail in mails)
        {
            if (mail != null && mail.id == mailId)
            {
                return mail;
            }
        }

        return null;
    }

    public void ClearAllMails()
    {
        mails.Clear();
        SaveMailbox();
        OnMailboxChanged?.Invoke();
    }

    private void CleanupExpiredReadMails(bool notifyIfChanged)
    {
        long expiredBeforeTicks = DateTime.Now.AddDays(-7).Ticks;
        int removedCount = 0;

        for (int i = mails.Count - 1; i >= 0; i--)
        {
            MailMessage mail = mails[i];

            if (mail == null)
            {
                mails.RemoveAt(i);
                removedCount++;
                continue;
            }

            if (!mail.isRead)
                continue;

            if (mail.readTicks <= 0)
                continue;

            if (mail.readTicks <= expiredBeforeTicks)
            {
                mails.RemoveAt(i);
                removedCount++;
            }
        }

        if (removedCount > 0)
        {
            SaveMailbox();

            if (logDebug)
            {
                Debug.Log("Đã tự xóa " + removedCount + " thư đã đọc quá 7 ngày.");
            }

            if (notifyIfChanged)
            {
                OnMailboxChanged?.Invoke();
            }
        }
    }

    private void SaveMailbox()
    {
        MailSaveData saveData = new MailSaveData();
        saveData.mails = mails;

        string json = JsonUtility.ToJson(saveData, true);
        File.WriteAllText(SavePath, json);
    }

    private void LoadMailbox()
    {
        mails.Clear();

        if (!File.Exists(SavePath))
        {
            return;
        }

        string json = File.ReadAllText(SavePath);

        if (string.IsNullOrWhiteSpace(json))
        {
            return;
        }

        MailSaveData saveData = JsonUtility.FromJson<MailSaveData>(json);

        if (saveData != null && saveData.mails != null)
        {
            mails = saveData.mails;

            foreach (MailMessage mail in mails)
            {
                if (mail == null)
                    continue;

                if (mail.herbRewards == null)
                {
                    mail.herbRewards = new List<MailHerbReward>();
                }

                if (mail.isRead && mail.readTicks <= 0)
                {
                    mail.readTicks = DateTime.Now.Ticks;
                }
            }
        }
    }

    private void OnApplicationQuit()
    {
        SaveMailbox();
    }
}