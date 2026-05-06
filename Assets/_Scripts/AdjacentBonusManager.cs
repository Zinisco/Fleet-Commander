using System.Collections.Generic;
using UnityEngine;

public class AdjacentBonusManager : MonoBehaviour
{
    [SerializeField] private HexGrid hexGrid;
    [SerializeField] private bool showDebugLogs = true;

    private void Awake()
    {
        if (hexGrid == null)
            hexGrid = FindFirstObjectByType<HexGrid>();
    }

    public void RecalculateAllBonuses()
    {
        PlacedShip[] ships = FindObjectsByType<PlacedShip>(FindObjectsSortMode.None);

        foreach (PlacedShip ship in ships)
        {
            ship.ResetBonusStats();
        }

        foreach (PlacedShip ship in ships)
        {
            ApplyAdjacentBonuses(ship);
        }

        if (showDebugLogs)
            Debug.Log($"Recalculated bonuses for {ships.Length} ships.");
    }

    private void ApplyAdjacentBonuses(PlacedShip ship)
    {
        if (ship.CurrentCell == null)
            return;

        List<HexCell> neighbors = hexGrid.GetNeighbors(ship.CurrentCell);

        foreach (HexCell neighbor in neighbors)
        {
            if (neighbor == null || !neighbor.isOccupied)
                continue;

            GameObject adjacentObject = neighbor.currentShip;
            if (adjacentObject == null)
                continue;

            PlacedShip adjacentShip = adjacentObject.GetComponent<PlacedShip>();
            if (adjacentShip == null)
                continue;

            ShipDefinition adjacentDef = adjacentShip.ShipDefinition;

            if (adjacentDef == null)
                continue;

            if (!adjacentDef.providesAdjacentBonus)
                continue;

            HexDirection directionFromBufferToReceiver =
    hexGrid.GetDirection(adjacentShip.CurrentCell, ship.CurrentCell);

            if (adjacentDef.useDirectionalBonus &&
                !adjacentDef.validBonusDirections.Contains(directionFromBufferToReceiver))
            {
                continue;
            }

            ship.ApplyBonus(adjacentDef.bonusType, adjacentDef.bonusAmount);

            Debug.Log(
                $"{adjacentDef.shipName} buffs {ship.ShipDefinition.shipName} " +
                $"from {directionFromBufferToReceiver}"
            );

            if (showDebugLogs)
            {
                Debug.Log(
                    $"{adjacentDef.shipName} gives {adjacentDef.bonusType} +{adjacentDef.bonusAmount} " +
                    $"to {ship.ShipDefinition.shipName}"
                );
            }
        }
    }
}