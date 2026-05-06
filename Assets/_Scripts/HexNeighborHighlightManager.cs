using System.Collections.Generic;
using UnityEngine;

public class HexNeighborHighlightManager : MonoBehaviour
{
    [SerializeField] private HexGrid hexGrid;

    private readonly List<HexCell> highlightedCells = new();

    private void Awake()
    {
        if (hexGrid == null)
            hexGrid = FindFirstObjectByType<HexGrid>();
    }

    public void ShowBonusPreview(ShipDefinition shipDefinition, HexCell originCell)
    {
        ClearHighlights();

        if (shipDefinition == null) return;
        if (originCell == null) return;
        if (hexGrid == null) return;

        if (!shipDefinition.providesAdjacentBonus)
            return;

        List<HexCell> neighbors = hexGrid.GetNeighbors(originCell);

        foreach (HexCell neighbor in neighbors)
        {
            HexDirection direction = hexGrid.GetDirection(originCell, neighbor);

            if (shipDefinition.useDirectionalBonus &&
                !shipDefinition.validBonusDirections.Contains(direction))
            {
                continue;
            }

            neighbor.SetNeighborHighlight(true);
            highlightedCells.Add(neighbor);
        }
    }

    public void ShowBonusPreviewFromPlacedShip(PlacedShip placedShip)
    {
        if (placedShip == null) return;

        ShowBonusPreview(placedShip.ShipDefinition, placedShip.CurrentCell);
    }

    public void ClearHighlights()
    {
        foreach (HexCell cell in highlightedCells)
        {
            if (cell != null)
                cell.SetNeighborHighlight(false);
        }

        highlightedCells.Clear();
    }
}