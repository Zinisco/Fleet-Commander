using UnityEngine;

public class ShipCombatController : MonoBehaviour
{
    private PlacedShip ship;
    private float attackTimer;

    private void Awake()
    {
        ship = GetComponent<PlacedShip>();
    }

    private void Update()
    {
        if (BattleManager.IsBattleActive == false)
            return;

        if (ship == null || ship.IsDead)
            return;

        attackTimer -= Time.deltaTime;

        if (attackTimer > 0f)
            return;

        PlacedShip target = FindNearestEnemy();

        if (target == null)
            return;

        float distance = Vector3.Distance(transform.position, target.transform.position);

        if (distance > ship.CurrentRange)
            return;

        target.TakeDamage(ship.CurrentDamage);

        Debug.DrawLine(transform.position, target.transform.position, Color.red, 0.25f);

        attackTimer = 1f / ship.CurrentFireRate;
    }

    private PlacedShip FindNearestEnemy()
    {
        PlacedShip[] ships = FindObjectsByType<PlacedShip>(FindObjectsSortMode.None);

        PlacedShip closest = null;
        float closestDistance = Mathf.Infinity;

        foreach (PlacedShip other in ships)
        {
            if (other == null || other == ship)
                continue;

            if (other.Team == ship.Team)
                continue;

            if (other.IsDead)
                continue;

            float distance = Vector3.Distance(transform.position, other.transform.position);

            if (distance < closestDistance)
            {
                closestDistance = distance;
                closest = other;
            }
        }

        return closest;
    }
}