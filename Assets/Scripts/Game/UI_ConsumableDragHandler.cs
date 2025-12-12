using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;

public class UI_ConsumableDragHandler : MonoBehaviour, 
    IBeginDragHandler, IDragHandler, IEndDragHandler
{
    [SerializeField] private ConsumableMenuCarousel carousel;

    private Canvas canvas;
    private GameObject dragIcon;

    void Awake()
    {
        canvas = GetComponentInParent<Canvas>();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
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
        if (dragIcon != null)
            dragIcon.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
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
            GameObject obj = Instantiate(data.worldPrefab, hit.point, Quaternion.identity);

            var wc = obj.AddComponent<WorldConsumable>();
            wc.data = data;
        }
    }
}
