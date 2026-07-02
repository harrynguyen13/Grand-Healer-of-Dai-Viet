using TMPro;
using UnityEngine;
using UnityEngine.InputSystem;

public class QuestPanelUI : MonoBehaviour
{
    [Header("Bảng nhiệm vụ")]
    [SerializeField] private GameObject questPanel;

    [Header("Text nội dung nhiệm vụ")]
    [SerializeField] private TMP_Text questContentText;

    [Header("Phím bật / tắt bảng nhiệm vụ")]
    [SerializeField] private Key toggleKey = Key.X;

    private bool isOpen;

    private void Start()
    {
        if (questPanel != null)
            questPanel.SetActive(false);

        isOpen = false;

        RefreshQuestContent();
    }

    private void Update()
    {
        if (Keyboard.current != null &&
            Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            ToggleQuestPanel();
        }

        if (!isOpen)
            return;

        RefreshQuestContent();

        if (Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseQuestPanel();
        }
    }

    private void ToggleQuestPanel()
    {
        if (questPanel == null)
            return;

        isOpen = !isOpen;
        questPanel.SetActive(isOpen);

        if (isOpen)
            RefreshQuestContent();
    }

    private void CloseQuestPanel()
    {
        isOpen = false;

        if (questPanel != null)
            questPanel.SetActive(false);
    }

    private void RefreshQuestContent()
    {
        if (questContentText == null)
            return;

        int reputation = GetReputation();

        if (reputation < 50)
        {
            questContentText.text =
                "Nhiệm vụ hiện tại:\n\n" +
                "- Chữa bệnh cho dân làng.\n" +
                "- Đạt 50 điểm tín nhiệm.\n\n" +
                "Tiến độ: " + reputation + " / 50";
        }
        else if (reputation < 150)
        {
            questContentText.text =
                "Nhiệm vụ hiện tại:\n\n" +
                "- Nâng danh tiếng y quán.\n" +
                "- Chữa thêm các bệnh cấp thấp.\n" +
                "- Đạt 150 điểm tín nhiệm.\n\n" +
                "Tiến độ: " + reputation + " / 150";
        }
        else if (reputation < 300)
        {
            questContentText.text =
                "Nhiệm vụ hiện tại:\n\n" +
                "- Chữa các bệnh khó hơn.\n" +
                "- Mở rộng danh tiếng trong vùng.\n" +
                "- Đạt 300 điểm tín nhiệm.\n\n" +
                "Tiến độ: " + reputation + " / 300";
        }
        else if (reputation < 500)
        {
            questContentText.text =
                "Nhiệm vụ hiện tại:\n\n" +
                "- Trở thành danh y được dân làng tin tưởng.\n" +
                "- Đạt 500 điểm tín nhiệm.\n\n" +
                "Tiến độ: " + reputation + " / 500";
        }
        else
        {
            questContentText.text =
                "Nhiệm vụ hiện tại:\n\n" +
                "- Tiếp tục chữa bệnh cho dân làng.\n" +
                "- Mở rộng y quán.\n" +
                "- Truyền lại y đạo cho học trò.\n\n" +
                "Bạn đã đạt danh tiếng cao nhất.";
        }
    }

    private int GetReputation()
    {
        if (PlayerEconomy.Instance != null)
            return PlayerEconomy.Instance.Reputation;

        return 0;
    }
}