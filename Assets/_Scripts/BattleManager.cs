using UnityEngine;

public class BattleManager : MonoBehaviour
{
    public static bool IsBattleActive { get; private set; }

    [SerializeField] private GamePhaseManager phaseManager;
    [SerializeField] private EnemyFleetSpawner enemyFleetSpawner;
    [SerializeField] private CreditsManager creditsManager;
    [SerializeField] private int winReward = 5;

    public void StartBattle()
    {
        enemyFleetSpawner.SpawnEnemies();

        RestorePlayerShips();

        IsBattleActive = true;

        AddCombatControllersToShips();

        Debug.Log("Battle started");
    }

    private void Update()
    {
        if (!IsBattleActive)
            return;

        CheckBattleEnd();
    }

    private void AddCombatControllersToShips()
    {
        PlacedShip[] ships = FindObjectsByType<PlacedShip>(FindObjectsSortMode.None);

        foreach (PlacedShip ship in ships)
        {
            if (ship.GetComponent<ShipCombatController>() == null)
                ship.gameObject.AddComponent<ShipCombatController>();
        }
    }

    private void CheckBattleEnd()
    {
        bool playerAlive = false;
        bool enemyAlive = false;

        PlacedShip[] ships = FindObjectsByType<PlacedShip>(FindObjectsSortMode.None);

        foreach (PlacedShip ship in ships)
        {
            if (ship.Team == ShipTeam.Player)
                playerAlive = true;

            if (ship.Team == ShipTeam.Enemy)
                enemyAlive = true;
        }

        if (playerAlive && enemyAlive)
            return;

        IsBattleActive = false;

        if (playerAlive)
        {
            Debug.Log("Player wins!");
            creditsManager.AddCredits(winReward);
        }
        else
        {
            Debug.Log("Player loses!");
        }

        RestorePlayerShips();
        phaseManager.OnBattleFinished();
    }

    private void RestorePlayerShips()
    {
        PlacedShip[] ships = FindObjectsByType<PlacedShip>(FindObjectsSortMode.None);

        foreach (PlacedShip ship in ships)
        {
            if (ship.Team == ShipTeam.Player)
                ship.RestoreAfterBattle();
        }
    }
}