using UnityEngine;
using UnityEngine.InputSystem;
using UnityEngine.UI;

public class GardenPlantCursorPreview : MonoBehaviour
{
    [Header("Canvas")]
    [SerializeField] private Canvas targetCanvas;

    [Header("Ảnh preview đi theo chuột")]
    [SerializeField] private Image previewImage;

    [Header("Cấu hình hiển thị")]
    [SerializeField] private Vector2 previewSize = new Vector2(42f, 42f);
    [SerializeField] private Vector2 mouseOffset = new Vector2(18f, -18f);

    [Header("Hủy chọn")]
    [SerializeField] private bool cancelWithRightClick = true;
    [SerializeField] private bool cancelWithEscape = true;

    private RectTransform rectTransform;
    private RectTransform canvasRectTransform;

    private void Awake()
    {
        rectTransform = GetComponent<RectTransform>();

        if (previewImage == null)
            previewImage = GetComponent<Image>();

        if (targetCanvas == null)
            targetCanvas = GetComponentInParent<Canvas>();

        if (targetCanvas != null)
            canvasRectTransform = targetCanvas.GetComponent<RectTransform>();

        if (previewImage != null)
        {
            previewImage.raycastTarget = false;
            previewImage.enabled = false;
        }

        rectTransform.sizeDelta = previewSize;
    }

    private void Update()
    {
        if (GardenPlantSelectionUI.Instance == null)
        {
            HidePreview();
            return;
        }

        GardenPlantData selectedPlant = GardenPlantSelectionUI.Instance.SelectedPlant;

        if (selectedPlant == null)
        {
            HidePreview();
            return;
        }

        UpdatePreviewSprite(selectedPlant);
        FollowMouse();

        if (cancelWithRightClick &&
            Mouse.current != null &&
            Mouse.current.rightButton.wasPressedThisFrame)
        {
            GardenPlantSelectionUI.Instance.ClearSelectedPlant();
            HidePreview();
            return;
        }

        if (cancelWithEscape &&
            Keyboard.current != null &&
            Keyboard.current.escapeKey.wasPressedThisFrame)
        {
            GardenPlantSelectionUI.Instance.ClearSelectedPlant();
            HidePreview();
        }
    }

    private void UpdatePreviewSprite(GardenPlantData selectedPlant)
    {
        if (previewImage == null)
            return;

        Sprite spriteToShow = selectedPlant.iconSprite;

        if (spriteToShow == null)
            spriteToShow = selectedPlant.seedlingSprite;

        if (spriteToShow == null)
        {
            HidePreview();
            return;
        }

        previewImage.sprite = spriteToShow;
        previewImage.enabled = true;

        rectTransform.sizeDelta = previewSize;
    }

    private void FollowMouse()
    {
        if (Mouse.current == null)
            return;

        if (canvasRectTransform == null)
            return;

        Vector2 mouseScreenPosition = Mouse.current.position.ReadValue();
        mouseScreenPosition += mouseOffset;

        Camera canvasCamera = null;

        if (targetCanvas != null && targetCanvas.renderMode != RenderMode.ScreenSpaceOverlay)
        {
            canvasCamera = targetCanvas.worldCamera;
        }

        RectTransformUtility.ScreenPointToLocalPointInRectangle(
            canvasRectTransform,
            mouseScreenPosition,
            canvasCamera,
            out Vector2 localPoint
        );

        rectTransform.anchoredPosition = localPoint;
    }

    private void HidePreview()
    {
        if (previewImage != null)
            previewImage.enabled = false;
    }
}