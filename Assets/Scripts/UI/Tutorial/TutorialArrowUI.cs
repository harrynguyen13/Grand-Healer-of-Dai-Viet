using UnityEngine;

public class TutorialArrowUI : MonoBehaviour
{
    [Header("Camera")]
    [SerializeField] private Camera worldCamera;

    [Header("UI")]
    [SerializeField] private Canvas canvas;
    [SerializeField] private RectTransform arrowRect;

    [Header("Target hiện tại")]
    [SerializeField] private Transform target;

    [Header("Vị trí")]
    [SerializeField] private Vector2 screenOffset = new Vector2(0f, 80f);

    [Header("Hướng mũi tên")]
    [SerializeField] private float rotationZ = -90f;

    [Header("Chuyển động")]
    [SerializeField] private float bobDistance = 10f;
    [SerializeField] private float bobSpeed = 3f;

    private RectTransform canvasRect;
    private bool isVisible;

    // false = object ngoài map
    // true = UI
    private bool targetIsUI;

    private void Start()
    {
        if (worldCamera == null)
            worldCamera = Camera.main;

        if (canvas == null)
            canvas = GetComponent<Canvas>();

        if (canvas == null)
            canvas = GetComponentInParent<Canvas>();

        if (canvas != null)
            canvasRect = canvas.transform as RectTransform;

        if (arrowRect == null)
            arrowRect = GetComponent<RectTransform>();

        ApplyRotation();

        if (target == null)
            HideArrow();
    }

    private void LateUpdate()
    {
        if (!isVisible)
            return;

        if (target == null)
            return;

        if (arrowRect == null)
            return;

        if (canvas == null || canvasRect == null)
            return;

        Vector3 screenPosition;

        // =========================
        // TARGET LÀ UI
        // =========================
        if (targetIsUI)
        {
            RectTransform uiTarget = target as RectTransform;

            if (uiTarget == null)
                return;

            Camera targetUICamera = null;

            Canvas targetCanvas =
                uiTarget.GetComponentInParent<Canvas>();

            if (targetCanvas != null &&
                targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
            {
                targetUICamera = targetCanvas.worldCamera;
            }

            screenPosition =
                RectTransformUtility.WorldToScreenPoint(
                    targetUICamera,
                    uiTarget.position
                );
        }

        // =========================
        // TARGET NGOÀI MAP
        // =========================
        else
        {
            if (worldCamera == null)
                worldCamera = Camera.main;

            if (worldCamera == null)
                return;

            screenPosition =
                worldCamera.WorldToScreenPoint(
                    target.position
                );

            if (screenPosition.z < 0f)
            {
                SetArrowObjectActive(false);
                return;
            }
        }

        Camera uiCamera = null;

        if (canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            uiCamera = canvas.worldCamera;

        Vector2 localPoint;

        bool converted =
            RectTransformUtility.ScreenPointToLocalPointInRectangle(
                canvasRect,
                screenPosition,
                uiCamera,
                out localPoint
            );

        if (!converted)
            return;

        SetArrowObjectActive(true);

        float bob =
            Mathf.Sin(Time.unscaledTime * bobSpeed)
            * bobDistance;

        arrowRect.anchoredPosition =
            localPoint
            + screenOffset
            + new Vector2(0f, bob);
    }

    // Dùng cho object ngoài map
    public void SetTarget(
        Transform newTarget,
        Vector2 newScreenOffset,
        float newRotationZ
    )
    {
        target = newTarget;
        screenOffset = newScreenOffset;
        rotationZ = newRotationZ;

        targetIsUI = false;

        ApplyRotation();
        ShowArrow();
    }

    // Dùng cho UI
    public void SetUITarget(
        RectTransform newTarget,
        Vector2 newScreenOffset,
        float newRotationZ
    )
    {
        target = newTarget;
        screenOffset = newScreenOffset;
        rotationZ = newRotationZ;

        targetIsUI = true;

        ApplyRotation();
        ShowArrow();
    }

    public void ShowArrow()
    {
        isVisible = true;
        SetArrowObjectActive(true);
    }

    public void HideArrow()
    {
        isVisible = false;
        SetArrowObjectActive(false);
    }

    private void ApplyRotation()
    {
        if (arrowRect == null)
            return;

        arrowRect.localRotation =
            Quaternion.Euler(
                0f,
                0f,
                rotationZ
            );
    }

    private void SetArrowObjectActive(bool value)
    {
        if (arrowRect == null)
            return;

        if (arrowRect.gameObject.activeSelf != value)
            arrowRect.gameObject.SetActive(value);
    }
}