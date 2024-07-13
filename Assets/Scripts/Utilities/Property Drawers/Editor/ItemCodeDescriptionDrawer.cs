
using System.Collections.Generic;
using UnityEngine;
using UnityEditor;

[CustomPropertyDrawer(typeof(ItemCodeDescriptionAttribute))]
public class ItemCodeDescriptionDrawer : PropertyDrawer
{
    public override float GetPropertyHeight(SerializedProperty property, GUIContent label)
    {
        // Change the returned property height to be double to cater for the additional item code description that we will draw
        return EditorGUI.GetPropertyHeight(property) * 2;
    }

    public override void OnGUI(Rect position, SerializedProperty property, GUIContent label)
    {
        // Using BeginProperty / EndProperty on the parent property means that prefab override logic works on the entire property.
        EditorGUI.BeginProperty(position, label, property);

        if (property.propertyType == SerializedPropertyType.Integer)
        {
            EditorGUI.BeginChangeCheck(); // Start of check for changed values

            // Draw item code
            var newValue = EditorGUI.IntField(new Rect(position.x, position.y, position.width, position.height / 2), label, property.intValue);

            // Draw item description
            EditorGUI.LabelField(new Rect(position.x, position.y + position.height / 2, position.width, position.height / 2), "Item Description", GetItemDescription(property.intValue));

            // If item code value has changed, then set value to new value
            if (EditorGUI.EndChangeCheck())
            {
                property.intValue = newValue;
            }
        }

        EditorGUI.EndProperty();
    }

    private string GetItemDescription(int itemCode)
    {
        string assetPath = "Assets/Scriptable Object Assets/Item/so_ItemList.asset";
        SO_ItemList so_itemList = AssetDatabase.LoadAssetAtPath<SO_ItemList>(assetPath);

        if (so_itemList == null)
        {
            Debug.LogError($"SO_ItemList asset not found at the specified path: {assetPath}");

            // Attempt to locate the asset dynamically
            string[] guids = AssetDatabase.FindAssets("t:SO_ItemList");
            if (guids.Length > 0)
            {
                string path = AssetDatabase.GUIDToAssetPath(guids[0]);
                so_itemList = AssetDatabase.LoadAssetAtPath<SO_ItemList>(path);
                Debug.Log($"SO_ItemList asset found at: {path}");
            }

            if (so_itemList == null)
            {
                Debug.LogError("SO_ItemList asset could not be found dynamically either.");
                return "Item list not found.";
            }
        }

        List<ItemDetails> itemDetailsList = so_itemList.itemDetails;

        if (itemDetailsList == null)
        {
            Debug.LogError("ItemDetails list is null in SO_ItemList.");
            return "Item details not available.";
        }

        ItemDetails itemDetail = itemDetailsList.Find(x => x.itemCode == itemCode);

        if (itemDetail != null)
        {
            // Debug log for new fields
            Debug.Log($"ItemCode: {itemDetail.itemCode}, IsWeapon: {itemDetail.isWeapon}, Damage: {itemDetail.damageAmount}");
            return itemDetail.itemDescription;
        }
        else
        {
            return "Item not found.";
        }
    }
}