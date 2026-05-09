using UnityEngine;

public class ShipPlacementManager : MonoBehaviour
{
    [SerializeField] private AdjacentBonusManager adjacentBonusManager;
    [SerializeField] private LayerMask hexLayer;
    [SerializeField] private float shipYOffset = 0.25f;

    public bool TryPlaceShip(HexCell cell, ShipDefinition shipDefinition, int level = 1, ShipTeam team = ShipTeam.Player)
    {
        if (cell == null) return false;
        if (cell.isOccupied) return false;
        if (shipDefinition == null) return false;
        if (shipDefinition.shipPrefab == null) return false;

        Vector3 spawnPosition = GetShipPosition(cell);

        GameObject ship = Instantiate(shipDefinition.shipPrefab, spawnPosition, Quaternion.identity);

        PlacedShip placedShip = ship.GetComponent<PlacedShip>();
        if (placedShip == null)
            placedShip = ship.AddComponent<PlacedShip>();

        placedShip.Init(shipDefinition, cell, level, team);

        PlacedShipDragReturn dragReturn = ship.GetComponent<PlacedShipDragReturn>();

        if (dragReturn == null)
            dragReturn = ship.AddComponent<PlacedShipDragReturn>();

        dragReturn.InitDragSettings(hexLayer);

        cell.SetShip(ship);

        adjacentBonusManager.RecalculateAllBonuses();

        return true;
    }

    public Vector3 GetShipPosition(HexCell cell)
    {
        return cell.GetShipPosition() + Vector3.up * shipYOffset;
    }

    public bool TryMoveOrSwapPlacedShip(PlacedShip movingShip, HexCell targetCell)
    {
        if (movingShip == null) return false;
        if (targetCell == null) return false;

        HexCell startCell = movingShip.CurrentCell;
        if (startCell == null) return false;
        if (targetCell == startCell) return false;

        GameObject movingObject = movingShip.gameObject;

        if (!targetCell.isOccupied)
        {
            startCell.Clear();

            targetCell.SetShip(movingObject);
            movingShip.SetCurrentCell(targetCell);
            movingObject.transform.position = GetShipPosition(targetCell);

            adjacentBonusManager.RecalculateAllBonuses();

            return true;
        }

        GameObject otherObject = targetCell.currentShip;
        if (otherObject == null) return false;

        PlacedShip otherShip = otherObject.GetComponent<PlacedShip>();
        if (otherShip == null) return false;

        if (CanMergeShips(movingShip, otherShip))
        {
            MergeShips(movingShip, otherShip, startCell);
            adjacentBonusManager.RecalculateAllBonuses();
            return true;
        }

        startCell.SetShip(otherObject);
        otherShip.SetCurrentCell(startCell);
        otherObject.transform.position = GetShipPosition(startCell);

        targetCell.SetShip(movingObject);
        movingShip.SetCurrentCell(targetCell);
        movingObject.transform.position = GetShipPosition(targetCell);

        adjacentBonusManager.RecalculateAllBonuses();

        return true;
    }

    public bool TryPlaceFromDockingBayWithSwap(
     HexCell targetCell,
     DockedShipData dockingBayShip,
     DockingBayManager dockingBayManager
 )
    {
        if (targetCell == null) return false;
        if (dockingBayShip == null || dockingBayShip.definition == null) return false;
        if (dockingBayManager == null) return false;

        if (!targetCell.isOccupied)
        {
            return TryPlaceShip(targetCell, dockingBayShip.definition, dockingBayShip.level);
        }

        GameObject existingShipObject = targetCell.currentShip;
        if (existingShipObject == null) return false;

        PlacedShip existingShip = existingShipObject.GetComponent<PlacedShip>();
        if (existingShip == null) return false;

        if (existingShip.ShipDefinition == dockingBayShip.definition &&
            existingShip.Level == dockingBayShip.level)
        {
            existingShip.LevelUp();
            adjacentBonusManager.RecalculateAllBonuses();
            return true;
        }

        DockedShipData existingData =
            new DockedShipData(existingShip.ShipDefinition, existingShip.Level);

        Destroy(existingShipObject);
        targetCell.Clear();

        bool placed = TryPlaceShip(
            targetCell,
            dockingBayShip.definition,
            dockingBayShip.level
        );

        if (placed)
        {
            dockingBayManager.AddShipToDockingBay(existingData);
            return true;
        }

        return false;
    }

    private bool CanMergeShips(PlacedShip movingShip, PlacedShip targetShip)
    {
        if (movingShip == null || targetShip == null)
            return false;

        if (movingShip.ShipDefinition != targetShip.ShipDefinition)
            return false;

        if (movingShip.Level != targetShip.Level)
            return false;

        return true;
    }

    private void MergeShips(PlacedShip movingShip, PlacedShip targetShip, HexCell startCell)
    {
        if (startCell != null)
            startCell.Clear();

        targetShip.LevelUp();

        Destroy(movingShip.gameObject);

        Debug.Log($"{targetShip.ShipDefinition.shipName} merged into {targetShip.DisplayName}");
    }

    public bool TryPlaceDockedShip(HexCell cell, DockedShipData dockedShip)
    {
        if (dockedShip == null) return false;

        return TryPlaceShip(cell, dockedShip.definition, dockedShip.level);
    }
}