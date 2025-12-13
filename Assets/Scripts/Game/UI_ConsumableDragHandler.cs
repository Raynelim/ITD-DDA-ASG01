using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using TMPro;

public class UI_ConsumableDragHandler : MonoBehaviour, 
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private ConsumableMenuCarousel carousel;
    [SerializeField] private TextMeshProUGUI notificationText; // For "Not enough batteries" message

    private Canvas canvas;
    private GameObject dragIcon;
    private bool hasEnoughBatteries = false;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        // Check if player has enough batteries
        hasEnoughBatteries = CheckBatteryInventory();

        if (!hasEnoughBatteries)
        {
            ShowNotification("Not enough batteries!");
            return; // Don't create drag icon if no batteries
        }

        // Create drag icon
        dragIcon = new GameObject("DragIcon");
        dragIcon.transform.SetParent(canvas.transform, false);

        Image img = dragIcon.AddComponent<Image>();

        img.raycastTarget = false;

        var rt = dragIcon.GetComponent<RectTransform>();
        rt.sizeDelta = new Vector2(100, 100);
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (!hasEnoughBatteries)
            return;

        if (dragIcon != null)
            dragIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        if (!hasEnoughBatteries)
            return;

        if (dragIcon != null)
            Destroy(dragIcon);

        DropInWorld(eventData);
    }

    private void DropInWorld(PointerEventData eventData)
    {
        Camera cam = Camera.main;
        Ray ray = cam.ScreenPointToRay(eventData.position);

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            var data = carousel.Current;
            
            // Deduct battery from inventory
            DeductBattery(data.batteryType, data.batteryCost);
            
            GameObject obj = Instantiate(data.worldPrefab, hit.point, Quaternion.identity);

            var wc = obj.AddComponent<WorldConsumable>();
            wc.data = data;
        }
    }

    private bool CheckBatteryInventory()
    {
        if (DevSettings.Instance != null && DevSettings.Instance.devModeEnabled)
            return true;

        var data = carousel.Current;
        int currentCount = 0;

        switch (data.batteryType.ToLower())
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

        return currentCount >= data.batteryCost;
    }


    private void DeductBattery(string batteryType, int amount)
    {
        GameManager gm = FindObjectOfType<GameManager>();
        if (gm != null)
        {
            gm.UseBattery(batteryType, amount);
            Debug.Log($"Used {amount} {batteryType} battery for feeding");
        }
    }

    private void ShowNotification(string message)
    {
        if (notificationText != null)
        {
            notificationText.text = message;
            notificationText.gameObject.SetActive(true);
            Invoke(nameof(HideNotification), 2f); // Hide after 2 seconds
        }
        Debug.LogWarning(message);
    }

    private void HideNotification()
    {
        if (notificationText != null)
        {
            notificationText.gameObject.SetActive(false);
        }
    }
}
