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

    [Header("Voice")]
    [SerializeField] private IntroVoice introVoice;

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
                "Vào thế kỷ XIX, đất Việt chìm trong những ngày tháng rối ren. Dịch bệnh lan khắp thôn làng, dân chúng lầm than, còn những y quán xưa dần vắng bóng lương y.",

                "Ngươi là truyền nhân trẻ tuổi của một dòng y gia lâu đời, được sư phụ truyền dạy y thuật, dược lý và đạo làm thầy thuốc.",

                "Sau một biến cố lớn, ngươi trở về quê hương, mang theo cuốn y thư cũ cùng lời dặn cuối cùng của thầy: cứu người trước, danh lợi hãy để sau.",

                "Từ hôm nay, con đường hành y cứu dân, gây dựng lại y quán và phục hưng y đạo đất Việt của ngươi chính thức bắt đầu."
            };
        }

        currentLineIndex = 0;
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
        {
            StopCoroutine(typingCoroutine);
        }

        if (introVoice != null)
        {
            introVoice.PlayVoiceByIndex(currentLineIndex);
        }

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
        {
            StopCoroutine(typingCoroutine);
        }

        storyText.text = storyLines[currentLineIndex];
        isTyping = false;
    }

    private void FinishIntro()
    {
        if (typingCoroutine != null)
        {
            StopCoroutine(typingCoroutine);
        }

        if (introVoice != null)
        {
            introVoice.StopVoice();
        }

        PlayerPrefs.SetInt("HasSeenIntro", 1);
        PlayerPrefs.Save();

        SceneManager.LoadScene(gameSceneName);
    }
}