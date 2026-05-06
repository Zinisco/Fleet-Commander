using System.Collections.Generic;
using UnityEngine;

public class DockingBayManager : MonoBehaviour
{
    [Header("Data")]
    [SerializeField] private DockingBayDatabase startingDockingBayDatabase;

    [Header("UI")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private DockingBaySlot slotPrefab;
    [SerializeField] private HexSelectionManager hexSelectionManager;
    [SerializeField] private RectTransform dockingBayDropArea;
    [SerializeField] private ShipInfoPanel dockShipInfoPanel;

    private readonly List<DockedShipData> runtimeDockedShips = new();
    private readonly List<DockingBaySlot> spawnedSlots = new();

    public DockedShipData SelectedShipData =>
      SelectedSlot != null ? SelectedSlot.ShipData : null;

    public ShipDefinition SelectedShip =>
        SelectedShipData != null ? SelectedShipData.definition : null;

    public bool HasSelectedShip => SelectedShip != null;

    public DockingBaySlot SelectedSlot { get; private set; }

    private void Start()
    {
        runtimeDockedShips.Clear();
        foreach (ShipDefinition ship in startingDockingBayDatabase.dockedShips)
        {
            runtimeDockedShips.Add(new DockedShipData(ship, 1));
        }

        RefreshUI();
    }

    public void RefreshUI()
    {
        ClearSpawnedSlots();

        for (int i = 0; i < runtimeDockedShips.Count; i++)
        {
            DockingBaySlot slot = Instantiate(slotPrefab, contentParent);
            slot.Init(this, i, dockShipInfoPanel);
            slot.SetShip(runtimeDockedShips[i]);

            spawnedSlots.Add(slot);
        }
    }

    private void ClearSpawnedSlots()
    {
        for (int i = spawnedSlots.Count - 1; i >= 0; i--)
        {
            if (spawnedSlots[i] != null)
                Destroy(spawnedSlots[i].gameObject);
        }

        spawnedSlots.Clear();
        SelectedSlot = null;
    }

    public void SelectSlot(DockingBaySlot slot)
    {
        if (slot == null || !slot.HasShip) return;

        if (SelectedSlot != null)
            SelectedSlot.SetSelected(false);

        SelectedSlot = slot;
        SelectedSlot.SetSelected(true);

        if (hexSelectionManager != null && hexSelectionManager.CurrentSelected != null)
        {
            hexSelectionManager.TryPlaceSelectedShipOnCell(hexSelectionManager.CurrentSelected);
        }
    }

    public bool TryPlaceSlotOnCell(DockingBaySlot slot, HexCell cell)
    {
        if (slot == null || cell == null) return false;
        if (!slot.HasShip) return false;

        bool placed = hexSelectionManager.TryPlaceSpecificDockedShipOnCell(cell, slot.ShipData);

        if (placed)
        {
            runtimeDockedShips.RemoveAt(slot.SlotIndex);
            RefreshUI();
        }

        return placed;
    }

    public void ConsumeSelectedShip()
    {
        if (SelectedSlot == null) return;

        runtimeDockedShips.RemoveAt(SelectedSlot.SlotIndex);

        RefreshUI();
    }

    public void ClearSelection()
    {
        if (SelectedSlot != null)
            SelectedSlot.SetSelected(false);

        SelectedSlot = null;
    }

    public void AddShipToDockingBay(DockedShipData ship)
    {
        if (ship == null || ship.definition == null) return;

        runtimeDockedShips.Add(ship);
        RefreshUI();
    }

    public void ReturnPlacedShipToDockingBay(PlacedShip placedShip)
    {
        if (placedShip == null) return;

        HexCell cell = placedShip.CurrentCell;

        if (cell != null)
            cell.Clear();

        AddShipToDockingBay(
            new DockedShipData(placedShip.ShipDefinition, placedShip.Level)
        );

        Destroy(placedShip.gameObject);
    }

    public bool TrySwapPlacedShipWithDockingBaySlot(PlacedShip placedShip, DockingBaySlot slot)
    {
        if (placedShip == null) return false;
        if (slot == null || !slot.HasShip) return false;

        int slotIndex = slot.SlotIndex;
        int dockedLevel = slot.Level;

        if (slotIndex < 0 || slotIndex >= runtimeDockedShips.Count)
            return false;

        HexCell cell = placedShip.CurrentCell;
        if (cell == null) return false;

        ShipDefinition shipFromGrid = placedShip.ShipDefinition;
        ShipDefinition shipFromDockingBay = slot.ShipDefinition;

        ShipPlacementManager placementManager =
            FindFirstObjectByType<ShipPlacementManager>();

        if (placementManager == null)
            return false;

        cell.Clear();
        Destroy(placedShip.gameObject);

        bool placed = placementManager.TryPlaceShip(cell, shipFromDockingBay, dockedLevel);

        if (!placed)
        {
            runtimeDockedShips[slotIndex] =
    new DockedShipData(shipFromGrid, placedShip.Level);
            RefreshUI();
            return false;
        }

        runtimeDockedShips[slotIndex] =
    new DockedShipData(shipFromGrid, placedShip.Level);
        RefreshUI();

        return true;
    }

    public bool IsPointerOverDockingBay(Vector2 screenPosition)
    {
        if (dockingBayDropArea == null) return false;

        return RectTransformUtility.RectangleContainsScreenPoint(
            dockingBayDropArea,
            screenPosition
        );
    }

    public void SwapDockingBaySlots(int indexA, int indexB)
    {
        if (indexA == indexB) return;

        if (indexA < 0 || indexA >= runtimeDockedShips.Count) return;
        if (indexB < 0 || indexB >= runtimeDockedShips.Count) return;

        DockedShipData shipA = runtimeDockedShips[indexA];
        DockedShipData shipB = runtimeDockedShips[indexB];

        if (CanMergeDockedShips(shipA, shipB))
        {
            shipB.level++;

            runtimeDockedShips.RemoveAt(indexA);

            RefreshUI();

            Debug.Log($"{shipB.definition.shipName} merged into {shipB.definition.shipName} {ToRoman(shipB.level)}");
            return;
        }

        runtimeDockedShips[indexA] = shipB;
        runtimeDockedShips[indexB] = shipA;

        RefreshUI();
    }

    public DockingBaySlot GetSlotUnderScreenPosition(Vector2 screenPosition)
    {
        foreach (DockingBaySlot slot in spawnedSlots)
        {
            if (slot != null && slot.ContainsScreenPoint(screenPosition))
                return slot;
        }

        return null;
    }

    private bool CanMergeDockedShips(DockedShipData a, DockedShipData b)
    {
        if (a == null || b == null)
            return false;

        if (a.definition == null || b.definition == null)
            return false;

        if (a.definition != b.definition)
            return false;

        if (a.level != b.level)
            return false;

        return true;
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