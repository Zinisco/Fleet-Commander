using UnityEngine;
using UnityEngine.InputSystem;
using System.Collections.Generic;
using UnityEngine.EventSystems;

public class PlacedShipDragReturn : MonoBehaviour
{
    [SerializeField] private float dragHeight = 0.5f;
    [SerializeField] private Material neighborHighlightMaterial;
    [SerializeField] private LayerMask hexLayer;
    [SerializeField] private float dragStartThreshold = 8f;

    private bool isPotentialDrag;
    private Vector2 dragStartMousePosition;

    private HexNeighborHighlightManager neighborHighlightManager;

    private Camera cam;
    private DockingBayManager dockingBayManager;
    private PlacedShip placedShip;
    private ShipPlacementManager placementManager;
    private HexSelectionManager hexSelectionManager;

    private bool isDragging;
    private Vector3 startPosition;
    private HexCell startCell;

    private Collider[] shipColliders;

    private void Awake()
    {
        cam = Camera.main;
        placedShip = GetComponent<PlacedShip>();
        dockingBayManager = FindFirstObjectByType<DockingBayManager>();
        placementManager = FindFirstObjectByType<ShipPlacementManager>();
        neighborHighlightManager = FindFirstObjectByType<HexNeighborHighlightManager>();
        hexSelectionManager = FindFirstObjectByType<HexSelectionManager>();

        shipColliders = GetComponentsInChildren<Collider>();
    }

    private void Update()
    {
        if (Mouse.current == null) return;

        if (Mouse.current.leftButton.wasPressedThisFrame)
            TryPrepareDrag();

        if (isPotentialDrag && Mouse.current.leftButton.isPressed)
            CheckIfDragShouldStart();

        if (isDragging)
            DragShip();

        if (Mouse.current.leftButton.wasReleasedThisFrame)
        {
            if (isDragging)
                EndDrag();
            else
                CancelPotentialDrag();
        }
    }

    private void TryPrepareDrag()
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (Physics.Raycast(ray, out RaycastHit hit))
        {
            if (hit.collider.GetComponentInParent<PlacedShipDragReturn>() == this)
            {
                isPotentialDrag = true;
                dragStartMousePosition = Mouse.current.position.ReadValue();

                startPosition = transform.position;
                startCell = placedShip.CurrentCell;
            }
        }
    }

    private void DragShip()
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());
        Plane plane = new Plane(Vector3.up, Vector3.zero);

        if (plane.Raycast(ray, out float distance))
        {
            Vector3 point = ray.GetPoint(distance);
            transform.position = point + Vector3.up * dragHeight;
        }

        HexCell targetCell = GetHexCellUnderMouse();

        if (targetCell != null)
            neighborHighlightManager?.ShowBonusPreview(placedShip.ShipDefinition, targetCell);
        else
            neighborHighlightManager?.ClearHighlights();
    }

    private void EndDrag()
    {
        if (!isDragging) return;

        isPotentialDrag = false;
        isDragging = false;
        HexHoverManager.IsDraggingShip = false;

        neighborHighlightManager?.ClearHighlights();

        Vector2 mousePos = Mouse.current.position.ReadValue();

        DockingBaySlot dockingBaySlot = GetDockingBaySlotUnderMouse(mousePos);

        if (dockingBaySlot != null)
        {
            SetShipColliders(true);

            bool swapped = dockingBayManager.TrySwapPlacedShipWithDockingBaySlot(
                placedShip,
                dockingBaySlot
            );

            if (!swapped)
                ReturnToStart();

            return;
        }

        if (dockingBayManager != null && dockingBayManager.IsPointerOverDockingBay(mousePos))
        {
            SetShipColliders(true);
            dockingBayManager.ReturnPlacedShipToDockingBay(placedShip);

            return;
        }

        HexCell targetCell = GetHexCellUnderMouse();

        if (targetCell == null)
        {
            ReturnToStart();
            SetShipColliders(true);

            return;
        }

        if (targetCell == startCell)
        {
            ReturnToStart();
            SetShipColliders(true);

            return;
        }

        bool movedOrSwapped = placementManager.TryMoveOrSwapPlacedShip(placedShip, targetCell);

        if (!movedOrSwapped)
            ReturnToStart();

        SetShipColliders(true);
    }

    private HexCell GetHexCellUnderMouse()
    {
        Ray ray = cam.ScreenPointToRay(Mouse.current.position.ReadValue());

        if (!Physics.Raycast(ray, out RaycastHit hit, Mathf.Infinity, hexLayer))
            return null;

        return hit.collider.GetComponentInParent<HexCell>();
    }

    private void ReturnToStart()
    {
        transform.position = startPosition;
    }

    private void SetShipColliders(bool state)
    {
        foreach (Collider col in shipColliders)
        {
            if (col != null)
                col.enabled = state;
        }
    }

    private DockingBaySlot GetDockingBaySlotUnderMouse(Vector2 screenPosition)
    {
        if (EventSystem.current == null)
            return null;

        PointerEventData pointerData = new PointerEventData(EventSystem.current)
        {
            position = screenPosition
        };

        List<RaycastResult> results = new();
        EventSystem.current.RaycastAll(pointerData, results);

        foreach (RaycastResult result in results)
        {
            DockingBaySlot slot = result.gameObject.GetComponentInParent<DockingBaySlot>();

            if (slot != null)
                return slot;
        }

        return null;
    }

    private void CheckIfDragShouldStart()
    {
        Vector2 currentMousePosition = Mouse.current.position.ReadValue();
        float distance = Vector2.Distance(dragStartMousePosition, currentMousePosition);

        if (distance < dragStartThreshold)
            return;

        BeginDrag();
    }

    private void BeginDrag()
    {
        isPotentialDrag = false;
        isDragging = true;

        hexSelectionManager?.ClearSelection();

        HexHoverManager.IsDraggingShip = true;

        SetShipColliders(false);
    }

    private void CancelPotentialDrag()
    {
        isPotentialDrag = false;
        HexHoverManager.IsDraggingShip = false;
    }

    public void InitDragSettings(LayerMask hexMask)
    {
        hexLayer = hexMask;
    }
}