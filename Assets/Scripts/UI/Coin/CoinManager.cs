using UnityEngine;
using System;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    public int Coins { get; private set; }

    // Event to notify when coins change
    public event Action<int> OnCoinsChanged;

    private void Awake()
    {
        if (Instance == null)
        {
            Instance = this;
        }
        else
        {
            Destroy(gameObject);
        }
    }

    public void AddCoins(int amount)
    {
        Coins += amount;
        OnCoinsChanged?.Invoke(Coins); // Trigger the event
    }

    public bool SpendCoins(int amount)
    {
        if (Coins >= amount)
        {
            Coins -= amount;
            OnCoinsChanged?.Invoke(Coins); // Trigger the event
            return true;
        }
        else
        {
            Debug.Log("Not enough coins!");
            return false;
        }
    }
}
