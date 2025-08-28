using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class ResizeColliderOfObject : MonoBehaviour
{
    [SerializeField] GameObject[] objectsToResizeCollider;
    private Dictionary<GameObject, ColliderData> originalColliderDataMap = new Dictionary<GameObject, ColliderData>();

    [System.Serializable]
    private struct ColliderData
    {
        public Vector2 size;
        public Vector2 offset;
        public Vector3 position;
    }

    void Start()
    {
        // Store the original collider data for all objects
        StoreOriginalColliderData();
        
        // Wait a frame then find and resize new objects
        StartCoroutine(WaitAndResizeNewObjects());
    }
    
    void StoreOriginalColliderData()
    {
        foreach (GameObject obj in objectsToResizeCollider)
        {
            if (obj != null)
            {
                BoxCollider2D boxCollider = obj.GetComponent<BoxCollider2D>();
                if (boxCollider != null)
                {
                    ColliderData data = new ColliderData
                    {
                        size = boxCollider.size,
                        offset = boxCollider.offset,
                        position = obj.transform.position
                    };
                    originalColliderDataMap[obj] = data;
                    Debug.Log($"Stored original collider data for {obj.name}: size: {data.size}, offset: {data.offset}");
                }
            }
        }
    }
    
    IEnumerator WaitAndResizeNewObjects()
    {
        // Wait 0.6 seconds for the new objects to appear
        yield return new WaitForSeconds(0.6f);
        
        // Find and resize new objects for each original object
        foreach (var kvp in originalColliderDataMap)
        {
            GameObject originalObject = kvp.Key;
            ColliderData originalData = kvp.Value;
            
            GameObject newObject = FindObjectAtPosition(originalData.position, originalObject);
            Debug.Log($"Found new object: {newObject?.name} at position: {originalData.position}");
            
            if (newObject != null)
            {
                ApplyOriginalSizeToNewObject(newObject, originalData);
            }
        }
    }
    
    GameObject FindObjectAtPosition(Vector3 position, GameObject excludeObject)
    {
        Collider2D[] colliders = Physics2D.OverlapCircleAll(position, 0.1f);
        foreach (Collider2D col in colliders)
        {
            // Check if it's not the original object and has an Item script
            if (col.gameObject != excludeObject && col.gameObject.GetComponent<Item>() != null)
            {
                return col.gameObject;
            }
        }
        return null;
    }
    
    void ApplyOriginalSizeToNewObject(GameObject newObject, ColliderData originalData)
    {
        BoxCollider2D boxCollider = newObject.GetComponent<BoxCollider2D>();
        if (boxCollider != null)
        {
            boxCollider.size = originalData.size;
            boxCollider.offset = originalData.offset;
            Debug.Log($"Applied original size {originalData.size} and offset {originalData.offset} to new object: {newObject.name}");
        }
    }
}
