using System.Collections.Generic;
using UnityEngine;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private ShipDatabase shipDatabase;
    [SerializeField] private DockingBayManager dockingBayManager;
    [SerializeField] private CreditsManager creditsManager;

    [Header("UI")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private ShopSlotUI shopSlotPrefab;

    [SerializeField] private int offersPerShop = 3;

    private readonly List<ShopSlotUI> spawnedSlots = new();

    private void Start()
    {
        GenerateShop();
    }

    public void GenerateShop()
    {
        ClearShop();

        for (int i = 0; i < offersPerShop; i++)
        {
            ShipDefinition randomShip =
                shipDatabase.GetRandomShip();

            ShopSlotUI slot =
                Instantiate(shopSlotPrefab, contentParent);

            slot.Setup(randomShip, this);

            spawnedSlots.Add(slot);
        }
    }

    public bool TryBuyShip(ShipDefinition ship)
    {
        if (ship == null)
            return false;

        bool spent =
            creditsManager.TrySpend(ship.shopCost);

        if (!spent)
            return false;

        dockingBayManager.AddShipToDockingBay(
            new DockedShipData(ship, 1)
        );

        return true;
    }

    private void ClearShop()
    {
        foreach (ShopSlotUI slot in spawnedSlots)
        {
            if (slot != null)
                Destroy(slot.gameObject);
        }

        spawnedSlots.Clear();
    }
}