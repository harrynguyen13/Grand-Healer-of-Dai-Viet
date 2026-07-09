using TMPro;
using UnityEngine;

public class InteractionPromptUI : MonoBehaviour
{
    public static InteractionPromptUI Instance { get; private set; }

    [Header("UI")]
    [SerializeField] private GameObject promptRoot;
    [SerializeField] private TMP_Text promptText;

    private RectTransform promptRect;
    private RectTransform parentRect;
    private Canvas canvas;

    private Object currentOwner;
    private Transform currentTarget;
    private Vector3 currentWorldOffset;

    private void Awake()
    {
        if (Instance != null && Instance != this)
        {
            Destroy(gameObject);
            return;
        }

        Instance = this;

        if (promptRoot != null)
        {
            promptRect = promptRoot.GetComponent<RectTransform>();
            parentRect = promptRect.parent as RectTransform;
            canvas = promptRoot.GetComponentInParent<Canvas>();
        }

        HidePrompt(null);
    }

    private void LateUpdate()
    {
        if (promptRoot == null)
            return;

        if (!promptRoot.activeSelf)
            return;

        if (currentTarget == null)
            return;

        UpdatePromptPosition();
    }

    public void ShowPrompt(Object owner, string message, Transform followTarget, Vector3 worldOffset)
    {
        currentOwner = owner;
        currentTarget = followTarget;
        currentWorldOffset = worldOffset;

        if (promptText != null)
            promptText.text = message;

        if (promptRoot != null)
            promptRoot.SetActive(true);

        UpdatePromptPosition();
    }

    public void HidePrompt(Object owner)
    {
        if (owner != null && currentOwner != owner)
            return;

        currentOwner = null;
        currentTarget = null;
        currentWorldOffset = Vector3.zero;

        if (promptRoot != null)
            promptRoot.SetActive(false);

        if (promptText != null)
            promptText.text = "";
    }

    private void UpdatePromptPosition()
    {
        if (promptRect == null)
            return;

        if (parentRect == null)
            return;

        if (currentTarget == null)
            return;

        Camera worldCamera = Camera.main;

        if (worldCamera == null)
            return;

        Vector3 worldPosition = currentTarget.position + currentWorldOffset;
        Vector3 screenPosition = worldCamera.WorldToScreenPoint(worldPosition);

        // Quan trọng: tránh lỗi Screen position out of view frustum
        if (screenPosition.z <= 0.01f)
        {
            if (promptRoot != null)
                promptRoot.SetActive(false);

            return;
        }

        if (screenPosition.x < 0f ||
            screenPosition.x > Screen.width ||
            screenPosition.y < 0f ||
            screenPosition.y > Screen.height)
        {
            if (promptRoot != null)
                promptRoot.SetActive(false);

            return;
        }

        if (promptRoot != null && !promptRoot.activeSelf)
            promptRoot.SetActive(true);

        Camera uiCamera = null;

        // Với Screen Space Overlay thì bắt buộc để null
        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            uiCamera = canvas.worldCamera;
        }

        Vector2 localPoint;

        bool success = RectTransformUtility.ScreenPointToLocalPointInRectangle(
            parentRect,
            screenPosition,
            uiCamera,
            out localPoint
        );

        if (!success)
            return;

        promptRect.anchoredPosition = localPoint;
    }
}