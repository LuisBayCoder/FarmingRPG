using UnityEngine;
using System;

public class CoinManager : MonoBehaviour
{
    public static CoinManager Instance;

    public int Coins { get; private set; }

    // Event to notify when coins change
    public event Action<int> OnCoinsChanged;

    private PanelBlink PanelBlink;

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
        //coinUI.StopBlinking();
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
            PanelBlink = GameObject.Find("PanelBlink").GetComponent<PanelBlink>();
            if (PanelBlink != null)
            {
                PanelBlink.StartBlinking();
            }
            else
            {
                Debug.LogError("coinUI is not assigned or found!");
            }
            Debug.Log("Not enough coins!");
            return false;
        }
    }
}
