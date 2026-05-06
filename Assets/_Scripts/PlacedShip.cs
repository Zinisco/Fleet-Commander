using UnityEngine;

public class PlacedShip : MonoBehaviour
{
    public ShipDefinition ShipDefinition { get; private set; }
    public HexCell CurrentCell { get; private set; }

    public int Level { get; private set; } = 1;

    public int CurrentDamage { get; private set; }
    public float CurrentFireRate { get; private set; }
    public float CurrentRange { get; private set; }
    public int CurrentMaxHealth { get; private set; }

    [Header("Visual Feedback")]
    [SerializeField] private GameObject bonusVisual;

    public bool HasActiveBonus { get; private set; }

    public string DisplayName =>
        Level <= 1 ? ShipDefinition.shipName : $"{ShipDefinition.shipName} {ToRoman(Level)}";

    public void Init(ShipDefinition definition, HexCell cell, int level = 1)
    {
        ShipDefinition = definition;
        CurrentCell = cell;
        Level = Mathf.Max(1, level);

        ResetBonusStats();
    }

    public void SetCurrentCell(HexCell cell)
    {
        CurrentCell = cell;
    }

    public void LevelUp()
    {
        Level++;
        ResetBonusStats();
    }

    public void ResetBonusStats()
    {
        CurrentDamage = ShipDefinition.damage * Level;
        CurrentFireRate = ShipDefinition.fireRate;
        CurrentRange = ShipDefinition.range;
        CurrentMaxHealth = ShipDefinition.maxHealth * Level;

        HasActiveBonus = false;
        UpdateBonusVisual();
    }

    public void ApplyBonus(ShipDefinition.BonusType bonusType, int amount)
    {
        if (bonusType == ShipDefinition.BonusType.None)
            return;

        HasActiveBonus = true;

        switch (bonusType)
        {
            case ShipDefinition.BonusType.Attack:
                CurrentDamage += amount;
                break;

            case ShipDefinition.BonusType.Health:
                CurrentMaxHealth += amount;
                break;

            case ShipDefinition.BonusType.FireRate:
                CurrentFireRate += amount;
                break;

            case ShipDefinition.BonusType.Range:
                CurrentRange += amount;
                break;
        }

        UpdateBonusVisual();
    }

    private void UpdateBonusVisual()
    {
        if (bonusVisual != null)
            bonusVisual.SetActive(HasActiveBonus);
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