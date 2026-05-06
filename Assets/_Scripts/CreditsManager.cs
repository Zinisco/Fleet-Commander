using TMPro;
using UnityEngine;

public class CreditsManager : MonoBehaviour
{
    [SerializeField] private int startingCredits = 10;
    [SerializeField] private TMP_Text creditsText;

    public int Credits { get; private set; }

    private void Start()
    {
        Credits = startingCredits;
        RefreshUI();
    }

    public bool TrySpend(int amount)
    {
        if (Credits < amount)
            return false;

        Credits -= amount;
        RefreshUI();

        return true;
    }

    public void AddCredits(int amount)
    {
        Credits += amount;
        RefreshUI();
    }

    private void RefreshUI()
    {
        if (creditsText != null)
            creditsText.text = $"Credits: {Credits}";
    }
}