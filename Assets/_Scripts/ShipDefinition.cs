using UnityEngine;
using System.Collections.Generic;

[CreateAssetMenu(menuName = "Fleet Commander/Ship Definition")]
public class ShipDefinition : ScriptableObject
{
    [Header("Identity")]
    public string shipName;
    public Sprite icon;

    [Header("Prefab")]
    public GameObject shipPrefab;

    [Header("Shop")]
    public int shopCost = 3;

    [Header("Stats")]
    public int maxHealth = 10;
    public int damage = 1;
    public float fireRate = 1f;
    public float range = 5f;

    [Header("Directional Bonus")]
    public bool useDirectionalBonus;
    public List<HexDirection> validBonusDirections = new();

    public enum ShipRole
    {
        Vanguard,
        Striker,
        Relay
    }

    public ShipRole role;

    [Header("Adjacency Bonus")]
    public bool providesAdjacentBonus;

    public enum BonusType
    {
        None,
        Attack,
        Health,
        FireRate,
        Range
    }

    public BonusType bonusType;
    public int bonusAmount = 1;
}