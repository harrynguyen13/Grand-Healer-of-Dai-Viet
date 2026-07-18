using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
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
        AutoBindReferences();

        if (questPanel != null)
            questPanel.SetActive(false);

        isOpen = false;

        RefreshQuestContent();
    }

    private void Update()
    {
        AutoBindReferences();

        if (questPanel != null)
            isOpen = questPanel.activeSelf;

        if (isOpen)
            RefreshQuestContent();

        // Khi đang gõ trong ô tìm kiếm/input thì không cho phím tắt UI chạy.
        if (IsTypingInInputField())
            return;

        if (Keyboard.current != null &&
            Keyboard.current[toggleKey].wasPressedThisFrame)
        {
            ToggleQuestPanel();
        }

        if (isOpen &&
            Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            CloseQuestPanel();
        }
    }

    private bool IsTypingInInputField()
    {
        if (EventSystem.current == null)
            return false;

        GameObject selectedObject = EventSystem.current.currentSelectedGameObject;

        if (selectedObject == null)
            return false;

        if (selectedObject.GetComponent<TMP_InputField>() != null)
            return true;

        if (selectedObject.GetComponentInParent<TMP_InputField>() != null)
            return true;

        return false;
    }

    private void AutoBindReferences()
    {
        if (questPanel == null)
        {
            Transform panelTransform = transform.Find("QuestPanel");

            if (panelTransform != null)
                questPanel = panelTransform.gameObject;
        }

        if (questContentText == null && questPanel != null)
        {
            TMP_Text[] texts = questPanel.GetComponentsInChildren<TMP_Text>(true);

            for (int i = 0; i < texts.Length; i++)
            {
                if (texts[i].name == "QuestContent_Text" ||
                    texts[i].name == "QuestContentText")
                {
                    questContentText = texts[i];
                    break;
                }
            }
        }
    }

    private void ToggleQuestPanel()
    {
        if (questPanel == null)
            return;

        isOpen = !questPanel.activeSelf;
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

        SetupTextStyle();

        if (QuestRuntimeManager.Instance == null)
        {
            questContentText.text =
                "<b>Nhiệm vụ</b>\n\n" +
                "Chưa tìm thấy QuestRuntimeManager trong scene.";

            return;
        }

        questContentText.text = QuestRuntimeManager.Instance.GetQuestPanelText();
    }

    private void SetupTextStyle()
    {
        questContentText.richText = true;
        questContentText.alignment = TextAlignmentOptions.TopLeft;
        questContentText.textWrappingMode = TextWrappingModes.Normal;
        questContentText.overflowMode = TextOverflowModes.Overflow;
        questContentText.color = Color.black;
    }

    public void RefreshNow()
    {
        RefreshQuestContent();
    }
}