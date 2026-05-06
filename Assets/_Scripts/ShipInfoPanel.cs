using UnityEngine;
using TMPro;

public class ShipInfoPanel : MonoBehaviour
{
    [SerializeField] private RectTransform panelRect;

    private Canvas parentCanvas;

    [SerializeField] private GameObject root;
    [SerializeField] private TMP_Text nameText;
    [SerializeField] private TMP_Text roleText;
    [SerializeField] private TMP_Text statsText;
    [SerializeField] private TMP_Text bonusText;

    private void Awake()
    {
        parentCanvas = GetComponentInParent<Canvas>();

        if (panelRect == null && root != null)
            panelRect = root.GetComponent<RectTransform>();

        Hide();
    }

    private void OnDisable()
    {
        Debug.LogWarning("ShipInfoPanel was disabled by parent/object: " + gameObject.name);
    }

    public void Show(PlacedShip ship)
    {
        if (ship == null || ship.ShipDefinition == null)
        {
            Hide();
            return;
        }

        ShipDefinition def = ship.ShipDefinition;

        if (root != null)
        {
            root.SetActive(true);
            root.transform.SetAsLastSibling();
            Debug.Log("Panel active: " + root.activeSelf);
        }

        if (nameText != null)
            nameText.text = ship.DisplayName;

        if (roleText != null)
            roleText.text = $"Role: {def.role}";

        if (statsText != null)
        {
            statsText.text =
                $"Damage: {ship.CurrentDamage}\n" +
                $"Health: {ship.CurrentMaxHealth}\n" +
                $"Fire Rate: {ship.CurrentFireRate}\n" +
                $"Range: {ship.CurrentRange}";
        }

        if (bonusText != null)
        {
            if (!def.providesAdjacentBonus)
            {
                bonusText.text = "Bonus: None";
            }
            else
            {
                string directions = def.useDirectionalBonus
                    ? string.Join(", ", def.validBonusDirections)
                    : "All adjacent cells";

                bonusText.text =
                    $"Gives: +{def.bonusAmount} {def.bonusType}\n" +
                    $"To: {directions}";
            }
        }
    }

    public void Show(DockedShipData shipData)
    {
        if (shipData == null || shipData.definition == null)
        {
            Hide();
            return;
        }

        ShipDefinition def = shipData.definition;
        int level = Mathf.Max(1, shipData.level);

        if (root != null)
        {
            root.SetActive(true);
            root.transform.SetAsLastSibling();
            Debug.Log("Panel active: " + root.activeSelf);
        }

        if (nameText != null)
            nameText.text = level <= 1 ? def.shipName : $"{def.shipName} {ToRoman(level)}";

        if (roleText != null)
            roleText.text = $"Role: {def.role}";

        if (statsText != null)
        {
            statsText.text =
                $"Damage: {def.damage * level}\n" +
                $"Health: {def.maxHealth * level}\n" +
                $"Fire Rate: {def.fireRate}\n" +
                $"Range: {def.range}";
        }

        if (bonusText != null)
        {
            if (!def.providesAdjacentBonus)
            {
                bonusText.text = "Bonus: None";
            }
            else
            {
                string directions = def.useDirectionalBonus
                    ? string.Join(", ", def.validBonusDirections)
                    : "All adjacent cells";

                bonusText.text =
                    $"Gives: +{def.bonusAmount} {def.bonusType}\n" +
                    $"To: {directions}";
            }
        }
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

    public void Hide()
    {
        if (root != null)
            root.SetActive(false);
    }
}