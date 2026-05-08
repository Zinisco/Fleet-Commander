using UnityEngine;
using UnityEngine.UI;
using TMPro;
using UnityEngine.EventSystems;

public class DockingBaySlot : MonoBehaviour,
    IPointerEnterHandler,
    IPointerExitHandler,
    IBeginDragHandler,
    IDragHandler,
    IEndDragHandler,
    IDropHandler
{
    [SerializeField] private Image iconImage;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private Button button;

    [Header("Visuals")]
    [SerializeField] private Image backgroundImage;
    [SerializeField] private Color normalColor = Color.white;
    [SerializeField] private Color hoverColor = new Color(1f, 1f, 1f, 0.85f);
    [SerializeField] private Color selectedColor = new Color(0.6f, 1f, 0.6f, 1f);
    [SerializeField] private ShipInfoPanel dockShipInfoPanel;

    [Header("Drag")]
    [SerializeField] private Canvas rootCanvas;
    [SerializeField] private Image dragIconPrefab;

    [Header("World Drop")]
    [SerializeField] private LayerMask hexLayer;

    private HexNeighborHighlightManager neighborHighlightManager;

    private Image dragIconInstance;

    private DockingBayManager dockingBayManager;
    private int slotIndex;

    private bool isSelected;

    private DockedShipData shipData;
    private ShipInfoPanel shipInfoPanel;
    private HexSelectionManager hexSelectionManager;
    private ShopManager shopManager;

    public DockedShipData ShipData => shipData;
    public ShipDefinition ShipDefinition => shipData != null ? shipData.definition : null;
    public int Level => shipData != null ? shipData.level : 1;
    public bool HasShip => ShipDefinition != null;
    public int SlotIndex => slotIndex;


    public void Init(DockingBayManager manager, int index, ShipInfoPanel infoPanel)
    {
        dockingBayManager = manager;
        slotIndex = index;
        shipInfoPanel = infoPanel;

        if (button == null)
            button = GetComponent<Button>();

        if (rootCanvas == null)
            rootCanvas = GetComponentInParent<Canvas>();

        neighborHighlightManager = FindFirstObjectByType<HexNeighborHighlightManager>();
        hexSelectionManager = FindFirstObjectByType<HexSelectionManager>();
        shopManager = FindFirstObjectByType<ShopManager>();

        button.onClick.RemoveAllListeners();
        button.onClick.AddListener(OnClicked);

        SetNormal();
    }

    public void SetShip(DockedShipData data)
    {
        shipData = data;

        if (shipData == null || shipData.definition == null)
        {
            ClearVisuals();
            return;
        }

        ShipDefinition def = shipData.definition;

        if (iconImage != null)
        {
            iconImage.enabled = true;
            iconImage.sprite = def.icon;
        }

        if (nameText != null)
        {
            nameText.text = shipData.level <= 1
                ? def.shipName
                : $"{def.shipName} {ToRoman(shipData.level)}";
        }

        if (button != null)
            button.interactable = true;
    }

    public void SetSelected(bool state)
    {
        isSelected = state;

        if (state)
            SetSelectedVisual();
        else
            SetNormal();
    }

    public void ClearSlot()
    {
        shipData = null;
        isSelected = false;
        ClearVisuals();
    }

    private void ClearVisuals()
    {
        if (iconImage != null)
        {
            iconImage.sprite = null;
            iconImage.enabled = false;
        }

        if (nameText != null)
            nameText.text = "";

        if (button != null)
            button.interactable = false;

        SetNormal();
    }

    private void OnClicked()
    {
        if (!HasShip) return;

        dockingBayManager.SelectSlot(this);
    }

    public void OnBeginDrag(PointerEventData eventData)
    {
        hexSelectionManager?.ClearSelection();

        if (!HasShip || dragIconPrefab == null) return;

        dragIconInstance = Instantiate(dragIconPrefab, rootCanvas.transform);
        dragIconInstance.sprite = ShipDefinition.icon;
        dragIconInstance.raycastTarget = false;
        dragIconInstance.transform.position = eventData.position;
    }

    public void OnDrag(PointerEventData eventData)
    {
        if (dragIconInstance == null) return;

        dragIconInstance.transform.position = eventData.position;

        bool shouldSellMode =
            shopManager != null &&
            shopManager.ShouldActivateSellMode(eventData.position);

        shopManager?.SetSellMode(shouldSellMode);

        HexCell cell = GetHexCellUnderMouse(eventData.position);

        if (cell != null)
            neighborHighlightManager?.ShowBonusPreview(ShipDefinition, cell);
        else
            neighborHighlightManager?.ClearHighlights();
    }

    public void OnDrop(PointerEventData eventData)
    {
        DockingBaySlot draggedSlot =
            eventData.pointerDrag != null
                ? eventData.pointerDrag.GetComponent<DockingBaySlot>()
                : null;

        if (draggedSlot == null) return;
        if (draggedSlot == this) return;

        dockingBayManager.SwapDockingBaySlots(
            draggedSlot.SlotIndex,
            SlotIndex
        );
    }

    public void OnEndDrag(PointerEventData eventData)
    {
        neighborHighlightManager?.ClearHighlights();
        CleanupDragIcon();

        shopManager?.SetSellMode(false);

        if (shopManager != null &&
     shopManager.ShouldActivateSellMode(eventData.position) &&
     shopManager.IsPointerOverShop(eventData.position))

            if (dockingBayManager != null &&
            dockingBayManager.IsPointerOverDockingBay(eventData.position))
        {
            return;
        }

        if (EventSystem.current != null &&
            EventSystem.current.IsPointerOverGameObject())
        {
            return;
        }

        if (!HasShip) return;

        Ray ray = Camera.main.ScreenPointToRay(eventData.position);

        if (Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, hexLayer))
        {
            HexCell cell = hit.collider.GetComponentInParent<HexCell>();

            if (cell != null)
            {
                dockingBayManager.TryPlaceSlotOnCell(this, cell);
            }
        }
    }

    public void OnPointerEnter(PointerEventData eventData)
    {
        if (!isSelected)
            SetHover();

        if (HasShip)
        {
            shipInfoPanel?.Show(ShipData);
        }
    }

    public void OnPointerExit(PointerEventData eventData)
    {
        if (!isSelected)
            SetNormal();

        shipInfoPanel?.Hide();
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

    private void SetSelectedVisual()
    {
        if (backgroundImage != null)
            backgroundImage.color = selectedColor;
    }

    private void OnDisable()
    {
        shopManager?.SetSellMode(false);
        CleanupDragIcon();
    }

    private void CleanupDragIcon()
    {
        if (dragIconInstance != null)
        {
            Destroy(dragIconInstance.gameObject);
            dragIconInstance = null;
        }
    }

    public bool ContainsScreenPoint(Vector2 screenPosition)
    {
        RectTransform rectTransform = transform as RectTransform;

        if (rectTransform == null)
            return false;

        Canvas canvas = GetComponentInParent<Canvas>();
        Camera canvasCamera = null;

        if (canvas != null && canvas.renderMode != RenderMode.ScreenSpaceOverlay)
            canvasCamera = canvas.worldCamera;

        return RectTransformUtility.RectangleContainsScreenPoint(
            rectTransform,
            screenPosition,
            canvasCamera
        );
    }

    private HexCell GetHexCellUnderMouse(Vector2 screenPosition)
    {
        Ray ray = Camera.main.ScreenPointToRay(screenPosition);

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, hexLayer))
            return null;

        return hit.collider.GetComponentInParent<HexCell>();
    }

    private string ToRoman(int number)
    {
        return number switch
        {
            1 => "I",
            2 => "II",
            3 => "III",
            4 => "IV",
            5 => "V",
            _ => number.ToString()
        };
    }
}