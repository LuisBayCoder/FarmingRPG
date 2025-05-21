using UnityEngine;

public class SkullPedestal : MonoBehaviour //, IInteractable
{
    public Item requiredSkull; // Assigned via Inspector
    public SpriteRenderer placedSkullRenderer;
    private bool isCorrectlyPlaced = false;
    
    public Transform itemPosition;

    public void Interact()
    {


    }

    public bool IsCorrect() => isCorrectlyPlaced;
}

