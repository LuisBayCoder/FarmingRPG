using UnityEngine;
using PixelCrushers.DialogueSystem; // Required namespace

public class CoinLuaBridge : MonoBehaviour
{
    private void OnEnable()
    {
        // Register the function when the script turns on
        Lua.RegisterFunction("RemoveCoins", this, SymbolExtensions.GetMethodInfo(() => RemoveCoins(System.Convert.ToDouble(0))));
    }

    private void OnDisable()
    {
        // Unregister to prevent memory leaks when changing scenes
        Lua.UnregisterFunction("RemoveCoins");
    }

    // Your actual C# method that handles inventory
    public void RemoveCoins(double amount)
    {
        // Lua handles numbers as doubles, cast it to int for your system
        int coinsToRemove = (int)amount;
        if (CoinManager.Instance != null)
        {
            bool success = CoinManager.Instance.SpendCoins(coinsToRemove);
            Debug.Log(success
                ? $"Removed {coinsToRemove} coins from the player."
                : $"Failed to remove {coinsToRemove} coins (not enough coins).");
        }
        else
        {
            Debug.LogWarning("CoinManager.Instance is null. Cannot remove coins.");
        }
    }
}
