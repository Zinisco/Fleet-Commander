using System.Collections.Generic;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.InputSystem;

public class HexSelectionManager : MonoBehaviour
{
    [Header("References")]
    [SerializeField] private ShipPlacementManager placementManager;
    [SerializeField] private DockingBayManager dockingBayManager;
    [SerializeField] private HexNeighborHighlightManager neighborHighlightManager;
    [SerializeField] private HexGrid hexGrid;
    [SerializeField] private ShipInfoPanel placedShipInfoPanel;

    private Camera cam;
    private HexCell currentSelected;

    public HexCell CurrentSelected => currentSelected;

    private InputSystem_Actions input;

    private bool leftClickRequested;
    private bool rightClickRequested;

    void Awake()
    {
        cam = Camera.main;

        input = new InputSystem_Actions();
        input.Player.Enable();

        input.Player.LeftClick.performed += OnLeftClickPerformed;
        input.Player.RightClick.performed += OnRightClickPerformed;

        if (neighborHighlightManager == null)
            neighborHighlightManager = FindFirstObjectByType<HexNeighborHighlightManager>();
    }

    void OnDestroy()
    {
        input.Player.LeftClick.performed -= OnLeftClickPerformed;
        input.Player.RightClick.performed -= OnRightClickPerformed;
        input.Player.Disable();
    }

    void Update()
    {
        if (leftClickRequested)
        {
            leftClickRequested = false;
            HandleWorldLeftClick();
        }

        if (rightClickRequested)
        {
            rightClickRequested = false;
            HandleWorldRightClick();
        }
    }

    void OnLeftClickPerformed(InputAction.CallbackContext ctx)
    {
        leftClickRequested = true;
    }

    void OnRightClickPerformed(InputAction.CallbackContext ctx)
    {
        rightClickRequested = true;
    }

    void HandleWorldLeftClick()
    {
        if (IsPointerOverUI())
            return;

        HexCell cell = GetHexCellUnderMouse();

        if (cell == null)
        {
            ClearSelection();
            return;
        }

        SelectCell(cell);

        if (dockingBayManager.HasSelectedShip)
        {
            TryPlaceSelectedShipOnCell(cell);
        }
    }

    void HandleWorldRightClick()
    {
        if (IsPointerOverUI())
            return;

        Vector2 mousePos = input.Player.Point.ReadValue<Vector2>();
        Ray ray = cam.ScreenPointToRay(mousePos);

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return;

        // First: did we right-click a placed ship?
        PlacedShip placedShip = hit.collider.GetComponentInParent<PlacedShip>();

        if (placedShip != null)
        {
            dockingBayManager.ReturnPlacedShipToDockingBay(placedShip);

            if (currentSelected == placedShip.CurrentCell)
                ClearSelection();

            return;
        }

        // Second: did we right-click an occupied hex?
        HexCell cell = hit.collider.GetComponentInParent<HexCell>();

        if (cell != null)
        {
            TryReturnShipFromCell(cell);
        }
    }

    private HexCell GetHexCellUnderMouse()
    {
        Vector2 mousePos = input.Player.Point.ReadValue<Vector2>();
        Ray ray = cam.ScreenPointToRay(mousePos);

        if (!Physics.Raycast(ray, out RaycastHit hit))
            return null;

        HexCell cell = hit.collider.GetComponentInParent<HexCell>();

        if (cell != null)
            return cell;

        PlacedShip placedShip = hit.collider.GetComponentInParent<PlacedShip>();

        if (placedShip != null)
            return placedShip.CurrentCell;

        return null;
    }

    private bool TryReturnShipFromCell(HexCell cell)
    {
        if (cell == null) return false;
        if (!cell.isOccupied) return false;
        if (cell.currentShip == null) return false;

        PlacedShip placedShip = cell.currentShip.GetComponent<PlacedShip>();

        if (placedShip == null)
            return false;

        dockingBayManager.ReturnPlacedShipToDockingBay(placedShip);

        if (currentSelected == cell)
            ClearSelection();

        return true;
    }

    private bool IsPointerOverUI()
    {
        return EventSystem.current != null &&
               EventSystem.current.IsPointerOverGameObject();
    }

    void SelectCell(HexCell cell)
    {
        if (currentSelected == cell)
        {
            ClearSelection();
            return;
        }

        if (currentSelected != null)
            currentSelected.SetSelected(false);

        currentSelected = cell;
        currentSelected.SetSelected(true);

        neighborHighlightManager?.ClearHighlights();
        placedShipInfoPanel?.Hide();

        if (cell.isOccupied && cell.currentShip != null)
        {
            PlacedShip placedShip = cell.currentShip.GetComponent<PlacedShip>();

            if (placedShip != null)
            {
                placedShipInfoPanel?.Show(placedShip);

                neighborHighlightManager?.ShowBonusPreviewFromPlacedShip(placedShip);
            }
        }
    }

    public void ClearSelection()
    {
        if (currentSelected != null)
        {
            currentSelected.SetSelected(false);
            currentSelected = null;
        }

        neighborHighlightManager?.ClearHighlights();
        placedShipInfoPanel?.Hide();
    }

    public bool TryPlaceSelectedShipOnCell(HexCell cell)
    {
        if (!dockingBayManager.HasSelectedShip)
            return false;

        bool placed = placementManager.TryPlaceFromDockingBayWithSwap(
            cell,
            dockingBayManager.SelectedShipData,
            dockingBayManager
        );

        if (placed)
            dockingBayManager.ConsumeSelectedShip();

        return placed;
    }

    public bool TryPlaceSpecificDockedShipOnCell(HexCell cell, DockedShipData dockedShip)
    {
        return placementManager.TryPlaceFromDockingBayWithSwap(
            cell,
            dockedShip,
            dockingBayManager
        );
    }
}