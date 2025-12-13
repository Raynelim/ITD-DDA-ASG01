using UnityEngine;
using UnityEngine.UI;
using TMPro;

public class ConsumableMenuCarousel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private GameObject uiElement;  
    // Can be Panel, Image, or RawImage GameObject

    [Header("Text Displays")]
    [SerializeField] private TextMeshProUGUI nameText;
    [SerializeField] private TextMeshProUGUI quantityText;

    [Header("Consumables")]
    [SerializeField] private ConsumableData[] consumables;

    public int CurrentIndex { get; private set; }
    public ConsumableData Current => consumables[CurrentIndex];

    private void Start()
    {
        UpdateDisplay();
    }

    public void Next()
    {
        CurrentIndex = (CurrentIndex + 1) % consumables.Length;
        UpdateDisplay();
    }

    public void Previous()
    {
        CurrentIndex = (CurrentIndex - 1 + consumables.Length) % consumables.Length;
        UpdateDisplay();
    }

    private void UpdateDisplay()
    {
        if (uiElement != null && Current.uiMaterial != null)
        {
            Graphic graphic = uiElement.GetComponent<Graphic>();
            if (graphic != null)
            {
                graphic.material = Current.uiMaterial;
            }
        }

        // Update name text
        if (nameText != null)
        {
            nameText.text = Current.consumableName;
        }

        // Update quantity text (current battery count)
        if (quantityText != null)
        {
            int currentCount = 0;
            switch (Current.batteryType.ToLower())
            {
                case "small":
                    currentCount = UserDataManager.currentSmallBattery;
                    break;
                case "medium":
                    currentCount = UserDataManager.currentMediumBattery;
                    break;
                case "large":
                    currentCount = UserDataManager.currentLargeBattery;
                    break;
            }
            quantityText.text = $"x{currentCount}";
        }
    }
}
