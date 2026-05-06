using System.Collections.Generic;
using UnityEngine;

[CreateAssetMenu(menuName = "Fleet Commander/Ship Database")]
public class ShipDatabase : ScriptableObject
{
    public List<ShipDefinition> allShips = new();

    public ShipDefinition GetRandomShip()
    {
        if (allShips.Count == 0) return null;

        int index = Random.Range(0, allShips.Count);
        return allShips[index];
    }
}