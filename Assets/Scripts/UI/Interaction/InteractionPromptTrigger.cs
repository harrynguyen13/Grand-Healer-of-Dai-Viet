using System.Collections;
using UnityEngine;
using UnityEngine.InputSystem;

public class InteractionPromptTrigger : MonoBehaviour
{
    [Header("Prompt")]
    [SerializeField] private string keyText = "E";
    [SerializeField] private string actionText = "tương tác";

    [Header("Phím dùng để ẩn prompt khi người chơi bấm tương tác")]
    [SerializeField] private Key hideKey = Key.E;

    [Header("Format")]
    [SerializeField] private string promptFormat = "Ấn {0} để {1}";

    [Header("Vị trí chữ so với Player")]
    [SerializeField] private Vector3 worldOffset = new Vector3(0f, 1.15f, 0f);

    [Header("Tự ẩn")]
    [SerializeField] private bool autoHide = true;
    [SerializeField] private float visibleDuration = 1.2f;
    [SerializeField] private bool hideWhenPressKey = true;

    [Header("Player")]
    [SerializeField] private string playerTag = "Player";

    private bool playerInside = false;
    private bool promptShowing = false;
    private Transform currentPlayer;
    private Coroutine autoHideCoroutine;

    private string PromptMessage
    {
        get
        {
            return string.Format(promptFormat, keyText, actionText);
        }
    }

    private void Update()
    {
        if (!playerInside)
            return;

        if (!promptShowing)
            return;

        if (!hideWhenPressKey)
            return;

        if (Keyboard.current == null)
            return;

        if (Keyboard.current[hideKey].wasPressedThisFrame)
        {
            HidePrompt();
        }
    }

    private void OnTriggerEnter2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInside = true;
        currentPlayer = other.transform;

        ShowPrompt();
    }

    private void OnTriggerExit2D(Collider2D other)
    {
        if (!other.CompareTag(playerTag))
            return;

        playerInside = false;
        currentPlayer = null;

        HidePrompt();
    }

    private void ShowPrompt()
    {
        if (currentPlayer == null)
            return;

        promptShowing = true;

        if (InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.ShowPrompt(
                this,
                PromptMessage,
                currentPlayer,
                worldOffset
            );
        }

        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
            autoHideCoroutine = null;
        }

        if (autoHide)
        {
            autoHideCoroutine = StartCoroutine(AutoHideRoutine());
        }
    }

    private IEnumerator AutoHideRoutine()
    {
        yield return new WaitForSecondsRealtime(visibleDuration);

        HidePrompt();

        autoHideCoroutine = null;
    }

    private void HidePrompt()
    {
        promptShowing = false;

        if (autoHideCoroutine != null)
        {
            StopCoroutine(autoHideCoroutine);
            autoHideCoroutine = null;
        }

        if (InteractionPromptUI.Instance != null)
        {
            InteractionPromptUI.Instance.HidePrompt(this);
        }
    }

    private void OnDisable()
    {
        playerInside = false;
        currentPlayer = null;
        HidePrompt();
    }

    private void OnDestroy()
    {
        playerInside = false;
        currentPlayer = null;
        HidePrompt();
    }
}