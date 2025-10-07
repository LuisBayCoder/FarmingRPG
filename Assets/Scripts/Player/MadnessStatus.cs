using UnityEngine;

public class MadnessStatus : MonoBehaviour
{
    [SerializeField] private StatusBar madnessBar;  // Reference to your StatusBar script
    [SerializeField] private int maxMadness = 100;
    [SerializeField] private int currentMadness = 0;
    [SerializeField] private int madnessIncrease = 5; // How much madness increases per press

    void Start()
    {
        if (madnessBar != null)
            madnessBar.Set(currentMadness, maxMadness);
    }

    void Update()
    {
        // Press M to increase madness
        if (Input.GetKeyDown(KeyCode.M))
        {
            AddMadness(madnessIncrease);
        }
        // Press N to decrease madness
        if (Input.GetKeyDown(KeyCode.N))
        {
            AddMadness(-madnessIncrease);
        }
    }

    void AddMadness(int amount)
    {
        currentMadness += amount;
        currentMadness = Mathf.Clamp(currentMadness, 0, maxMadness);

        if (madnessBar != null)
            madnessBar.Set(currentMadness, maxMadness);
    }
}

