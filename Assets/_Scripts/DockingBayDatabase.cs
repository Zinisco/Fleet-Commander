using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fleet Commander/Docking Bay Database")]
public class DockingBayDatabase : ScriptableObject
{
    public List<ShipDefinition> dockedShips = new();

    public void AddShip(ShipDefinition ship)
    {
        if (ship == null) return;

        dockedShips.Add(ship);
    }

    public void RemoveShip(ShipDefinition ship)
    {
        if (ship == null) return;

        dockedShips.Remove(ship);
    }

    public ShipDefinition GetShipAt(int index)
    {
        if (index < 0 || index >= dockedShips.Count)
            return null;

        return dockedShips[index];
    }

    public void RemoveShipAt(int index)
    {
        if (index < 0 || index >= dockedShips.Count)
            return;

        dockedShips.RemoveAt(index);
    }
}