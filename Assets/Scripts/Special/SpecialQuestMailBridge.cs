using UnityEngine;

public class SpecialQuestMailBridge : MonoBehaviour
{
    public static SpecialQuestMailBridge Instance { get; private set; }

    [Header("Mail nhiệm vụ Quan Huyện")]
    [SerializeField] private string mailKey = "SPECIAL_QUEST_QUAN_HUYEN_START_MAIL_SENT";

    [SerializeField] private string senderName = "Phủ huyện";

    [TextArea(5, 12)]
    [SerializeField] private string mailContent =
        "Nghe danh ngươi là một thầy thuốc giỏi có tiếng.\n\n" +
        "Hiện tại, Quan Huyện đang lâm trọng bệnh, hơi thở suy yếu, thần trí mê sảng, các thầy thuốc trong vùng đều chưa tìm ra cách chữa.\n\n" +
        "Phủ huyện truyền gọi lương y đến chẩn khám ngay. Hãy đến phủ huyện để xem bệnh cho Quan Huyện.";

    [Header("Debug")]
    [SerializeField] private bool logDebug = true;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;
        DontDestroyOnLoad(gameObject);
    }

    public void SendQuanHuyenQuestMailOnce()
    {
        if (HasSentMail())
        {
            if (logDebug)
                Debug.Log("SpecialQuestMailBridge: Mail Quan Huyện đã gửi trước đó.");

            return;
        }

        if (MailboxManager.Instance == null)
        {
            Debug.LogWarning("SpecialQuestMailBridge: Không tìm thấy MailboxManager.");
            return;
        }

        MailboxManager.Instance.AddMail(
            MailType.QuestReward,
            senderName,
            mailContent,
            0,
            0,
            null,
            ""
        );

        MarkMailAsSent();

        if (logDebug)
            Debug.Log("SpecialQuestMailBridge: Đã gửi mail nhiệm vụ Quan Huyện.");
    }

    public bool HasSentMail()
    {
        return PlayerPrefs.GetInt(mailKey, 0) == 1;
    }

    private void MarkMailAsSent()
    {
        PlayerPrefs.SetInt(mailKey, 1);
        PlayerPrefs.Save();
    }

    public void ResetQuanHuyenQuestMail()
    {
        PlayerPrefs.DeleteKey(mailKey);
        PlayerPrefs.Save();

        if (logDebug)
            Debug.Log("SpecialQuestMailBridge: Đã reset mail nhiệm vụ Quan Huyện.");
    }

    [ContextMenu("DEBUG - Send Quan Huyen Mail")]
    private void DebugSendQuanHuyenMail()
    {
        SendQuanHuyenQuestMailOnce();
    }

    [ContextMenu("DEBUG - Reset Quan Huyen Mail")]
    private void DebugResetQuanHuyenMail()
    {
        ResetQuanHuyenQuestMail();
    }
}