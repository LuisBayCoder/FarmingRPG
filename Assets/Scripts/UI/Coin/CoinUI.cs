using UnityEngine;
using TMPro;

public class CoinUI : MonoBehaviour
{
    public TextMeshProUGUI coinText; // Reference to the UI text element

    private void Start()
    {
        // Subscribe to the event
        CoinManager.Instance.OnCoinsChanged += UpdateCoinDisplay;
        // Initialize the display with the current coin balance
        UpdateCoinDisplay(CoinManager.Instance.Coins);
    }

    private void OnDestroy()
    {
        // Unsubscribe to avoid memory leaks
        CoinManager.Instance.OnCoinsChanged -= UpdateCoinDisplay;
    }

    // Update the coin display
    private void UpdateCoinDisplay(int coins)
    {
        coinText.text = coins.ToString();
    }
}
