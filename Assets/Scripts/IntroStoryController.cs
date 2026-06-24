using System.Collections;
using TMPro;
using UnityEngine;
using UnityEngine.SceneManagement;

public class IntroStoryController : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private TMP_Text storyText;

    [Header("Story")]
    [TextArea(3, 6)]
    [SerializeField] private string[] storyLines;

    [Header("Typing")]
    [SerializeField] private float typingSpeed = 0.035f;

    [Header("Scene")]
    [SerializeField] private string gameSceneName = "SampleScene";

    private int currentLineIndex = 0;
    private bool isTyping = false;
    private Coroutine typingCoroutine;

    private void Start()
    {
        if (storyLines == null || storyLines.Length == 0)
        {
            storyLines = new string[]
            {
                "Cuối thế kỷ XIX, Đại Việt chìm trong những ngày tháng rối ren. Dịch bệnh lan khắp thôn làng, người dân khốn khó, y quán dần vắng bóng lương y.",
                "Ngươi là truyền nhân trẻ tuổi của một dòng y gia lâu đời, được thầy truyền dạy y thuật, dược lý và đạo làm thầy thuốc.",
                "Sau một biến cố lớn, ngươi trở về quê hương, mang theo y thư cũ và lời dặn cuối cùng của sư phụ: cứu người trước, danh lợi theo sau.",
                "Từ hôm nay, con đường hành y cứu dân, phục hưng y đạo Đại Việt của ngươi chính thức bắt đầu."
            };
        }

        ShowCurrentLine();
    }

    public void OnNextClicked()
    {
        if (isTyping)
        {
            FinishTypingCurrentLine();
            return;
        }

        currentLineIndex++;

        if (currentLineIndex >= storyLines.Length)
        {
            FinishIntro();
            return;
        }

        ShowCurrentLine();
    }

    public void OnSkipClicked()
    {
        FinishIntro();
    }

    private void ShowCurrentLine()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        typingCoroutine = StartCoroutine(TypeLine(storyLines[currentLineIndex]));
    }

    private IEnumerator TypeLine(string line)
    {
        isTyping = true;
        storyText.text = "";

        foreach (char c in line)
        {
            storyText.text += c;
            yield return new WaitForSeconds(typingSpeed);
        }

        isTyping = false;
    }

    private void FinishTypingCurrentLine()
    {
        if (typingCoroutine != null)
            StopCoroutine(typingCoroutine);

        storyText.text = storyLines[currentLineIndex];
        isTyping = false;
    }

    private void FinishIntro()
    {
        PlayerPrefs.SetInt("HasSeenIntro", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene(gameSceneName);
    }
}