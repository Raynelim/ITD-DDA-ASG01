using UnityEngine;
using UnityEngine.UI;

public class ConsumableMenuCarousel : MonoBehaviour
{
    [Header("UI")]
    [SerializeField] private Graphic uiElement;  
    // Graphic works for both Image and RawImage

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
            uiElement.material = Current.uiMaterial;
        }
    }
}
