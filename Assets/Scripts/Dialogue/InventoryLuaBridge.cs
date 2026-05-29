using UnityEngine;
using PixelCrushers.DialogueSystem; // Required namespace

public class InventoryLuaBridge : MonoBehaviour
{
    private void OnEnable()
    {
        // Register the "GiveItem" function to Lua
        Lua.RegisterFunction("GiveItem", this, SymbolExtensions.GetMethodInfo(() => GiveItem(string.Empty, System.Convert.ToDouble(0))));
    }

    private void OnDisable()
    {
        // Clean up registration
        Lua.UnregisterFunction("GiveItem");
    }

    // The method Lua will trigger. 
    // Remember: Lua numbers must be 'double' in C# parameters.
    public void GiveItem(string itemName, double amount)
    {
        int quantity = (int)amount;
        Debug.Log($"Dialogue System gave: {quantity}x {itemName}");

        // Find the item in the item list by name (itemDescription)
        var itemList = InventoryManager.Instance.GetType()
            .GetField("itemList", System.Reflection.BindingFlags.NonPublic | System.Reflection.BindingFlags.Instance)
            ?.GetValue(InventoryManager.Instance) as SO_ItemList;
        if (itemList == null)
        {
            Debug.LogError("Item list not found in InventoryManager.");
            return;
        }

        var itemDetails = itemList.itemDetails.Find(item => item.itemDescription == itemName);
        if (itemDetails == null)
        {
            Debug.LogError($"Item with name '{itemName}' not found in item list.");
            return;
        }

        for (int i = 0; i < quantity; i++)
        {
            InventoryManager.Instance.AddItem(InventoryLocation.keyring, itemDetails.itemCode);
        }
    }
}
