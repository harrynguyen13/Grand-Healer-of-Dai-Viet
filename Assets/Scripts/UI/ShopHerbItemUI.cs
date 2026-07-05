using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopHerbItemUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text herbNameText;
    [SerializeField] private TMP_Text priceText;
    [SerializeField] private Button button;

    private HerbData herbData;
    private Action<HerbData> onClicked;

    public void Setup(HerbData data, Action<HerbData> clickCallback)
    {
        herbData = data;
        onClicked = clickCallback;

        if (herbNameText != null)
        {
            herbNameText.text = data.herbName;
        }

        if (priceText != null)
        {
            priceText.text = data.buyPrice + " xu";
        }

        if (iconImage != null)
        {
            iconImage.sprite = data.icon;
            iconImage.preserveAspect = true;
        }
    }

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

    private void OnButtonClicked()
    {
        if (herbData != null)
        {
            onClicked?.Invoke(herbData);
        }
    }
}