using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class ShopSlotUI : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text shipNameText;
    [SerializeField] private TMP_Text costText;

    [Header("Visuals")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(0.8f, 1f, 1f, 1f);

    [Header("Drag")]
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private Image dragIconPrefab;

    private Image dragIconInstance;

    private ShipDefinition ship;
    private ShopManager shopManager;

    public ShipDefinition Ship => ship;

    public void Setup(ShipDefinition definition, ShopManager manager)
    {
        ship = definition;
        shopManager = manager;

        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();

        iconImage.sprite = ship.icon;
        shipNameText.text = ship.shipName;
        costText.text = $"{ship.shopCost} Credits";

        SetNormal();
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        SetHover();
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        SetNormal();
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        if (dragIconPrefab == null || ship == null)
            return;

        dragIconInstance = Instantiate(
            dragIconPrefab,
            rootCanvas.transform
        );

        dragIconInstance.sprite = ship.icon;
        dragIconInstance.raycastTarget = false;
        dragIconInstance.transform.position = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIconInstance != null)
            dragIconInstance.transform.position = eventData.position;
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        CleanupDragIcon();

        if (!shopManager.IsPointerOverDockingBay(eventData.position))
            return;

        DockingBaySlot targetSlot =
            shopManager.GetDockingBaySlotUnderMouse(eventData.position);

        bool bought;

        if (targetSlot != null)
        {
            bought = shopManager.TryBuyShipIntoDockingSlot(ship, targetSlot);
        }
        else
        {
            bought = shopManager.TryBuyShip(ship);
        }

        if (bought)
        {
            gameObject.SetActive(false);
        }
    }

    private void CleanupDragIcon()
    {
        if (dragIconInstance != null)
        {
            Destroy(dragIconInstance.gameObject);
            dragIconInstance = null;
        }
    }

    private void SetNormal()
    {
        if (backgroundImage != null)
            backgroundImage.color = normalColor;
    }

    private void SetHover()
    {
        if (backgroundImage != null)
            backgroundImage.color = hoverColor;
    }
}