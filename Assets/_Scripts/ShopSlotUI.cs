using TMPro;
using UnityEngine;
using UnityEngine.UI;

public class ShopSlotUI : MonoBehaviour
{
    [SerializeField] private Image icon;
    [SerializeField] private TMP_Text shipNameText;
    [SerializeField] private TMP_Text costText;
    [SerializeField] private Button buyButton;

    private ShipDefinition ship;
    private ShopManager shopManager;

    public void Setup(ShipDefinition definition, ShopManager manager)
    {
        ship = definition;
        shopManager = manager;

        icon.sprite = ship.icon;
        shipNameText.text = ship.shipName;
        costText.text = $"{ship.shopCost} Credits";

        buyButton.onClick.RemoveAllListeners();
        buyButton.onClick.AddListener(Buy);
    }

    private void Buy()
    {
        if (ship == null || shopManager == null)
            return;

        bool bought = shopManager.TryBuyShip(ship);

        if (bought)
        {
            buyButton.interactable = false;
            costText.text = "Purchased";
        }
    }
}