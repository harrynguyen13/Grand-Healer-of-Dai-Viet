using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class HerbItemUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text herbNameText;
    [SerializeField] private TMP_Text quantityText;
    [SerializeField] private Button button;

    private HerbData herbData;
    private Action<HerbData> onClickCallback;

    public void Setup(HerbData herb, int quantity, Action<HerbData> onClick)
    {
        herbData = herb;
        onClickCallback = onClick;

        if (herbData == null)
            return;

        if (iconImage != null)
            iconImage.sprite = herbData.icon;

        if (herbNameText != null)
            herbNameText.text = herbData.herbName;

        if (quantityText != null)
            quantityText.text = "x" + quantity;

        if (button != null)
        {
            button.onClick.RemoveAllListeners();
            button.onClick.AddListener(OnClickItem);
        }

        SetInteractable(quantity > 0);
    }

    public void UpdateQuantity(int quantity)
    {
        if (quantityText != null)
            quantityText.text = "x" + quantity;

        SetInteractable(quantity > 0);
    }

    private void OnClickItem()
    {
        if (herbData == null)
            return;

        onClickCallback?.Invoke(herbData);
    }

    private void SetInteractable(bool value)
    {
        if (button != null)
            button.interactable = value;
    }
}