using TMPro;
using UnityEngine;
using UnityEngine.UI;

public enum GamePhase
{
    Shop,
    Planning,
    Battle,
    BattleComplete
}

public class GamePhaseManager : MonoBehaviour
{
    [Header("Managers")]
    [SerializeField] private ShopManager shopManager;
    [SerializeField] private BattleManager battleManager;

    [Header("UI")]
    [SerializeField] private Button phaseButton;
    [SerializeField] private TMP_Text phaseButtonText;

    private GamePhase currentPhase = GamePhase.Shop;

    private void Start()
    {
        phaseButton.onClick.AddListener(AdvancePhase);
        EnterShopPhase();
    }

    private void AdvancePhase()
    {
        switch (currentPhase)
        {
            case GamePhase.Shop:
                EnterPlanningPhase();
                break;

            case GamePhase.Planning:
                EnterBattlePhase();
                break;

            case GamePhase.BattleComplete:
                EnterShopPhase();
                break;
        }
    }

    public void EnterShopPhase()
    {
        currentPhase = GamePhase.Shop;

        shopManager.ShowShop();

        phaseButton.interactable = true;
        phaseButtonText.text = "Plan";
    }

    private void EnterPlanningPhase()
    {
        currentPhase = GamePhase.Planning;

        shopManager.HideShop();

        phaseButton.interactable = true;
        phaseButtonText.text = "Battle";
    }

    private void EnterBattlePhase()
    {
        currentPhase = GamePhase.Battle;

        shopManager.HideShop();

        phaseButton.interactable = false;
        phaseButton.gameObject.SetActive(false);

        battleManager.StartBattle();
    }

    public void OnBattleFinished()
    {
        currentPhase = GamePhase.BattleComplete;

        phaseButton.gameObject.SetActive(true);
        phaseButton.interactable = true;

        phaseButtonText.text = "Shop";
    }
}