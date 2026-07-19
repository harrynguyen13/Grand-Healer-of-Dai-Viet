using System;
using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class GardenPlantItemUI : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text plantNameText;
    [SerializeField] private Button button;

    private GardenPlantData plantData;
    private Action<GardenPlantData> onClicked;

    private void Reset()
    {
        button = GetComponent<Button>();

        Image[] images = GetComponentsInChildren<Image>(true);

        for (int i = 0; i < images.Length; i++)
        {
            if (images[i].gameObject != gameObject)
            {
                iconImage = images[i];
                break;
            }
        }

        TMP_Text[] texts = GetComponentsInChildren<TMP_Text>(true);

        if (texts.Length > 0)
            plantNameText = texts[0];
    }

    private void Awake()
    {
        if (button == null)
            button = GetComponent<Button>();

        if (button != null)
        {
            button.onClick.RemoveListener(OnButtonClicked);
            button.onClick.AddListener(OnButtonClicked);
        }
    }

    public void Setup(GardenPlantData plant, Action<GardenPlantData> clickCallback)
    {
        plantData = plant;
        onClicked = clickCallback;

        if (plantNameText != null)
        {
            plantNameText.text = plant != null ? plant.plantName : "Không rõ";
        }

        if (iconImage != null)
        {
            if (plant != null && plant.iconSprite != null)
            {
                iconImage.sprite = plant.iconSprite;
                iconImage.enabled = true;
            }
            else
            {
                iconImage.enabled = false;
            }
        }
    }

    private void OnButtonClicked()
    {
        if (plantData == null)
            return;

        onClicked?.Invoke(plantData);
    }
}