using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSelectedHerbItemUI : MonoBehaviour
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
            button.onClick.AddListener(OnButtonClicked);
        }
    }

    public void Setup(HerbData data, int quantity, Action<HerbData> clickCallback)
    {
        herbData = data;
        onClicked = clickCallback;

        if (iconImage != null)
        {
            iconImage.sprite = data.icon;
            iconImage.preserveAspect = true;
        }

        if (herbNameText != null)
        {
            herbNameText.text = data.herbName;
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
}