
using UnityEngine;
public class StoreStanding : MonoBehaviour
{
    public static StoreStanding Instance; // Singleton pattern

    public int standing = 0; // Player's standing with the store (can be negative or positive)

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

    // Adjust the player's standing
    public void AdjustStanding(int amount)
    {
        standing += amount;
        standing = Mathf.Clamp(standing, -100, 100); // Clamp standing between -100 and 100
    }

    // Calculate the markup/discount percentage based on standing
    public float GetPriceMultiplier()
    {
        // Example: Standing affects the multiplier linearly
        // At standing = 0, multiplier = 1.5 (50% markup for buying, 0.5 for selling)
        // At standing = 100, multiplier = 1.0 (no markup for buying, 1.0 for selling)
        // At standing = -100, multiplier = 2.0 (100% markup for buying, 0.0 for selling)
        return 1.5f - (standing / 200f);
    }
}