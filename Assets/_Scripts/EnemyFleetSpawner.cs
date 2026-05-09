using System.Collections.Generic;
using UnityEngine;

public class EnemyFleetSpawner : MonoBehaviour
{
    [SerializeField] private HexGrid hexGrid;
    [SerializeField] private ShipPlacementManager placementManager;

    [Header("Enemy Test Fleet")]
    [SerializeField] private List<ShipDefinition> enemyShips = new();

    [Header("Spawn Cells")]
    [SerializeField]
    private List<Vector2Int> spawnCoords = new()
    {
        new Vector2Int(2, 3),
        new Vector2Int(1, 3),
        new Vector2Int(3, 3)
    };

    public void SpawnEnemies()
    {
        ClearExistingEnemies();

        for (int i = 0; i < enemyShips.Count && i < spawnCoords.Count; i++)
        {
            Vector2Int coord = spawnCoords[i];

            if (!hexGrid.TryGetCell(coord.x, coord.y, out HexCell cell))
                continue;

            if (cell.isOccupied)
                continue;

            placementManager.TryPlaceShip(
                cell,
                enemyShips[i],
                1,
                ShipTeam.Enemy
            );
        }
    }

    public void ClearExistingEnemies()
    {
        PlacedShip[] ships = FindObjectsByType<PlacedShip>(FindObjectsSortMode.None);

        foreach (PlacedShip ship in ships)
        {
            if (ship.Team != ShipTeam.Enemy)
                continue;

            if (ship.CurrentCell != null)
                ship.CurrentCell.Clear();

            Destroy(ship.gameObject);
        }
    }
}