using System;
using TMPro;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class ShopSelectedHerbItemUI : MonoBehaviour, IPointerEnterHandler, IPointerExitHandler, IPointerMoveHandler
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text herbNameText;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Button button;

    private HerbData herbData;
    private Action<HerbData> onClicked;

    private void Awake()
    {
        if (button == null)
        {
            button = GetComponent<Button>();
        }

        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
            button.onClick.AddListener(OnButtonClicked);
        }
    }

    public void Setup(HerbData data, int quantity, Action<HerbData> clickCallback)
    {
        herbData = data;
        onClicked = clickCallback;

        if (herbData == null)
            return;

        if (iconImage != null)
        {
            iconImage.sprite = herbData.icon;
            iconImage.preserveAspect = true;
            iconImage.raycastTarget = true;
        }

        if (herbNameText != null)
        {
            herbNameText.text = herbData.herbName;
        }

        if (quantityText != null)
        {
            quantityText.text = "x" + quantity;
        }
    }

    private void OnButtonClicked()
    {
        if (herbData != null)
        {
            onClicked?.Invoke(herbData);
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (herbData == null)
            return;

        if (HerbRoleTooltipUI.Instance == null)
        {
            Debug.LogWarning("Không tìm thấy HerbRoleTooltipUI.Instance.");
            return;
        }

        HerbRoleTooltipUI.Instance.Show(herbData, eventData.position);
    }

    public void OnPointerMove(PointerEventData eventData)
    {
        if (HerbRoleTooltipUI.Instance == null)
            return;

        HerbRoleTooltipUI.Instance.Move(eventData.position);
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (HerbRoleTooltipUI.Instance == null)
            return;

        HerbRoleTooltipUI.Instance.Hide();
    }

    private void OnDisable()
    {
        if (HerbRoleTooltipUI.Instance != null)
        {
            HerbRoleTooltipUI.Instance.Hide();
        }
    }
}