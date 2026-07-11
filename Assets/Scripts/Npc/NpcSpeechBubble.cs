using TMPro;
using UnityEngine;

public class NpcSpeechBubble : MonoBehaviour
{
    [Header("Bubble UI")]
    [SerializeField] private GameObject bubbleRoot;
    [SerializeField] private TextMeshProUGUI bubbleText;

    private void Awake()
    {
        Hide();
    }

    public void Show(string text)
    {
        if (bubbleText != null)
            bubbleText.text = text;

        if (bubbleRoot != null)
            bubbleRoot.SetActive(true);
    }

    public void Hide()
    {
        if (bubbleText != null)
            bubbleText.text = "";

        if (bubbleRoot != null)
            bubbleRoot.SetActive(false);
    }
}