using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

public class ShopManager : MonoBehaviour
{
    [SerializeField] private ShipDatabase shipDatabase;
    [SerializeField] private DockingBayManager dockingBayManager;
    [SerializeField] private CreditsManager creditsManager;
    [SerializeField] private GameObject hexGridRoot;
    [SerializeField] private GameObject shopRoot;

    [Header("Sell")]
    [SerializeField] private RectTransform shopDropArea;
    [SerializeField] private float sellValueMultiplier = 0.5f;
    [SerializeField] private GameObject shopOffersRoot;
    [SerializeField] private GameObject shopButtonsRoot;
    [SerializeField] private GameObject sellModeRoot;
    [SerializeField, Range(0f, 1f)]
    private float sellModeScreenYThreshold = 0.65f;

    [Header("UI")]
    [SerializeField] private Transform contentParent;
    [SerializeField] private ShopSlotUI shopSlotPrefab;
    [SerializeField] private Button rerollButton;
    [SerializeField] private Button lockButton;

    [Header("Shop Settings")]
    [SerializeField] private int offersPerShop = 3;
    [SerializeField] private int rerollCost = 2;

    private readonly List<ShopSlotUI> spawnedSlots = new();
    private readonly List<ShipDefinition> currentOffers = new();

    private bool isLocked;

    private void Start()
    {
        if (rerollButton != null)
            rerollButton.onClick.AddListener(RerollShop);

        if (lockButton != null)
            lockButton.onClick.AddListener(ToggleLock);

        SetSellMode(false);

        ShowShop();
    }

    public void GenerateShop()
    {
        ClearShop();

        if (!isLocked || currentOffers.Count == 0)
        {
            RollNewOffers();
        }

        SpawnCurrentOffers();
    }

    private void RollNewOffers()
    {
        currentOffers.Clear();

        for (int i = 0; i < offersPerShop; i++)
        {
            ShipDefinition randomShip = shipDatabase.GetRandomShip();
            currentOffers.Add(randomShip);
        }
    }

    private void SpawnCurrentOffers()
    {
        foreach (ShipDefinition offer in currentOffers)
        {
            ShopSlotUI slot = Instantiate(shopSlotPrefab, contentParent);
            slot.Setup(offer, this);
            spawnedSlots.Add(slot);
        }
    }

    public void RerollShop()
    {
        if (isLocked)
            return;

        if (!creditsManager.TrySpend(rerollCost))
            return;

        RollNewOffers();

        ClearShop();
        SpawnCurrentOffers();
    }

    public void ToggleLock()
    {
        isLocked = !isLocked;

        // Optional later: update lock button text/icon here.
        Debug.Log(isLocked ? "Shop locked" : "Shop unlocked");
    }

    public bool TryBuyShip(ShipDefinition ship)
    {
        if (ship == null)
            return false;

        if (!creditsManager.TrySpend(ship.shopCost))
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

    public void ShowShop()
    {
        if (shopRoot != null)
            shopRoot.SetActive(true);

        if (hexGridRoot != null)
            hexGridRoot.SetActive(false);

        GenerateShop();
    }

    public void HideShop()
    {
        if (shopRoot != null)
            shopRoot.SetActive(false);

        if (hexGridRoot != null)
            hexGridRoot.SetActive(true);
    }

    public bool IsPointerOverDockingBay(Vector2 screenPosition)
    {
        return dockingBayManager != null &&
               dockingBayManager.IsPointerOverDockingBayArea(screenPosition);
    }

    public bool IsPointerOverShop(Vector2 screenPosition)
    {
        if (shopDropArea == null) return false;

        return RectTransformUtility.RectangleContainsScreenPoint(
            shopDropArea,
            screenPosition
        );
    }

    public bool TrySellDockedShip(DockingBaySlot slot)
    {
        if (slot == null || !slot.HasShip)
            return false;

        int sellValue = Mathf.Max(
     1,
     Mathf.FloorToInt(slot.ShipDefinition.shopCost * sellValueMultiplier)
 );

        creditsManager.AddCredits(sellValue);
        dockingBayManager.RemoveDockedShipAt(slot.SlotIndex);

        return true;
    }

    public void SetSellMode(bool state)
    {
        if (shopOffersRoot != null)
            shopOffersRoot.SetActive(!state);

        if (shopButtonsRoot != null)
            shopButtonsRoot.SetActive(!state);

        if (sellModeRoot != null)
            sellModeRoot.SetActive(state);
    }

    public bool TryBuyShipIntoDockingSlot(ShipDefinition ship, DockingBaySlot targetSlot)
    {
        if (ship == null || targetSlot == null)
            return false;

        if (!creditsManager.TrySpend(ship.shopCost))
            return false;

        bool mergedOrAdded = dockingBayManager.TryAddOrMergeShopShip(
            new DockedShipData(ship, 1),
            targetSlot
        );

        if (!mergedOrAdded)
        {
            creditsManager.AddCredits(ship.shopCost);
            return false;
        }

        return true;
    }

    public DockingBaySlot GetDockingBaySlotUnderMouse(Vector2 screenPosition)
    {
        return dockingBayManager.GetSlotUnderScreenPosition(screenPosition);
    }

    public bool ShouldActivateSellMode(Vector2 screenPosition)
    {
        return screenPosition.y >= Screen.height * sellModeScreenYThreshold;
    }
}